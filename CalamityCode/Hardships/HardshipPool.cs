using BaseLib.Abstracts;

namespace Calamity.CalamityCode.Hardships;

/// <summary>
/// Pool holding every hardship. It must be "shared" so it's listed in ModelDb.AllRelicPools (RelicModel.Pool searches
/// there and throws if the relic isn't found). Shared pools aren't rolled for rewards on their own (vanilla's Event
/// and Deprecated pools are shared too), and hardships also return IsAllowed = false.
/// </summary>
public sealed class HardshipPool : CustomRelicPoolModel
{
    public override bool IsShared => true;
}
