using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode;

/// <summary>Ascension levels Calamity adds on top of vanilla's 10.</summary>
public static class CalamityAscension
{
    public const int VanillaMax = 10;

    /// <summary>A11: every act gains Calamity floors.</summary>
    public const int Calamity = 11;

    /// <summary>Highest ascension the mod currently adds.</summary>
    public const int Max = Calamity;

    public static bool IsActive(IRunState runState) => runState.AscensionLevel >= Calamity;
}
