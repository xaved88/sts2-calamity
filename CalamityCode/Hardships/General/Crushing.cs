using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>You're Drowsy: draw 1 fewer card each turn.</summary>
public sealed class LeadBookmark : PlayerPowerHardship<DrowsyPower>
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override decimal PowerAmount => DynamicVars.Cards.BaseValue;
}

public sealed class DrowsyPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player.Creature == Owner ? Math.Max(0m, count - Amount) : count;
    }
}
