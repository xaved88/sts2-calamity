using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships.Act3;

// Act 3 (any act at index 2): immediate pain. Few fights are left, so long-term penalties are pointless here.

/// <summary>Discard 1 random card at the start of your first turn.</summary>
public sealed class TremblingGauntlet : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    // AfterPlayerTurnStart runs after the hand is drawn.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || TurnNumber != 1 || Owner.PlayerCombatState is not { } state || state.Hand.Cards.Count == 0)
        {
            return;
        }
        CardModel? victim = Owner.RunState.Rng.CombatCardSelection.NextItem(state.Hand.Cards);
        if (victim == null)
        {
            return;
        }
        Flash();
        await CardCmd.Discard(choiceContext, victim);
    }
}

/// <summary>The first time each enemy drops to half HP or below, it gains 3 Strength (Red Skull, for them).</summary>
public sealed class BlueSkull : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RageThresholdPower>()];

    protected override Task AtCombatStart(PlayerChoiceContext choiceContext, ICombatState combatState) =>
        ApplyToEnemies<RageThresholdPower>(choiceContext, combatState, DynamicVars["StrengthPower"].BaseValue);
}

/// <summary>Amount = Strength gained once this drops to half HP; then the power is spent.</summary>
public sealed class RageThresholdPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || !Owner.IsAlive || Owner.CurrentHp * 2 > Owner.MaxHp)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}

/// <summary>You can play only 1 Attack per turn.</summary>
public sealed class IronManacle : HardshipRelic
{
    private int _attackTurn = -1;

    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act3;

    public override bool ShouldPlay(CardModel card, MegaCrit.Sts2.Core.Entities.Cards.AutoPlayType autoPlayType)
    {
        return card.Owner != Owner || card.Type != CardType.Attack || _attackTurn != TurnNumber;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            _attackTurn = TurnNumber;
            Flash();
        }
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _attackTurn = -1;
        return Task.CompletedTask;
    }
}

/// <summary>1 less Energy on your first two turns.</summary>
public sealed class FrozenEmber : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        return player == Owner && TurnNumber is >= 1 and <= 2 ? amount - DynamicVars.Energy.BaseValue : amount;
    }
}

/// <summary>From turn 3, gain 3 Doom at the start of each of your turns (vanilla Doom: die at end of turn if Doom >= HP).</summary>
public sealed class DoomClock : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DoomPower>(3m), new DynamicVar("StartTurn", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || TurnNumber < DynamicVars["StartTurn"].IntValue)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<DoomPower>(choiceContext, Owner.Creature, DynamicVars["DoomPower"].BaseValue, Owner.Creature, null);
    }
}
