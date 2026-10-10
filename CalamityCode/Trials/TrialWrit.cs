using BaseLib.Abstracts;
using BaseLib.Utils;
using Calamity.CalamityCode.Hardships;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Calamity.CalamityCode.Trials;

/// <summary>
/// Records which trial(s) this act's boss faces, saved with the run. When the boss fight starts it applies them; one
/// writ per player, but trial powers are only applied to a boss that doesn't already have one. Greyed out once the
/// boss is beaten. Lives in the hardship pool only so it has a pool (never offered: not a HardshipRelic).
/// </summary>
[Pool(typeof(HardshipPool))]
public sealed class TrialWrit : CustomRelicModel
{
    private int _act = -1;
    private string _trials = "";
    private bool _fulfilled;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsAllowed(IRunState runState) => false;

    public override bool IsAllowedInShops => false;

    // Several writs across a run (one per act), so allow duplicates.
    public override bool IsStackable => true;

    public override bool IsUsedUp => Fulfilled;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("Trials"), new DynamicVar("Act", 1m)];

    public override string PackedIconPath => "res://Calamity/images/relics/packed/trial_writ.png";

    protected override string PackedIconOutlinePath => "res://Calamity/images/relics/outline/trial_writ.png";

    protected override string BigIconPath => "res://Calamity/images/relics/trial_writ.png";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => Assigned.Select(k => HoverTipFactory.FromPower(Trials.Power(k)));

    [SavedProperty]
    public int Act
    {
        get => _act;
        set
        {
            AssertMutable();
            _act = value;
            DynamicVars["Act"].BaseValue = value + 1;
        }
    }

    /// <summary>Comma-separated TrialKind names; in Act 3, first is boss 1 and second is boss 2.</summary>
    [SavedProperty]
    public string AssignedTrials
    {
        get => _trials;
        set
        {
            AssertMutable();
            _trials = value;
            ((StringVar)DynamicVars["Trials"]).StringValue = string.Join(", ",
                Assigned.Select(k => new MegaCrit.Sts2.Core.Localization.LocString("events", $"CALAMITY-TRIAL_EVENT.trials.{Trials.Key(k)}.title").GetFormattedText()));
        }
    }

    [SavedProperty]
    public bool Fulfilled
    {
        get => _fulfilled;
        set
        {
            AssertMutable();
            _fulfilled = value;
            if (value)
            {
                Status = RelicStatus.Disabled;
            }
        }
    }

    public IReadOnlyList<TrialKind> Assigned =>
        _trials.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Enum.Parse<TrialKind>).ToList();

    public void Record(int act, IReadOnlyList<TrialKind> trials)
    {
        Act = act;
        AssignedTrials = string.Join(",", trials);
    }

    /// <summary>The trial for the boss fight we're in, if this writ covers it.</summary>
    public TrialKind? TrialForCurrentBoss()
    {
        IRunState runState = Owner.RunState;
        if (Fulfilled || runState.CurrentActIndex != Act || runState.CurrentRoom is not CombatRoom { RoomType: RoomType.Boss })
        {
            return null;
        }
        int bossIndex = runState.Map.SecondBossMapPoint is { } second && runState.CurrentMapPoint == second ? 1 : 0;
        IReadOnlyList<TrialKind> assigned = Assigned;
        return bossIndex < assigned.Count ? assigned[bossIndex] : null;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState?.TurnNumber > 1 || TrialForCurrentBoss() is not { } kind)
        {
            return;
        }
        Type powerType = Trials.Powers[kind];
        foreach (Creature boss in combatState.HittableEnemies.Where(e => !e.Powers.Any(p => p is TrialPower)).ToList())
        {
            Flash();
            var canonical = (TrialPower)ModelDb.GetById<PowerModel>(ModelDb.GetId(powerType));
            int tier = Math.Clamp(Act + 1, 1, 3);
            await PowerCmd.Apply(choiceContext, canonical.ToMutable(), boss, canonical.InitialAmount(tier), Owner.Creature, null);
            if (boss.Powers.FirstOrDefault(p => p.GetType() == powerType) is TrialPower applied)
            {
                await applied.Configure(choiceContext);
            }
        }
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType == RoomType.Boss && Owner.RunState.CurrentActIndex == Act)
        {
            // In Act 3 the writ covers both bosses; it's fulfilled after the last one.
            bool lastBoss = Owner.RunState.Map.SecondBossMapPoint is not { } second || Owner.RunState.CurrentMapPoint == second;
            if (lastBoss)
            {
                Fulfilled = true;
            }
        }
        return Task.CompletedTask;
    }
}
