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

/// <summary>Your first Attack each combat deals 3 less damage (the Damp power, removed after that attack).</summary>
public sealed class DampPowderHorn : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Underdocks;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Damp", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DampPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        return PowerCmd.Apply<DampPower>(choiceContext, Owner.Creature, DynamicVars["Damp"].BaseValue, Owner.Creature, null);
    }
}

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
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>Whenever an enemy deals unblocked attack damage to a player, add a Slimed to that player's discard pile.</summary>
public sealed class BilgeBucket : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Underdocks;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Slimed>()];

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer is not { IsEnemy: true } || target.Player is not { } victim || result.UnblockedDamage <= 0 || !props.IsPoweredAttack() || target.CombatState is not { } combat)
        {
            return;
        }
        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(combat.CreateCard<Slimed>(victim), PileType.Discard, victim);
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
