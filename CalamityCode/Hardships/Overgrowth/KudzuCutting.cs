using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Calamity.CalamityCode.Hardships.Overgrowth;

/// <summary>Every 4th combat, add a Kudzu (Curse, Unplayable) to your deck. Nothing on pickup.</summary>
public sealed class KudzuCutting : HardshipRelic
{
    private int _combatsSeen;

    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Overgrowth;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Combats", 4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Kudzu>()];

    public override bool ShowCounter => true;

    public override int DisplayAmount => CombatsSeen % DynamicVars["Combats"].IntValue;

    [SavedProperty]
    public int CombatsSeen
    {
        get => _combatsSeen;
        set
        {
            AssertMutable();
            _combatsSeen = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        CombatsSeen++;
        if (CombatsSeen % DynamicVars["Combats"].IntValue == 0)
        {
            Flash();
            await CardPileCmd.AddCurseToDeck<Kudzu>(Owner);
        }
    }
}
