using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Pact;

/// <summary>
/// Curse. Unplayable. Eternal. When drawn, take X damage, where X is how many times you've pulled someone back from
/// death. One card per bearer; each further revive raises X on it rather than adding another.
/// </summary>
[Pool(typeof(CurseCardPool))]
public sealed class UnholyPact() : CustomCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    private int _severity = 1;

    public override int MaxUpgradeLevel => 0;

    /// <summary>Never handed out by vanilla's random-curse effects (they all filter on this).</summary>
    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1m, ValueProp.Unpowered)];

    public override string? CustomPortraitPath => "res://Calamity/images/cards/unholy_pact.png";

    [SavedProperty]
    public int Severity
    {
        get => _severity;
        set
        {
            AssertMutable();
            _severity = value;
            DynamicVars.Damage.BaseValue = value;
        }
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != this)
        {
            return;
        }
        await Cmd.Wait(0.25f);
        await CreatureCmd.Damage(choiceContext, Owner.Creature, Severity, ValueProp.Unpowered, null, this);
    }

    /// <summary>Adds the Pact to the bearer's deck, or deepens every Pact they already carry.</summary>
    public static async Task Bind(Player bearer)
    {
        List<UnholyPact> existing = bearer.Deck.Cards.OfType<UnholyPact>().ToList();
        if (existing.Count == 0)
        {
            await CardPileCmd.AddCurseToDeck<UnholyPact>(bearer);
            return;
        }
        foreach (UnholyPact pact in existing)
        {
            pact.Severity++;
        }
        CardCmd.PreviewCardPileAdd(existing.Select(p => new CardPileAddResult { success = true, cardAdded = p }).ToList(), 2f);
    }
}
