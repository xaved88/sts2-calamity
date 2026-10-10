using Calamity.CalamityCode.Map;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Patches;

// A calamity room is a vanilla treasure room with hardships in the chest (players + 1 of them), no gold and no
// skipping. Awarding is untouched vanilla: everyone takes one, ties are rock-paper-scissors, losers are handed an
// unclaimed one, and the leftover is discarded. Reusing the treasure room gets us the layout, pointing hands,
// network sync and save/restore for free.

internal static class CalamityRoomState
{
    public static bool IsActive => RunManager.Instance.State is { } state && CalamityMapPoint.IsCurrent(state);
}

/// <summary>Fill the chest with hardships instead of rolling relics.</summary>
[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.BeginRelicPicking))]
internal static class BeginRelicPickingPatch
{
    private static bool Prefix(TreasureRoomRelicSynchronizer __instance)
    {
        if (!CalamityRoomState.IsActive)
        {
            return true;
        }
        if (__instance._currentRelics != null)
        {
            throw new InvalidOperationException("Attempted to start new relic picking session while one was already occurring!");
        }

        IReadOnlyList<Player> players = __instance._playerCollection.Players;
        __instance._currentRelics = CalamityRoom.PickHardships(players[0].RunState);
        __instance._votes.Clear();
        __instance._predictedVote = null;
        foreach (Player _ in players)
        {
            __instance._votes.Add(new TreasureRoomRelicSynchronizer.PlayerVote { voteReceived = false });
        }

        if (__instance._currentRelics.Count == 0)
        {
            __instance.EndRelicVoting();
            return false;
        }

        // Mirrors vanilla: in fake multiplayer (local testing), the other "players" vote at random.
        if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer && players.Count > 1)
        {
            foreach (Player player in players.Where(p => p != __instance.LocalPlayer))
            {
                var vote = __instance._votes[__instance._playerCollection.GetPlayerSlotIndex(player)];
                vote.index = __instance._rng.NextInt(__instance._currentRelics.Count);
                vote.voteReceived = true;
            }
        }
        // Field-like event: the backing delegate shares the event's name.
        (AccessTools.Field(typeof(TreasureRoomRelicSynchronizer), nameof(TreasureRoomRelicSynchronizer.VotesChanged)).GetValue(__instance) as Action)?.Invoke();
        return false;
    }
}

/// <summary>
/// The treasure scene has 4 multiplayer relic slots; a 4-player calamity offers 5. Add slots from the holder's own
/// scene and spread all of them evenly across the original row.
/// </summary>
[HarmonyPatch(typeof(NTreasureRoomRelicCollection), nameof(NTreasureRoomRelicCollection.InitializeRelics))]
internal static class ExtraRelicHoldersPatch
{
    private const string HolderScene = "res://scenes/ui/treasure_relic_holder.tscn";

    private static void Prefix(NTreasureRoomRelicCollection __instance)
    {
        int needed = RunManager.Instance.TreasureRoomRelicSynchronizer.CurrentRelics?.Count ?? 0;
        List<NTreasureRoomRelicHolder> holders = __instance._multiplayerHolders;
        if (!CalamityRoomState.IsActive || needed <= holders.Count || holders.Count == 0)
        {
            return;
        }

        Node parent = holders[0].GetParent();
        float minX = holders.Min(h => h.Position.X);
        float maxX = holders.Max(h => h.Position.X);
        float y = holders.Average(h => h.Position.Y);
        while (holders.Count < needed)
        {
            var holder = ResourceLoader.Load<PackedScene>(HolderScene).Instantiate<NTreasureRoomRelicHolder>();
            parent.AddChild(holder);
            holders.Add(holder);
        }
        for (int i = 0; i < holders.Count; i++)
        {
            holders[i].Position = new Vector2(minX + (maxX - minX) * i / (holders.Count - 1), y);
        }
    }
}

/// <summary>No gold from a calamity chest.</summary>
[HarmonyPatch(typeof(TreasureRoom), nameof(TreasureRoom.DoNormalRewards))]
internal static class CalamityNoGoldPatch
{
    private static bool Prefix(ref Task<int> __result)
    {
        if (!CalamityRoomState.IsActive)
        {
            return true;
        }
        __result = Task.FromResult(0);
        return false;
    }
}

[HarmonyPatch(typeof(TreasureRoom), nameof(TreasureRoom.DoExtraRewardsIfNeeded))]
internal static class CalamityNoExtraRewardsPatch
{
    private static bool Prefix(ref Task __result)
    {
        if (!CalamityRoomState.IsActive)
        {
            return true;
        }
        __result = Task.CompletedTask;
        return false;
    }
}

/// <summary>
/// In singleplayer, vanilla enables a Skip button 2.5s after the chest opens, and a solo skip takes nothing.
/// A hardship isn't optional, so never enable it; the button re-enables as Proceed once the pick resolves.
/// </summary>
[HarmonyPatch(typeof(NTreasureRoom), nameof(NTreasureRoom.EnableSkipAfterDelay))]
internal static class CalamityNoSkipPatch
{
    private static bool Prefix(ref Task __result)
    {
        if (!CalamityRoomState.IsActive)
        {
            return true;
        }
        __result = Task.CompletedTask;
        return false;
    }
}
