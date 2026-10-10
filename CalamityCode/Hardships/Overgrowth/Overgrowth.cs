using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
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

    protected override async Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        WeakPower? weak = await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, DynamicVars.Weak.BaseValue, Owner.Creature, null);
        // Vanilla makes every debuff applied to a player skip its next tick, because enemies apply them during their own
        // turn and the tick at the end of that turn would eat them instantly. Applied at the start of the player's turn,
        // that skip would make 1 Weak last two turns, so tick normally: Weak 1 covers turn 1 only.
        if (weak != null)
        {
            weak.SkipNextDurationTick = false;
        }
    }
}

/// <summary>You have Thorny Grip: the first Attack you play each turn deals 1 damage to you.</summary>
public sealed class ThistleGrip : PlayerPowerHardship<ThornyGripPower>
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1m, ValueProp.Unpowered)];

    protected override decimal PowerAmount => DynamicVars.Damage.BaseValue;
}

public sealed class ThornyGripPower : HardshipPower
{
    private int _lastTurnStung;

    public override PowerType Type => PowerType.Debuff;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int turn = Owner.Player?.PlayerCombatState?.TurnNumber ?? 0;
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack || _lastTurnStung == turn)
        {
            return;
        }
        _lastTurnStung = turn;
        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, Amount, ValueProp.Unpowered, null, null);
    }
}

/// <summary>Enemies carry a Spore Cloud: when one dies, every player gains 1 Weak.</summary>
public sealed class PuffballSpore : EnemyPowerHardship<SporeCloudPower>
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1m)];

    protected override decimal PowerAmount => DynamicVars.Weak.BaseValue;
}

/// <summary>Amount = Weak each player gains when this dies.</summary>
public sealed class SporeCloudPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Owner || wasRemovalPrevented || Owner.CombatState is not { } combat)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<WeakPower>(choiceContext, combat.PlayerCreatures.Where(c => c.IsAlive), Amount, Owner, null);
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
