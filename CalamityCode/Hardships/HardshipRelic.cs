using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships;

/// <summary>Severity only (see design/hardships.md). Trying/Crushing hit you; Crippling hits enemies or the world.</summary>
public enum HardshipTier
{
    Trying,
    Crippling,
    Crushing,
}

/// <summary>Which pool a hardship belongs to. Act 2/3 are keyed by act index so future acts at that index inherit them.</summary>
public enum HardshipAct
{
    General,
    Overgrowth,
    Underdocks,
    Act2,
    Act3,
}

/// <summary>
/// A negative relic taken at a Calamity. Each player holds their own; a hardship held by anyone is never offered again,
/// so party-wide (Crippling) effects only ever come from one copy.
///
/// Combat effects should be visible: apply a <see cref="HardshipPower"/> or a vanilla power rather than silently
/// modifying numbers.
/// </summary>
[Pool(typeof(HardshipPool))]
public abstract class HardshipRelic : CustomRelicModel
{
    public abstract HardshipTier Tier { get; }

    public virtual HardshipAct Act => HardshipAct.General;

    /// <summary>Extra restrictions on when this can be offered (e.g. not in Act 3, multiplayer only).</summary>
    public virtual bool IsOfferable(IRunState runState) => true;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsAllowedInShops => false;

    /// <summary>Never offered by vanilla relic generation; only Calamity rooms grant hardships.</summary>
    public override bool IsAllowed(IRunState runState) => false;

    /// <summary>"CALAMITY-LUMPY_COT" -> "lumpy_cot"; matches the art file names.</summary>
    public string Slug => Id.Entry[(Id.Entry.IndexOf('-') + 1)..].ToLowerInvariant();

    public override string PackedIconPath => $"res://Calamity/images/relics/packed/{Slug}.png";

    protected override string PackedIconOutlinePath => $"res://Calamity/images/relics/outline/{Slug}.png";

    protected override string BigIconPath => $"res://Calamity/images/relics/{Slug}.png";

    protected int TurnNumber => Owner.PlayerCombatState?.TurnNumber ?? 0;

    protected bool InCombat => Owner.Creature.CombatState != null;

    /// <summary>Runs once per combat, at the start of the owner's first turn (vanilla Bag of Marbles timing).</summary>
    protected virtual Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState) => Task.CompletedTask;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) && TurnNumber <= 1)
        {
            await AtCombatStart(choiceContext, combatState);
        }
    }

    protected async Task ApplyToEnemies<TPower>(PlayerChoiceContext choiceContext, ICombatState combatState, decimal amount) where TPower : PowerModel
    {
        Flash();
        await PowerCmd.Apply<TPower>(choiceContext, combatState.HittableEnemies, amount, Owner.Creature, null);
    }

    /// <summary>HP loss (ignores Block), from no one.</summary>
    protected Task LoseHp(PlayerChoiceContext choiceContext, decimal amount) =>
        CreatureCmd.Damage(choiceContext, Owner.Creature, amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
}
