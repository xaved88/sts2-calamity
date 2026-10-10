using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>Whenever you lose HP, lose 1 more. In combat this is the Sensitive power; outside combat the relic does it.</summary>
public sealed class NettleShirt : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Extra", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SensitivePower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        return PowerCmd.Apply<SensitivePower>(choiceContext, Owner.Creature, DynamicVars["Extra"].BaseValue, Owner.Creature, null);
    }

    // Events, rest sites, etc. The power covers combat (including the turn-1 window before it's applied: rare enough to ignore).
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (InCombat || target != Owner.Creature || amount <= 0m)
        {
            return amount;
        }
        Flash();
        return amount + DynamicVars["Extra"].BaseValue;
    }
}

public sealed class SensitivePower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return target == Owner && amount > 0m ? amount + Amount : amount;
    }
}

/// <summary>Start each combat with 1 Brittle: take additional damage from each attack hit.</summary>
public sealed class CrackedFemur : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Brittle", 1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BrittlePower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        return PowerCmd.Apply<BrittlePower>(choiceContext, Owner.Creature, DynamicVars["Brittle"].BaseValue, Owner.Creature, null);
    }
}

/// <summary>Flat, like Strength, so it matters on small hits.</summary>
public sealed class BrittlePower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer == Owner || !props.IsPoweredAttack())
        {
            return 0m;
        }
        return Amount;
    }
}

/// <summary>
/// Restore 50% less HP at campfires. Mend heals through the same hook, applied to whoever is healed, so trading Mends
/// doesn't dodge it. Shown under the Rest option like vanilla's Regal Pillow.
/// </summary>
public sealed class LumpyCot : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("HealReduction", 50m)];

    public override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
    {
        if (creature.Player != Owner && creature.PetOwner != Owner)
        {
            return amount;
        }
        return Math.Floor(amount * (100m - DynamicVars["HealReduction"].BaseValue) / 100m);
    }

    public override Task AfterRestSiteHeal(Player player, bool isMimicked)
    {
        if (player == Owner)
        {
            Flash();
        }
        return Task.CompletedTask;
    }

    public override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
    {
        if (!LocalContext.IsMe(Owner) || AdditionalRestSiteHealText is not { } text)
        {
            return currentExtraText;
        }
        return [.. currentExtraText, text];
    }
}

/// <summary>Gain 25% less Gold. Gold is nearly worthless by Act 3, so it isn't offered there.</summary>
public sealed class MothEatenPurse : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("GoldLoss", 25m)];

    public override bool IsOfferable(IRunState runState) => runState.CurrentActIndex < 2;

    public override decimal ModifyGoldGained(Player player, decimal amount)
    {
        return player == Owner ? Math.Floor(amount * (100m - DynamicVars["GoldLoss"].BaseValue) / 100m) : amount;
    }

    public override Task AfterModifyingGoldGained(Player player, decimal amount)
    {
        Flash();
        return Task.CompletedTask;
    }
}

/// <summary>Start each combat with 2 Dazed in your draw pile.</summary>
public sealed class HeavyPack : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Dazed>()];

    protected override async Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        var dazed = Enumerable.Range(0, DynamicVars.Cards.IntValue).Select(_ => combatState.CreateCard<Dazed>(Owner)).ToList();
        await CardPileCmd.AddGeneratedCardsToCombat(dazed, PileType.Draw, Owner, CardPilePosition.Random);
    }
}

/// <summary>One fewer potion slot (the same call vanilla's Tight Belt ascension uses).</summary>
public sealed class LeakyFlask : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override async Task AfterObtained()
    {
        // Vanilla's slot removal assumes a free slot to move the last potion into (it only runs at run start, on an empty
        // belt); with a full belt it indexes slot -1 and throws. So with no free slot, discard the right-most potion first.
        if (!Owner.HasOpenPotionSlots && Owner.PotionSlots.LastOrDefault(p => p != null) is { } rightmost)
        {
            await PotionCmd.Discard(rightmost);
        }
        await PlayerCmd.LoseMaxPotionCount(1, Owner);
    }

    public override Task AfterRemoved() => PlayerCmd.GainMaxPotionCount(1, Owner);
}

/// <summary>You're Itchy: whenever you shuffle your draw pile, lose 1 HP.</summary>
public sealed class FleaRiddenBlanket : PlayerPowerHardship<ItchyPower>
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("HpLoss", 1m)];

    protected override decimal PowerAmount => DynamicVars["HpLoss"].BaseValue;
}

public sealed class ItchyPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler.Creature != Owner)
        {
            return;
        }
        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }
}

/// <summary>The first card you play each combat costs 1 more (the Seized power, removed once you play a card).</summary>
public sealed class RustedHinge : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SeizedPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        return PowerCmd.Apply<SeizedPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, null);
    }
}

public sealed class SeizedPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || originalCost < 0m)
        {
            return false;
        }
        modifiedCost = originalCost + Amount;
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner)
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>Whenever you enter a "?" room, lose 5 HP. Makes dodging fights through events cost something.</summary>
public sealed class BloodiedSignpost : HardshipRelic
{
    private MapCoord? _lastPaidAt;

    public override HardshipTier Tier => HardshipTier.Trying;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("HpLoss", 5m)];

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        // An event that starts a fight enters a second room on the same map point; only pay once per point.
        MapPoint? point = Owner.RunState.CurrentMapPoint;
        if (point?.PointType != MapPointType.Unknown || _lastPaidAt == point.coord)
        {
            return;
        }
        _lastPaidAt = point.coord;
        Flash();
        await LoseHp(new ThrowingPlayerChoiceContext(), DynamicVars["HpLoss"].BaseValue);
    }
}
