using Calamity.CalamityCode.Pact;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>
/// Every 4 combats, choose one of your relics to melt (vanilla's wax-relic mechanic). Same counter pattern as vanilla's
/// Toy Box; the relic picker is vanilla's synced choose-a-relic screen.
/// </summary>
public sealed class GutteringBrazier : HardshipRelic
{
    private int _combatsSeen;

    public override HardshipTier Tier => HardshipTier.Crushing;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Combats", 4m)];

    public override bool ShowCounter => true;

    public override int DisplayAmount => CombatsSeen % DynamicVars["Combats"].IntValue;

    [SavedProperty]
    public int CombatsSeen
    {
        get => _combatsSeen;
        set
        {
            AssertMutable();
            _combatsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        CombatsSeen++;
        if (CombatsSeen % DynamicVars["Combats"].IntValue != 0)
        {
            return;
        }
        List<RelicModel> meltable = Owner.Relics.Where(CanMelt).ToList();
        if (meltable.Count == 0)
        {
            return;
        }
        Flash();
        RelicModel victim = await RelicSelectCmd.FromChooseARelicScreen(Owner, meltable) ?? meltable[0];
        // The game only melts wax relics (Wax Choker's), and throws otherwise; that throw hung the end of combat.
        victim.IsWax = true;
        await RelicCmd.Melt(victim);
    }

    // Only relics that still do something count: none of Calamity's own (hardships, Trial Writs: melting those would
    // be free), and no spent ones (e.g. Matryoshka after its chests).
    private static bool CanMelt(RelicModel relic) =>
        !relic.IsMelted && !relic.IsUsedUp && relic.GetType().Assembly != typeof(GutteringBrazier).Assembly;
}

/// <summary>A counter that carries between fights: every 3 of your turns, a Weight is added to your deck.</summary>
public sealed class BallastBox : HardshipRelic
{
    private int _turnsCounted;

    public override HardshipTier Tier => HardshipTier.Crushing;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Turns", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Weight>()];

    public override bool ShowCounter => true;

    public override int DisplayAmount => TurnsCounted % DynamicVars["Turns"].IntValue;

    [SavedProperty]
    public int TurnsCounted
    {
        get => _turnsCounted;
        set
        {
            AssertMutable();
            _turnsCounted = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }
        TurnsCounted++;
        if (TurnsCounted % DynamicVars["Turns"].IntValue != 0)
        {
            return;
        }
        Flash();
        // Permanent copy in the deck, plus a combat copy shuffled into the draw pile now, linked through DeckVersion (as
        // vanilla does for every card at combat start) so playing the Weight removes the deck copy too.
        CardModel deckCard = Owner.RunState.CreateCard(ModelDb.Card<Weight>(), Owner);
        CardPileAddResult added = await CardPileCmd.Add(deckCard, PileType.Deck);
        if (Owner.Creature.CombatState is { } combat)
        {
            CardModel combatCard = combat.CloneCard(deckCard);
            combatCard.DeckVersion = deckCard;
            added = await CardPileCmd.AddGeneratedCardToCombat(combatCard, PileType.Draw, Owner, CardPilePosition.Random);
        }
        CardCmd.PreviewCardPileAdd(added);
    }
}

/// <summary>When an ally adds a curse to their deck, you get a copy, Unholy Pacts included. Multiplayer only.</summary>
public sealed class BoundLocket : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override bool IsOfferable(IRunState runState) => runState.Players.Count > 1;

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        // Only curses newly entering someone else's deck, and never our own copies (no ping-pong between lockets).
        if (oldPileType != PileType.None || card.Pile?.Type != PileType.Deck || card.Type != CardType.Curse
            || card.Owner == Owner || clonedBy is BoundLocket)
        {
            return;
        }
        CardModel copy = Owner.RunState.CreateCard(ModelDb.GetById<CardModel>(card.Id), Owner);
        if (card is UnholyPact source && copy is UnholyPact pact)
        {
            pact.Severity = source.Severity;
        }
        Flash();
        CardPileAddResult added = await CardPileCmd.Add(copy, PileType.Deck, clonedBy: this);
        CardCmd.PreviewCardPileAdd(added);
    }
}

/// <summary>You have Blurred Vision: cards and potions you aim at an enemy hit a random enemy instead.</summary>
public sealed class BrokenGlasses : PlayerPowerHardship<BlurredVisionPower>
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    protected override decimal PowerAmount => 1m;
}

/// <summary>The redirect itself happens in BrokenGlassesCardPatch / BrokenGlassesPotionPatch (the target is fixed when a play is created).</summary>
public sealed class BlurredVisionPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>Picks the new target from the run's shared combat-targets RNG, so every client agrees.</summary>
    internal Creature Redirect(Creature target)
    {
        List<Creature> enemies = target.CombatState?.HittableEnemies.ToList() ?? [];
        if (!target.IsEnemy || enemies.Count < 2 || Owner.Player is not { } player)
        {
            return target;
        }
        Creature? pick = player.RunState.Rng.CombatTargets.NextItem(enemies);
        if (pick != null && pick != target)
        {
            Flash();
        }
        return pick ?? target;
    }
}

/// <summary>Add Gnawing to your deck: Eternal; at the end of your turn, if it's in your hand, lose 3 HP.</summary>
public sealed class HungryJar : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Gnawing>()];

    public override async Task AfterObtained()
    {
        await CardPileCmd.AddCurseToDeck<Gnawing>(Owner);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class BrokenGlassesCardPatch
{
    private static void Prefix(CardModel __instance, ref Creature? target)
    {
        if (target != null && __instance.TargetType == TargetType.AnyEnemy && __instance.Owner.Creature.GetPower<BlurredVisionPower>() is { } blur)
        {
            target = blur.Redirect(target);
        }
    }
}

[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OnUseWrapper))]
internal static class BrokenGlassesPotionPatch
{
    private static void Prefix(PotionModel __instance, ref Creature? target)
    {
        if (target != null && __instance.TargetType == TargetType.AnyEnemy && __instance.Owner.Creature.GetPower<BlurredVisionPower>() is { } blur)
        {
            target = blur.Redirect(target);
        }
    }
}
