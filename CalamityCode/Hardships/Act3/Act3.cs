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

/// <summary>You're Manacled: you can play only 1 Attack each turn.</summary>
public sealed class IronManacle : PlayerPowerHardship<ManacledPower>
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override decimal PowerAmount => 1m;
}

/// <summary>Amount = Attacks allowed per turn.</summary>
public sealed class ManacledPower : HardshipPower
{
    private int _turn = -1;
    private int _attacksThisTurn;

    public override PowerType Type => PowerType.Debuff;

    private int Turn => Owner.Player?.PlayerCombatState?.TurnNumber ?? 0;

    private int AttacksThisTurn => _turn == Turn ? _attacksThisTurn : 0;

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        return card.Owner.Creature != Owner || card.Type != CardType.Attack || AttacksThisTurn < Amount;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            _attacksThisTurn = AttacksThisTurn + 1;
            _turn = Turn;
            if (_attacksThisTurn >= Amount)
            {
                Flash();
            }
        }
        return Task.CompletedTask;
    }
}

/// <summary>You start each combat Frozen: 1 less Energy on your first 2 turns.</summary>
public sealed class FrozenEmber : PlayerPowerHardship<FrozenPower>
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Turns", 2m)];

    protected override decimal PowerAmount => DynamicVars["Turns"].BaseValue;
}

/// <summary>Amount = turns left; 1 less Energy while it lasts. Applied before the first energy reset.</summary>
public sealed class FrozenPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        return player.Creature == Owner ? amount - 1m : amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }
}

/// <summary>Doom Clock: counts down from 3; from then on, gain 3 Doom at the start of each of your turns.</summary>
public sealed class DoomClock : PlayerPowerHardship<TickingPower>
{
    public override HardshipTier Tier => HardshipTier.Crushing;

    public override HardshipAct Act => HardshipAct.Act3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DoomPower>(3m), new DynamicVar("StartTurn", 3m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TickingPower>(), HoverTipFactory.FromPower<DoomPower>()];

    protected override decimal PowerAmount => DynamicVars["StartTurn"].BaseValue;
}

/// <summary>
/// Amount counts down by 1 at the start of each of your turns (3 -> 2 -> 1). While it's at 1, gain Doom at the start of
/// each turn, so Doom starts on turn 3.
/// </summary>
public sealed class TickingPower : HardshipPower
{
    private const int DoomPerTurn = 3;

    public override PowerType Type => PowerType.Debuff;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            if (Amount > 1)
            {
                return;
            }
        }
        if (player.PlayerCombatState?.TurnNumber >= 3)
        {
            Flash();
            await PowerCmd.Apply<DoomPower>(choiceContext, Owner, DoomPerTurn, Owner, null);
        }
    }
}
