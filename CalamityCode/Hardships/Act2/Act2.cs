using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Act2;

// Act 2 (any act at index 1): attrition and denial.

/// <summary>You're Rusted: Block from cards is reduced by 1.</summary>
public sealed class RustedBuckler : PlayerPowerHardship<RustedPower>
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(1m, ValueProp.Unpowered)];

    protected override decimal PowerAmount => DynamicVars.Block.BaseValue;
}

public sealed class RustedPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        return target == Owner && cardSource != null ? -Amount : 0m;
    }
}

/// <summary>You're Cramped: your max hand size is 2 smaller (BaseLib's hand-size hook).</summary>
public sealed class TinyPouch : PlayerPowerHardship<CrampedPower>
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override decimal PowerAmount => DynamicVars.Cards.BaseValue;
}

public sealed class CrampedPower : HardshipPower, IMaxHandSizeModifier
{
    public override PowerType Type => PowerType.Debuff;

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        return player.Creature == Owner ? currentMaxHandSize - Amount : currentMaxHandSize;
    }
}

/// <summary>Enemies share Lifeblood: when one dies, the others heal half of its Max HP.</summary>
public sealed class HiveNectar : EnemyPowerHardship<SharedLifebloodPower>
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override decimal PowerAmount => 50m;
}

/// <summary>Amount = percent of this creature's Max HP the other enemies heal when it dies.</summary>
public sealed class SharedLifebloodPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Owner || wasRemovalPrevented || Owner.CombatState is not { } combat)
        {
            return;
        }
        List<Creature> survivors = combat.HittableEnemies.Where(e => e != Owner && e.IsAlive).ToList();
        if (survivors.Count == 0)
        {
            return;
        }
        Flash();
        decimal heal = Math.Floor(Owner.MaxHp * Amount / 100m);
        foreach (Creature survivor in survivors)
        {
            await CreatureCmd.Heal(survivor, heal);
        }
    }
}

/// <summary>You can't Smith at rest sites.</summary>
public sealed class DullWhetstone : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act2;

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }
        List<RestSiteOption> smiths = options.Where(o => o is SmithRestSiteOption).ToList();
        foreach (RestSiteOption smith in smiths)
        {
            options.Remove(smith);
        }
        return smiths.Count > 0;
    }
}

/// <summary>You're Clouded: your Powers cost 1 more.</summary>
public sealed class CloudedOrb : PlayerPowerHardship<CloudedPower>
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override decimal PowerAmount => DynamicVars.Energy.BaseValue;
}

public sealed class CloudedPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || card.Type != CardType.Power || originalCost < 0m)
        {
            return false;
        }
        modifiedCost = originalCost + Amount;
        return true;
    }
}
