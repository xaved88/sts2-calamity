using Calamity.CalamityCode.Hardships;
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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Trials;

/// <summary>
/// A boss modifier chosen at a Trial. Amount is the trial's visible counter or value; act-scaled numbers come from
/// <see cref="Tier"/> (Act 1/2/3) and are shown through DynamicVars set in <see cref="Configure"/>.
/// </summary>
public abstract class TrialPower : HardshipPower
{
    public override PowerType Type => PowerType.Buff;

    protected int Tier => Math.Clamp((Owner.CombatState?.RunState.CurrentActIndex ?? 0) + 1, 1, 3);

    protected static T ByAct<T>(int tier, T act1, T act2, T act3) => tier switch { 1 => act1, 2 => act2, _ => act3 };

    /// <summary>Starting Amount for a tier.</summary>
    public abstract decimal InitialAmount(int tier);

    /// <summary>Sets act-scaled description values and any starting effects, right after the power is applied.</summary>
    public virtual Task Configure(PlayerChoiceContext choiceContext) => Task.CompletedTask;

    protected IEnumerable<Creature> LivingPlayers => Owner.CombatState?.PlayerCreatures.Where(c => c.IsAlive) ?? [];

    protected async Task AddToEachPlayer<TCard>(PileType pile, int count) where TCard : CardModel
    {
        if (Owner.CombatState is not { } combat)
        {
            return;
        }
        foreach (Creature creature in LivingPlayers)
        {
            if (creature.Player is not { } player)
            {
                continue;
            }
            var cards = Enumerable.Range(0, count).Select(_ => combat.CreateCard<TCard>(player)).ToList();
            await CardPileCmd.AddGeneratedCardsToCombat(cards, pile, player, CardPilePosition.Random);
        }
    }

    protected async Task RemoveDebuffs()
    {
        foreach (PowerModel debuff in Owner.Powers.Where(p => p.GetTypeForAmount(p.Amount) == PowerType.Debuff).ToList())
        {
            await PowerCmd.Remove(debuff);
        }
    }
}

/// <summary>Trial of the Slime. Split: each time it loses a quarter of its Max HP, a slime splits off.</summary>
public sealed class SplitPower : TrialPower
{
    public override decimal InitialAmount(int tier) => 3m;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // Amount = splits left; thresholds at 75%, 50%, 25% HP.
        while (target == Owner && Owner.IsAlive && Amount > 0 && Owner.CurrentHp * 4 <= Owner.MaxHp * Amount)
        {
            Flash();
            await PowerCmd.Decrement(this);
            await Split(choiceContext);
        }
    }

    private async Task Split(PlayerChoiceContext choiceContext)
    {
        if (Owner.CombatState is not { } combat)
        {
            return;
        }
        string slot = combat.Encounter?.GetNextSlot(combat) ?? string.Empty;
        if (slot.Length == 0)
        {
            // No room in this encounter's layout: the slime splatters onto everyone instead.
            await AddToEachPlayer<Slimed>(PileType.Discard, 2);
            return;
        }
        bool leaf = Owner.CombatState.RunState.Rng.MonsterAi.NextInt(2) == 0;
        MonsterModel slime = Tier == 1
            ? (leaf ? ModelDb.Monster<LeafSlimeS>() : ModelDb.Monster<TwigSlimeS>())
            : (leaf ? ModelDb.Monster<LeafSlimeM>() : ModelDb.Monster<TwigSlimeM>());
        await CreatureCmd.Add(slime.ToMutable(), combat, CombatSide.Enemy, slot);
    }
}

/// <summary>Trial of the Ghost. Inferno: every 3rd turn, Burns are shuffled into each player's draw pile.</summary>
public sealed class InfernoPower : TrialPower
{
    private const int Period = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override decimal InitialAmount(int tier) => Period;

    public override Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars.Cards.BaseValue = ByAct(Tier, 1, 2, 3);
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy || !participants.Contains(Owner))
        {
            return;
        }
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, Period - Amount, Owner, null, silent: true);
        await AddToEachPlayer<Burn>(PileType.Draw, DynamicVars.Cards.IntValue);
    }
}

