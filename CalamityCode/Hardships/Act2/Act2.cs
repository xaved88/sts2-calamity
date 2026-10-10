using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Act2;

// Act 2 (any act at index 1): attrition and denial.

/// <summary>Block from cards is reduced by 1.</summary>
public sealed class RustedBuckler : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(1m, ValueProp.Unpowered)];

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        return target == Owner.Creature && cardSource != null ? -DynamicVars.Block.BaseValue : 0m;
    }
}

/// <summary>Your max hand size is 2 smaller (BaseLib's hand-size hook).</summary>
public sealed class TinyPouch : HardshipRelic, IMaxHandSizeModifier
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        return player == Owner ? currentMaxHandSize - DynamicVars.Cards.IntValue : currentMaxHandSize;
    }
}

/// <summary>When an enemy dies, the other enemies heal half of its Max HP.</summary>
public sealed class HiveNectar : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act2;

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!creature.IsEnemy || wasRemovalPrevented || creature.CombatState is not { } combat)
        {
            return;
        }
        List<Creature> survivors = combat.HittableEnemies.Where(e => e != creature && e.IsAlive).ToList();
        if (survivors.Count == 0)
        {
            return;
        }
        Flash();
        decimal heal = Math.Floor(creature.MaxHp / 2m);
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

/// <summary>Your Powers cost 1 more.</summary>
public sealed class CloudedOrb : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner != Owner || card.Type != CardType.Power || originalCost < 0m)
        {
            return false;
        }
        modifiedCost = originalCost + DynamicVars.Energy.BaseValue;
        return true;
    }
}
