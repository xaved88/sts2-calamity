using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Underdocks;

// Act 1, Underdocks: water, rot, smugglers. Crushing ones hurt the rest of the run.

/// <summary>You start each combat Damp 3: each Attack deals Damp less damage, then Damp dries out by 1.</summary>
public sealed class DampPowderHorn : PlayerPowerHardship<DampPower>
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Underdocks;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Damp", 3m)];

    protected override decimal PowerAmount => DynamicVars["Damp"].BaseValue;
}

/// <summary>Amount = damage removed from each hit of your Attacks; drops by 1 after each Attack you play.</summary>
public sealed class DampPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return dealer == Owner && props.IsPoweredAttack() ? -Amount : 0m;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            await PowerCmd.Decrement(this);
        }
    }
}

/// <summary>Enemies are Slimy: their unblocked attacks add a Slimed to the target's discard pile.</summary>
public sealed class BilgeBucket : EnemyPowerHardship<SlimyPower>
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Underdocks;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SlimyPower>(), HoverTipFactory.FromCard<Slimed>()];

    protected override decimal PowerAmount => 1m;
}

/// <summary>Amount = Slimed added per unblocked attack hit.</summary>
public sealed class SlimyPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || target.Player is not { } victim || result.UnblockedDamage <= 0 || !props.IsPoweredAttack() || Owner.CombatState is not { } combat)
        {
            return;
        }
        Flash();
        for (int i = 0; i < Amount; i++)
        {
            await CardPileCmd.AddGeneratedCardToCombat(combat.CreateCard<Slimed>(victim), PileType.Discard, victim);
        }
    }
}

/// <summary>Cards from rewards are never pre-upgraded.</summary>
public sealed class RottenCrate : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Underdocks;

    public override decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
    {
        return player == Owner ? 0m : odds;
    }
}

/// <summary>Lose 1 Max HP after every combat.</summary>
public sealed class CrackedHullPlank : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Underdocks;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(1m)];

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Flash();
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.MaxHp.BaseValue, isFromCard: false);
    }
}
