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

    // Hooks run on every game; each game only fogs its own local player's hand.
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

/// <summary>Local-only presentation: card backs over the hand, no hover tips, deck button disabled.</summary>
internal static class HandFog
{
    private const string CardBack = "res://images/packed/run_history/card_back.png";
    private const string OverlayName = "CalamityFog";

    public static bool IsUp { get; private set; }

    public static void Raise()
    {
        IsUp = true;
        Texture2D? back = ResourceLoader.Load<Texture2D>(CardBack, null, ResourceLoader.CacheMode.Reuse);
        foreach (NHandCardHolder holder in NPlayerHand.Instance?.ActiveHolders ?? [])
        {
            Cover(holder.CardNode, back);
        }
        NRun.Instance?.GlobalUi.TopBar.Deck.Disable();
    }

    public static void Lift()
    {
        if (!IsUp)
        {
            return;
        }
        IsUp = false;
        foreach (NHandCardHolder holder in NPlayerHand.Instance?.Holders ?? [])
        {
            holder.CardNode?.GetNodeOrNull(OverlayName)?.QueueFree();
        }
        NRun.Instance?.GlobalUi.TopBar.Deck.Enable();
    }

    internal static void Cover(NCard? card, Texture2D? back)
    {
        if (card == null || card.GetNodeOrNull(OverlayName) != null)
        {
            return;
        }
        // NCard draws centred on its origin, at NCard.defaultSize.
        Vector2 size = NCard.defaultSize;
        Control overlay = back != null
            ? new TextureRect { Texture = back, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.Scale }
            : new ColorRect { Color = new Color(0.12f, 0.1f, 0.16f) };
        overlay.Name = OverlayName;
        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        overlay.Size = size;
        overlay.Position = -size / 2f;
        overlay.ZIndex = 10;
        card.AddChild(overlay);
    }
}

/// <summary>No keyword tooltips while the hand is hidden; they'd give the cards away.</summary>
// Declared (protected virtual) on NCardHolder; only hand holders are affected because the fog only exists then.
[HarmonyPatch(typeof(NCardHolder), "CreateHoverTips")]
internal static class HandFogHoverTipsPatch
{
    private static bool Prefix(NCardHolder __instance) => !(HandFog.IsUp && __instance is NHandCardHolder);
}
