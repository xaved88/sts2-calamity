using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>Draw 1 fewer card every turn.</summary>
public sealed class LeadBookmark : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player == Owner ? Math.Max(0m, count - DynamicVars.Cards.BaseValue) : count;
    }

    public override Task AfterModifyingHandDraw()
    {
        Flash();
        return Task.CompletedTask;
    }
}
