using BaseLib.Abstracts;
using Calamity.CalamityCode.Map;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode;

/// <summary>
/// Always-present rules object that receives run-level hooks (BaseLib's standard for mod-owned hook listeners).
/// </summary>
public sealed class CalamityRules() : CustomSingletonModel(HookType.Run)
{
    /// <summary>
    /// Only runs for freshly generated maps. Saved maps are restored through ModifyGeneratedMapLate and already
    /// contain the calamity rows, so they're never inserted twice.
    /// </summary>
    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
        if (!CalamityAscension.IsActive(runState) || map is not StandardActMap)
        {
            return map;
        }
        return new CalamityActMap(map);
    }
}
