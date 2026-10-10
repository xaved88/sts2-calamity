using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Trials;

/// <summary>
/// The event behind every Trial point (first floor after the Ancient). Shared, so the party votes with vanilla's
/// voting (a random ballot decides, the same as events and pathing).
/// * Acts 1-2: vote for the trial to include; this act's boss gets it.
/// * Act 3: vote for the trial to exclude; the other two are randomly assigned to the two bosses.
/// Every player gets a Trial Writ recording the result, which applies it when the boss fight starts.
///
/// autoAdd is off and IsAllowed is false, so it never shows up in random "?" rooms; only Trial points create it.
/// </summary>
public sealed class TrialEvent() : CustomEventModel(autoAdd: false)
{
    private const int Offered = 3;

    public override bool IsShared => true;

    public override bool IsAllowed(IRunState runState) => false;

    public override string? CustomInitialPortraitPath => "res://Calamity/images/events/trial_event.png";

    private static bool IsFinalAct(IRunState runState) => runState.CurrentActIndex >= 2;

    /// <summary>Same three for every player: seeded by run and act only.</summary>
    private static List<TrialKind> Drawn(IRunState runState)
    {
        var rng = new Rng(runState.Rng.Seed, $"trial_{runState.CurrentActIndex}");
        return Enum.GetValues<TrialKind>().ToList().UnstableShuffle(rng).Take(Offered).ToList();
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        Player owner = Owner ?? throw new InvalidOperationException("Trial event has no owner.");
        bool exclude = IsFinalAct(owner.RunState);
        return Drawn(owner.RunState).Select(kind =>
        {
            LocString title = L10NLookup($"{Id.Entry}.trials.{Trials.Key(kind)}.title");
            LocString description = L10NLookup($"{Id.Entry}.{(exclude ? "exclude" : "include")}");
            description.Add("Trial", L10NLookup($"{Id.Entry}.trials.{Trials.Key(kind)}.title").GetFormattedText());
            return new EventOption(this, () => Choose(kind), title, description, InitialOptionKey(Trials.Key(kind)),
                [HoverTipFactory.FromPower(Trials.Power(kind))]);
        }).ToList();
    }

    private async Task Choose(TrialKind chosen)
    {
        IRunState runState = Owner!.RunState;
        List<TrialKind> assignment;
        if (IsFinalAct(runState))
        {
            var rng = new Rng(runState.Rng.Seed, $"trial_assign_{runState.CurrentActIndex}");
            assignment = Drawn(runState).Where(k => k != chosen).ToList().UnstableShuffle(rng);
        }
        else
        {
            assignment = [chosen];
        }

        TrialWrit writ = (TrialWrit)ModelDb.Relic<TrialWrit>().ToMutable();
        writ.Record(runState.CurrentActIndex, assignment);
        await RelicCmd.Obtain(writ, Owner);
        TrialBadges.Refresh();
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CHOSEN.description"));
    }
}
