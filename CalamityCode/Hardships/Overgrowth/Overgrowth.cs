using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Overgrowth;

// Act 1, Overgrowth: spores, thorns, growth. Crushing ones hurt the rest of the run.

/// <summary>Start each combat with 1 Weak.</summary>
public sealed class PollenSachet : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        return PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, DynamicVars.Weak.BaseValue, Owner.Creature, null);
    }
}

/// <summary>The first Attack you play each turn deals 1 damage to you.</summary>
public sealed class ThistleGrip : HardshipRelic
{
    private int _lastTurnStung;

    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1m, ValueProp.Unpowered)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card.Type != CardType.Attack || _lastTurnStung == TurnNumber)
        {
            return;
        }
        _lastTurnStung = TurnNumber;
        Flash();
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue, ValueProp.Unpowered, null, null);
    }

    public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        _lastTurnStung = 0;
        return Task.CompletedTask;
    }
}

/// <summary>When an enemy dies, every player gains 1 Weak.</summary>
public sealed class PuffballSpore : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WeakPower>()];

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsEnemy || wasRemovalPrevented || creature.CombatState is not { } combat)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<WeakPower>(choiceContext, combat.PlayerCreatures.Where(c => c.IsAlive), DynamicVars.Weak.BaseValue, creature, null);
    }
}

/// <summary>Enemies start combat with 3 Plating.</summary>
public sealed class IronbarkSap : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState) =>
        ApplyToEnemies<PlatingPower>(choiceContext, combatState, DynamicVars["PlatingPower"].BaseValue);
}

/// <summary>Card rewards offer 1 fewer card.</summary>
public sealed class BlightedSeeds : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        if (player != Owner || cardRewardOptions.Count <= 1)
        {
            return false;
        }
        cardRewardOptions.RemoveAt(cardRewardOptions.Count - 1);
        Flash();
        return true;
    }
}
