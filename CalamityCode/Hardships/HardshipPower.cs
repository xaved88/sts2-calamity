using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Calamity.CalamityCode.Hardships;

/// <summary>The visible, in-combat face of a hardship. Permanent for the combat (no duration tick-down).</summary>
public abstract class HardshipPower : CustomPowerModel
{
    public override PowerStackType StackType => PowerStackType.Counter;

    private string Slug => Id.Entry[(Id.Entry.IndexOf('-') + 1)..].ToLowerInvariant();

    public override string? CustomPackedIconPath => $"res://Calamity/images/powers/{Slug}.png";

    public override string? CustomBigIconPath => $"res://Calamity/images/powers/big/{Slug}.png";
}
