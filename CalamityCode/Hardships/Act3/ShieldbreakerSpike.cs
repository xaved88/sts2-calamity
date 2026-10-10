using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Act3;

/// <summary>Enemies are Shieldbreakers: their attacks deal 3 extra damage to Block. Party-wide.</summary>
public sealed class ShieldbreakerSpike : EnemyPowerHardship<ShieldbreakerPower>
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3m, ValueProp.Unpowered)];

    protected override decimal PowerAmount => DynamicVars.Block.BaseValue;
}

/// <summary>Amount = extra damage to Block: strips up to that much Block before each attack hit lands.</summary>
public sealed class ShieldbreakerPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || !target.IsPlayer || !props.IsPoweredAttack() || target.Block <= 0)
        {
            return;
        }
        Flash();
        await CreatureCmd.LoseBlock(target, Math.Min(target.Block, Amount));
    }
}
