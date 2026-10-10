using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Map;

public static class CalamityMapPoint
{
    /// <summary>
    /// The calamity map point type, allocated by BaseLib at ModelDb.Init. Network sync (8-bit) and JSON saves
    /// (allowIntegerValues) carry it as-is; the switches that would throw on it are patched in MapPatches.
    /// </summary>
    [CustomEnum]
    public static MapPointType Type;

    /// <summary>The trial point type (first floor after the Ancient); same handling as <see cref="Type"/>.</summary>
    [CustomEnum]
    public static MapPointType Trial;

    public static bool IsCurrent(IRunState runState) => runState.CurrentMapPoint?.PointType == Type;
}
