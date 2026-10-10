using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Calamity.CalamityCode.Hardships;

// The two shapes almost every combat hardship takes (see CONVENTIONS.md): the relic only hands out a power, and the
// power holds all the logic, so whatever affects a fight is visible on the creature it affects.

/// <summary>
/// A hardship whose effect is a power on every enemy, including ones summoned mid-combat. Only ever held by one player
/// (a hardship held by anyone isn't offered again), and the power is applied once per enemy.
/// </summary>
public abstract class EnemyPowerHardship<TPower> : HardshipRelic where TPower : PowerModel
{
    /// <summary>The power's starting Amount (its number on the enemy).</summary>
    protected abstract decimal PowerAmount { get; }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TPower>()];

    protected override async Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        List<Creature> enemies = combatState.HittableEnemies.Where(e => e.GetPower<TPower>() == null).ToList();
        if (enemies.Count == 0)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<TPower>(choiceContext, enemies, PowerAmount, Owner.Creature, null);
    }

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.IsEnemy && TurnNumber >= 1 && creature.GetPower<TPower>() == null)
        {
            await PowerCmd.Apply<TPower>(new ThrowingPlayerChoiceContext(), creature, PowerAmount, Owner.Creature, null);
        }
    }
}

/// <summary>A hardship whose effect is a power on its holder, applied at the start of each combat (before the first draw).</summary>
public abstract class PlayerPowerHardship<TPower> : HardshipRelic where TPower : PowerModel
{
    /// <summary>The power's starting Amount (its number on the player).</summary>
    protected abstract decimal PowerAmount { get; }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        Flash();
        return PowerCmd.Apply<TPower>(choiceContext, Owner.Creature, PowerAmount, Owner.Creature, null);
    }
}
