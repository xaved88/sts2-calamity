using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Calamity.CalamityCode.Hardships;

// Cards that hardships put in your deck. All opt out of vanilla's random curse/status generation.

/// <summary>Ballast Box. Status, costs 1: playing it removes it from your deck for good.</summary>
[Pool(typeof(StatusCardPool))]
public sealed class Weight() : CustomCardModel(1, CardType.Status, CardRarity.Status, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override string? CustomPortraitPath => "res://Calamity/images/cards/weight.png";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DeckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(DeckVersion);
        }
    }
}

/// <summary>Hungry Jar. Curse, Unplayable, Eternal: at the end of your turn, if it's in your hand, lose 3 HP.</summary>
[Pool(typeof(CurseCardPool))]
public sealed class Gnawing() : CustomCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Unblockable | ValueProp.Unpowered)];

    public override string? CustomPortraitPath => "res://Calamity/images/cards/gnawing.png";

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.Damage.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, null, this);
    }
}

/// <summary>Kudzu Cutting. Curse, Unplayable. Just takes up space, and more keeps arriving.</summary>
[Pool(typeof(CurseCardPool))]
public sealed class Kudzu() : CustomCardModel(-1, CardType.Curse, CardRarity.Curse, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public override string? CustomPortraitPath => "res://Calamity/images/cards/kudzu.png";
}
