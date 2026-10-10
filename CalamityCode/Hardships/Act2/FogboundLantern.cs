using Godot;
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
/// On each player's first turn of a combat, their hand is face-down until they play a card, and the deck can't be
/// opened (the draw pile can, so if you know your deck you can guess). Innate cards are the counterplay: you know
/// they're in there. Party-wide; it's purely what each player's own screen shows, so nothing needs syncing.
/// </summary>
public sealed class FogboundLantern : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act2;

    // Hooks run on every game; each game only fogs its own local player's hand.
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (LocalContext.IsMe(player) && player.PlayerCombatState?.TurnNumber == 1)
        {
            Flash();
            HandFog.Raise();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMe(cardPlay.Card.Owner))
        {
            HandFog.Lift();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        HandFog.Lift();
        return Task.CompletedTask;
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
