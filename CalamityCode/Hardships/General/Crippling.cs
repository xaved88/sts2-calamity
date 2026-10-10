using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>Enemies have Backlash: whenever they lose Strength, they regain 25% of it (rounded up) permanently.</summary>
public sealed class SpitefulTotem : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Percent", 25m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BacklashPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState) =>
        ApplyToEnemies<BacklashPower>(choiceContext, combatState, DynamicVars["Percent"].BaseValue);

    /// <summary>Enemies summoned mid-combat get it too.</summary>
    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.IsEnemy && TurnNumber >= 1 && creature.GetPower<BacklashPower>() == null)
        {
            await PowerCmd.Apply<BacklashPower>(new ThrowingPlayerChoiceContext(), creature, DynamicVars["Percent"].BaseValue, Owner.Creature, null);
        }
    }
}

/// <summary>
/// Amount = percent of lost Strength regained, as real (permanent) StrengthPower. Temporary Strength Down routes
/// through PowerCmd.Apply&lt;StrengthPower&gt; with the original applier (TemporaryStrengthPower.BeforeApplied), so
/// this catches it; the temporary loss is refunded at end of turn but the regained Strength stays.
/// </summary>
public sealed class BacklashPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // Only Strength lost to someone else (a player); ignore the enemy's own scripted Strength changes.
        if (power is not StrengthPower || power.Owner != Owner || amount >= 0m || applier == null || applier == Owner)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Math.Ceiling(-amount * Amount / 100m), Owner, null);
    }
}

/// <summary>Enemies start combat with 1 Thorns.</summary>
public sealed class BriarCrown : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ThornsPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ThornsPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState) =>
        ApplyToEnemies<ThornsPower>(choiceContext, combatState, DynamicVars["ThornsPower"].BaseValue);
}

/// <summary>Enemies start combat with 1 Strength.</summary>
public sealed class WarDrum : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState) =>
        ApplyToEnemies<StrengthPower>(choiceContext, combatState, DynamicVars["StrengthPower"].BaseValue);
}

/// <summary>When an enemy dies, the other enemies gain 1 Strength.</summary>
public sealed class MourningBell : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsEnemy || wasRemovalPrevented || creature.CombatState is not { } combat)
        {
            return;
        }
        List<Creature> survivors = combat.HittableEnemies.Where(e => e != creature).ToList();
        if (survivors.Count == 0)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, survivors, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, null);
    }
}

/// <summary>When an enemy dies, it deals 3 damage to each player.</summary>
public sealed class MartyrsFuse : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Unpowered)];

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsEnemy || wasRemovalPrevented || creature.CombatState is not { } combat)
        {
            return;
        }
        Flash();
        await CreatureCmd.Damage(choiceContext, combat.PlayerCreatures.Where(c => c.IsAlive), DynamicVars.Damage.BaseValue, ValueProp.Unpowered, null, null);
    }
}

/// <summary>Elites and bosses start combat with 1 Artifact.</summary>
public sealed class WardensSeal : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ArtifactPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArtifactPower>()];

    protected override async Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (Owner.RunState.CurrentRoom is CombatRoom { RoomType: RoomType.Elite or RoomType.Boss })
        {
            await ApplyToEnemies<ArtifactPower>(choiceContext, combatState, DynamicVars["ArtifactPower"].BaseValue);
        }
    }
}

/// <summary>Merchant prices are 25% higher, for everyone.</summary>
public sealed class CrookedScale : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Markup", 25m)];

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
    {
        return Math.Ceiling(cost * (100m + DynamicVars["Markup"].BaseValue) / 100m);
    }
}

/// <summary>
/// "?" rooms are much more likely to be fights: every "?" that isn't a fight raises the fight odds 3x as fast as
/// normal (vanilla raises them by a fixed step each time).
/// </summary>
public sealed class BrambleSignpost : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Multiplier", 3m)];

    public override float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
    {
        return roomType == RoomType.Monster ? oddsIncrease * (float)DynamicVars["Multiplier"].BaseValue : oddsIncrease;
    }
}