/// <summary>Trial of the Guardian. Mode Shift: after taking enough damage, gain Block and Thorns for 2 turns.</summary>
public sealed class ModeShiftPower : TrialPower
{
    private int _shifts;
    private int _thornsTurnsLeft;
    private int _thornsGranted;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15m, ValueProp.Unpowered), new PowerVar<ThornsPower>(2m)];

    private int BaseThreshold => ByAct(Tier, 30, 45, 60);

    public override decimal InitialAmount(int tier) => ByAct(tier, 30, 45, 60);

    public override Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars.Block.BaseValue = ByAct(Tier, 15, 20, 30);
        DynamicVars["ThornsPower"].BaseValue = ByAct(Tier, 2, 3, 4);
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || !Owner.IsAlive || result.UnblockedDamage <= 0)
        {
            return;
        }
        if (Amount > result.UnblockedDamage)
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -result.UnblockedDamage, Owner, null, silent: true);
            return;
        }
        // Shift: threshold rises by 10 each time, like the original.
        _shifts++;
        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, BaseThreshold + 10 * _shifts - Amount, Owner, null, silent: true);
        await CreatureCmd.GainBlock(Owner, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
        int thorns = DynamicVars["ThornsPower"].IntValue;
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, thorns, Owner, null);
        _thornsGranted += thorns;
        _thornsTurnsLeft = 2;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy || _thornsGranted == 0 || --_thornsTurnsLeft > 0)
        {
            return;
        }
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, -_thornsGranted, Owner, null);
        _thornsGranted = 0;
    }
}

/// <summary>Trial of the Champion. Second Wind: at half HP, cleanse debuffs and gain Strength and Plating.</summary>
public sealed class SecondWindPower : TrialPower
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(5m)];

    public override decimal InitialAmount(int tier) => ByAct(tier, 3, 4, 5);

    public override Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars["PlatingPower"].BaseValue = ByAct(Tier, 5, 8, 10);
        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || !Owner.IsAlive || Owner.CurrentHp * 2 > Owner.MaxHp)
        {
            return;
        }
        Flash();
        await RemoveDebuffs();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, DynamicVars["PlatingPower"].BaseValue, Owner, null);
        await PowerCmd.Remove(this);
    }
}

/// <summary>
/// Trial of the Automaton. Stasis: starts with Artifact; at the start of combat and every 4 rounds it takes the
/// highest-cost card from each player's hand. Taken cards come back when it drops to half HP or dies.
/// </summary>
public sealed class StasisPower : TrialPower
{
    private const int Period = 4;
    private readonly List<CardModel> _held = [];
    private bool _released;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ArtifactPower>(1m)];

    public override decimal InitialAmount(int tier) => 1m;

    public override async Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars["ArtifactPower"].BaseValue = ByAct(Tier, 1, 2, 3);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner, DynamicVars["ArtifactPower"].BaseValue, Owner, null);
    }

    // After each player's hand is drawn.
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (_released || !Owner.IsAlive || player.PlayerCombatState is not { } state || (state.TurnNumber - 1) % Period != 0)
        {
            return;
        }
        CardModel? taken = state.Hand.Cards.Where(c => c.EnergyCost.Canonical >= 0).OrderByDescending(c => c.EnergyCost.Canonical).FirstOrDefault();
        if (taken == null)
        {
            return;
        }
        Flash();
        _held.Add(taken);
        await CardPileCmd.Add(taken, PileType.Exhaust);
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && Owner.CurrentHp * 2 <= Owner.MaxHp)
        {
            await Release();
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature == Owner)
        {
            await Release();
        }
    }

    private async Task Release()
    {
        if (_released)
        {
            return;
        }
        _released = true;
        foreach (CardModel card in _held.Where(c => c.Pile?.Type == PileType.Exhaust))
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
        _held.Clear();
    }
}

/// <summary>Trial of the Collector. Mega Debuff: on its 3rd turn, every player gains Vulnerable, Weak and Frail.</summary>
public sealed class MegaDebuffPower : TrialPower
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Turns", 2m)];

    public override decimal InitialAmount(int tier) => 3m;

    public override Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars["Turns"].BaseValue = ByAct(Tier, 2, 3, 5);
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy || !participants.Contains(Owner))
        {
            return;
        }
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        Flash();
        decimal turns = DynamicVars["Turns"].BaseValue;
        List<Creature> players = LivingPlayers.ToList();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, players, turns, Owner, null);
        await PowerCmd.Apply<WeakPower>(choiceContext, players, turns, Owner, null);
        await PowerCmd.Apply<FrailPower>(choiceContext, players, turns, Owner, null);
        await PowerCmd.Remove(this);
    }
}

