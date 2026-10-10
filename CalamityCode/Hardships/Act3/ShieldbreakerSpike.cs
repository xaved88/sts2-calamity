using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Act3;

/// <summary>Enemy attacks deal 3 extra damage to Block: they strip up to 3 Block before the hit lands. Party-wide.</summary>
public sealed class ShieldbreakerSpike : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3m, ValueProp.Unpowered)];

    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer is not { IsEnemy: true } || !target.IsPlayer || !props.IsPoweredAttack() || target.Block <= 0)
        {
            return;
        }
        Flash();
        await CreatureCmd.LoseBlock(target, Math.Min(target.Block, DynamicVars.Block.BaseValue));
    }
}
