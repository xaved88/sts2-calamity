using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;

namespace Calamity.CalamityCode.Hardships.Act2;

/// <summary>
/// Every player starts each combat Fogbound: their hand is face-down until they play a card, and the deck can't be
/// opened (the draw pile can, so if you know your deck you can guess). Innate cards are the counterplay: you know
/// they're in there. Party-wide; the fog is purely what each player's own screen shows, so nothing needs syncing.
/// </summary>
public sealed class FogboundLantern : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<FogboundPower>()];

    protected override async Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        List<Creature> players = combatState.PlayerCreatures.Where(c => c.IsAlive && c.GetPower<FogboundPower>() == null).ToList();
        if (players.Count == 0)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<FogboundPower>(choiceContext, players, 1m, Owner.Creature, null);
    }

    // Safety net: never leave the local hand covered after a fight.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        HandFog.Lift();
        return Task.CompletedTask;
    }
}

/// <summary>Hand hidden until this player plays a card; then the power is removed.</summary>
public sealed class FogboundPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    // Hooks run on every game; each game only fogs its own local player's hand. Armed before the turn-1 draw so cards
    // are face-down while they fly in, then everything already in hand is covered once the turn has started.
    public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player.Creature == Owner && LocalContext.IsMe(player))
        {
            HandFog.Arm();
        }
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner && LocalContext.IsMe(player))
        {
            HandFog.Raise();
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }
        if (LocalContext.IsMe(cardPlay.Card.Owner))
        {
            HandFog.Lift();
        }
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Local-only presentation: hand cards show a card back, keyword tooltips are off, and the deck button is disabled.
/// A covered card hides its whole face (the card's Body) rather than laying something over it, so nothing peeks out.
/// </summary>
internal static class HandFog
{
    private const string CardBack = "res://images/packed/run_history/card_back.png";
    private const string OverlayName = "CalamityFog";

    private static readonly List<WeakReference<NCard>> Covered = [];

    public static bool IsUp { get; private set; }

    /// <summary>From now on, every card heading into the local hand is face-down.</summary>
    public static void Arm()
    {
        IsUp = true;
        NRun.Instance?.GlobalUi.TopBar.Deck.Disable();
    }

    /// <summary>Arm, and cover whatever is already in hand.</summary>
    public static void Raise()
    {
        Arm();
        foreach (NHandCardHolder holder in NPlayerHand.Instance?.ActiveHolders ?? [])
        {
            Cover(holder.CardNode);
        }
    }

    public static void Lift()
    {
        if (!IsUp)
        {
            return;
        }
        IsUp = false;
        foreach (WeakReference<NCard> weak in Covered)
        {
            if (weak.TryGetTarget(out NCard? card) && GodotObject.IsInstanceValid(card))
            {
                Uncover(card);
            }
        }
        Covered.Clear();
        NRun.Instance?.GlobalUi.TopBar.Deck.Enable();
    }

    internal static void Cover(NCard? card)
    {
        if (card == null || !GodotObject.IsInstanceValid(card) || card.GetNodeOrNull(OverlayName) != null)
        {
            return;
        }
        Texture2D? back = ResourceLoader.Load<Texture2D>(CardBack, null, ResourceLoader.CacheMode.Reuse);
        // NCard draws centred on its origin, at NCard.defaultSize.
        Vector2 size = NCard.defaultSize;
        Control overlay = back != null
            ? new TextureRect { Texture = back, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.Scale }
            : new ColorRect { Color = new Color(0.12f, 0.1f, 0.16f) };
        overlay.Name = OverlayName;
        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        overlay.Size = size;
        overlay.Position = -size / 2f;
        card.Body.Visible = false;
        card.AddChild(overlay);
        Covered.Add(new WeakReference<NCard>(card));
    }

    internal static void Uncover(NCard card)
    {
        if (card.GetNodeOrNull(OverlayName) is not { } overlay)
        {
            return;
        }
        card.RemoveChild(overlay);
        overlay.QueueFree();
        card.Body.Visible = true;
    }
}

/// <summary>
/// Cards get their hand visuals (UpdateVisuals(Hand)) before they fly in from the draw pile, so covering here hides
/// them for the whole draw. A card leaving the hand (played, discarded) turns face-up again.
/// </summary>
[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
internal static class HandFogUpdateVisualsPatch
{
    private static void Postfix(NCard __instance, PileType pileType)
    {
        if (pileType == PileType.Hand && HandFog.IsUp && __instance.Model is { } model && LocalContext.IsMine(model))
        {
            HandFog.Cover(__instance);
        }
        else if (pileType != PileType.Hand)
        {
            HandFog.Uncover(__instance);
        }
    }
}

/// <summary>Card nodes are pooled: never let a cover survive into a reused node.</summary>
[HarmonyPatch(typeof(NCard), nameof(NCard.OnReturnedFromPool))]
internal static class HandFogPoolPatch
{
    private static void Postfix(NCard __instance) => HandFog.Uncover(__instance);
}

/// <summary>No keyword tooltips while the hand is hidden; they'd give the cards away.</summary>
// Declared (protected virtual) on NCardHolder; only hand holders are affected because the fog only exists then.
[HarmonyPatch(typeof(NCardHolder), "CreateHoverTips")]
internal static class HandFogHoverTipsPatch
{
    private static bool Prefix(NCardHolder __instance) => !(HandFog.IsUp && __instance is NHandCardHolder);
}
