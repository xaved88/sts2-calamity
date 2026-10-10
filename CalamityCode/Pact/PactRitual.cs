using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Pact;

/// <summary>
/// The Pact: at A11 in multiplayer, a player who died is still revived at the end of combat (vanilla does that for
/// free), but someone alive pays with an Unholy Pact.
///
/// Who pays is one synced choice owned by the dead player, the same scheme vanilla's Mend uses:
/// * The dead player gets a targeting arrow and grasps a living ally.
/// * Any living ally can click "Offer your soul" instead; that sends a message to the dead player's game, which resolves
///   the choice with the volunteer. The dead player's game is the only one that decides, so two clicks can't race.
/// With only one living ally there's nothing to choose, so it binds them directly.
/// </summary>
public static class PactRitual
{
    /// <summary>Dead player NetId -> volunteer handler, on the dead player's own game while it's choosing.</summary>
    private static readonly Dictionary<ulong, Action<ulong>> PendingVolunteers = new();

    public static bool Applies(Player player) =>
        player.Creature.IsDead
        && player.RunState.Players.Count > 1
        && CalamityAscension.IsActive(player.RunState)
        && player.RunState.Players.Any(p => p != player && p.Creature.IsAlive);

    public static async Task ReviveWithPact(Player dead)
    {
        List<Player> living = dead.RunState.Players.Where(p => p != dead && p.Creature.IsAlive).ToList();
        Player bearer = living.Count == 1 ? living[0] : await ChooseBearer(dead, living);

        MainFile.Logger.Info($"Pact: {dead.NetId} is pulled back by {bearer.NetId}.");
        await CreatureCmd.Heal(dead.Creature, 1m);
        await UnholyPact.Bind(bearer);
    }

    private static async Task<Player> ChooseBearer(Player dead, List<Player> living)
    {
        PlayerChoiceSynchronizer sync = RunManager.Instance.PlayerChoiceSynchronizer;
        uint choiceId = sync.ReserveChoiceId(dead);

        if (!LocalContext.IsMe(dead))
        {
            using var prompt = PactPrompt.ShowForAlly(dead, living);
            ulong? remote = (await sync.WaitForRemoteChoice(dead, choiceId)).AsPlayerId();
            return living.FirstOrDefault(p => p.NetId == remote) ?? Fallback(living);
        }

        Player? chosen = null;
        using (PactPrompt.ShowForDead(dead))
        {
            NTargetManager targeting = NTargetManager.Instance;
            PendingVolunteers[dead.NetId] = volunteerId =>
            {
                chosen ??= living.FirstOrDefault(p => p.NetId == volunteerId);
                if (chosen != null && targeting.IsInSelection)
                {
                    targeting.CancelTargeting();
                }
            };
            try
            {
                // Re-open the arrow if it's cancelled (right-click, Esc) without a choice or a volunteer.
                while (chosen == null)
                {
                    targeting.StartTargeting(TargetType.AnyPlayer, ArrowStart(dead), TargetMode.ClickMouseToTarget, null,
                        node => NodeToPlayer(node) is { } p && living.Contains(p));
                    chosen ??= NodeToPlayer(await targeting.SelectionFinished()) is { } p && living.Contains(p) ? p : null;
                }
            }
            finally
            {
                PendingVolunteers.Remove(dead.NetId);
            }
        }
        sync.SyncLocalChoice(dead, choiceId, PlayerChoiceResult.FromPlayerId(chosen.NetId));
        return chosen;
    }

    /// <summary>Called on every game when someone volunteers; only the dead player's game acts on it.</summary>
    internal static void OnVolunteer(ulong deadPlayerId, ulong volunteerId)
    {
        if (PendingVolunteers.TryGetValue(deadPlayerId, out Action<ulong>? handler))
        {
            handler(volunteerId);
        }
    }

    // Lowest Pact first, so a desync fallback (should never happen) is at least fair.
    private static Player Fallback(List<Player> living) =>
        living.OrderBy(p => p.Deck.Cards.OfType<UnholyPact>().Sum(c => c.Severity)).First();

    private static Vector2 ArrowStart(Player dead) =>
        NCombatRoom.Instance?.GetCreatureNode(dead.Creature)?.GlobalPosition
        ?? NGame.Instance.GetViewport().GetVisibleRect().Size / 2f;

    private static Player? NodeToPlayer(Node? node) => node switch
    {
        NCreature creature => creature.Entity.Player,
        NMultiplayerPlayerState state => state.Player,
        _ => null,
    };
}

/// <summary>Vanilla revives dead players at 1 HP right before combat ends; at A11 that revive costs a Pact.</summary>
[HarmonyPatch(typeof(Player), nameof(Player.ReviveBeforeCombatEnd))]
internal static class ReviveBeforeCombatEndPatch
{
    private static bool Prefix(Player __instance, ref Task __result)
    {
        if (!PactRitual.Applies(__instance))
        {
            return true;
        }
        __result = PactRitual.ReviveWithPact(__instance);
        return false;
    }
}