/// <summary>Trial of the Shapes. Square and Circle: every 2nd turn, gains Strength and Dazes each player.</summary>
public sealed class SquareAndCirclePower : TrialPower
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override decimal InitialAmount(int tier) => ByAct(tier, 1, 2, 2);

    public override Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars.Cards.BaseValue = ByAct(Tier, 1, 1, 2);
        return Task.CompletedTask;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Enemy || !participants.Contains(Owner) || combatState.RoundNumber % 2 != 0)
        {
            return;
        }
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        await AddToEachPlayer<Dazed>(PileType.Draw, DynamicVars.Cards.IntValue);
    }
}

/// <summary>
/// Trial of the Time Eater. Time Warp: counts every card any player plays; on the Nth, it gains Strength and the
/// player who played it ends their turn (same call vanilla's Void Form uses).
/// </summary>
public sealed class TimeWarpPower : TrialPower
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m), new CardsVar(12)];

    public override decimal InitialAmount(int tier) => ByAct(tier, 12, 12, 10);

    public override Task Configure(PlayerChoiceContext choiceContext)
    {
        DynamicVars["StrengthPower"].BaseValue = ByAct(Tier, 1, 2, 2);
        DynamicVars.Cards.BaseValue = ByAct(Tier, 12, 12, 10);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!Owner.IsAlive || cardPlay.Card.Owner.Creature.IsEnemy)
        {
            return;
        }
        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, DynamicVars.Cards.BaseValue - Amount, Owner, null, silent: true);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, DynamicVars["StrengthPower"].BaseValue, Owner, null);
        PlayerCmd.EndTurn(cardPlay.Card.Owner, canBackOut: false);
    }
}

/// <summary>
/// Trial of the Awakened One. Rebirth: the first time it would die, it rises at X% Max HP, cleansed, then spends its
/// next turn Stunned and Intangible, so the killing turn's leftover damage barely touches it.
/// </summary>
public sealed class RebirthPower : TrialPower
{
    private bool _used;

    // True while the boss sits at 0 HP mid-rebirth. With no living enemy the game counts combat as ending, and while
    // it's ending Heal and PowerCmd do nothing to monsters, so the revive would silently fail and the boss would die.
    // Vanilla revivers (AdaptablePower, MockRevivePower) hold combat open the same way.
    private bool _reviving;

    public override decimal InitialAmount(int tier) => ByAct(tier, 25, 35, 50);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<IntangiblePower>(), HoverTipFactory.Static(StaticHoverTip.Stun)];

    // Same pattern as vanilla's Lizard Tail.
    public override bool ShouldDieLate(Creature creature) => creature != Owner || _used;

    public override bool ShouldStopCombatFromEnding() => _reviving;

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || _used)
        {
            return;
        }
        _used = true;
        _reviving = true;
        try
        {
            Flash();
            await RemoveDebuffs();
            await CreatureCmd.Heal(Owner, Math.Max(1m, Math.Floor(Owner.MaxHp * Amount / 100m)));
            // Intangible 1 wears off at the end of the enemy turn, the same turn the stun is spent.
            await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Owner, 1m, Owner, null);
            await Stun();
        }
        finally
        {
            _reviving = false;
        }
        await PowerCmd.Remove(this);
    }

    // Skip the coming enemy turn, then carry on with the move it had queued.
    private async Task Stun()
    {
        if (Owner.Monster is not { } monster || Owner.IsStunned)
        {
            return;
        }
        string? queued = monster.NextMove.Id;
        if (queued != null && monster.MoveStateMachine.States.ContainsKey(queued))
        {
            await CreatureCmd.Stun(Owner, queued);
        }
        else if (monster.MoveStateMachine.StateLog.Count > 0)
        {
            await CreatureCmd.Stun(Owner);
        }
    }
}
