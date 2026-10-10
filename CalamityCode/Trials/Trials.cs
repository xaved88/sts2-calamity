using MegaCrit.Sts2.Core.Models;

namespace Calamity.CalamityCode.Trials;

/// <summary>The nine trials, each a nod to a Slay the Spire 1 boss, and the boss power that implements each.</summary>
public enum TrialKind
{
    Slime,
    Ghost,
    Guardian,
    Champion,
    Automaton,
    Collector,
    Shapes,
    TimeEater,
    Awakened,
}

public static class Trials
{
    public static readonly IReadOnlyDictionary<TrialKind, Type> Powers = new Dictionary<TrialKind, Type>
    {
        [TrialKind.Slime] = typeof(SplitPower),
        [TrialKind.Ghost] = typeof(InfernoPower),
        [TrialKind.Guardian] = typeof(ModeShiftPower),
        [TrialKind.Champion] = typeof(SecondWindPower),
        [TrialKind.Automaton] = typeof(StasisPower),
        [TrialKind.Collector] = typeof(MegaDebuffPower),
        [TrialKind.Shapes] = typeof(SquareAndCirclePower),
        [TrialKind.TimeEater] = typeof(TimeWarpPower),
        [TrialKind.Awakened] = typeof(RebirthPower),
    };

    public static PowerModel Power(TrialKind kind) => ModelDb.GetById<PowerModel>(ModelDb.GetId(Powers[kind]));

    /// <summary>"TimeEater" -> "TIME_EATER": loc key segment and art slug.</summary>
    public static string Key(TrialKind kind) => MegaCrit.Sts2.Core.Helpers.StringHelper.Slugify(kind.ToString());
}
