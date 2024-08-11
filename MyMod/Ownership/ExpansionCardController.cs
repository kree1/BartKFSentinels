using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class ExpansionCardController : CardController
    {
        public ExpansionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _expansionTeam = null;
            _powerUser = null;
            _cardSources = null;
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            AddThisCardControllerToList(CardControllerListType.ReplacesCards);
            AddThisCardControllerToList(CardControllerListType.ReplacesCardSource);
            AddThisCardControllerToList(CardControllerListType.ReplacesTurnTakerController);
        }

        private Card _expansionTeam;
        private Card _powerUser;
        private Dictionary<Power, CardSource> _cardSources;

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible."
            if (card == base.Card)
                return true;
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, if the character next to this card is active, one player plays the top card of their deck and another player's hero may use the power on this character."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, ExpansionTeamActionsResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.UsePower }, additionalCriteria: (PhaseChangeAction pca) => base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.Location == base.Card.NextToLocation && c.IsActive), visibleToCard: GetCardSource()).Any());
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, choose a hero character card from the box and put it into play next to this card."
            // Copied from Representative of Earth
            Func<string, bool> identifierCriteria = (string s) => !FindCardsWhere((Card c) => c.QualifiedPromoIdentifierOrIdentifier == s && !c.Location.IsInTheBox, realCardsOnly: false, null, ignoreBattleZone: true).Any();
            Func<string, bool> turnTakerCriteria = (string tt) => true;
            List<SelectFromBoxDecision> storedResults = new List<SelectFromBoxDecision>();
            IEnumerator coroutine = base.GameController.SelectFromBox(DecisionMaker, identifierCriteria, turnTakerCriteria, SelectionType.HeroCharacterCard, storedResults, optional: false, allowMultiCardSelection: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            CardController cardController = null;
            SelectFromBoxDecision selection = storedResults.FirstOrDefault();
            if (selection != null && selection.SelectedIdentifier != null && selection.SelectedTurnTakerIdentifier != null)
            {
                Log.Debug("Selected from box: SelectedIdentifier = " + selection.SelectedIdentifier);
                Log.Debug("Selected from box: SelectedTurnTakerIdentifier = " + selection.SelectedTurnTakerIdentifier);
                Card modelCard = FindCardsWhere((Card c) => c.QualifiedPromoIdentifierOrIdentifier == selection.SelectedIdentifier && c.Location.IsInTheBox).FirstOrDefault();
                if (modelCard == null)
                {
                    // Edited to use the environment TurnTakerController and avoid making the new hero a villain by association
                    TurnTakerController turnTakerController = base.GameController.FindEnvironmentTurnTakerController();
                    DeckDefinition deckDefinition = DeckDefinitionCache.GetDeckDefinition(selection.SelectedTurnTakerIdentifier);
                    CardDefinition cardDefinition = (from cd in deckDefinition.CardDefinitions.Concat(deckDefinition.PromoCardDefinitions)
                                                     where cd.QualifiedPromoIdentifierOrIdentifier == selection.SelectedIdentifier
                                                     select cd).FirstOrDefault();
                    if (cardDefinition == null)
                    {
                        cardDefinition = ModHelper.GetPromoDefinition(selection.SelectedTurnTakerIdentifier, selection.SelectedIdentifier);
                    }
                    Log.Debug("Card definition: " + cardDefinition);
                    if (cardDefinition != null)
                    {
                        modelCard = new Card(cardDefinition, turnTakerController.TurnTaker, 0, selection.UseFoilVersion);
                        turnTakerController.TurnTaker.OffToTheSide.AddCard(modelCard);
                        string overrideNamespace = selection.SelectedTurnTakerIdentifier;
                        if (!string.IsNullOrEmpty(cardDefinition.Namespace))
                        {
                            overrideNamespace = $"{cardDefinition.Namespace}.{deckDefinition.Identifier}";
                        }
                        cardController = CardControllerFactory.CreateInstance(modelCard, turnTakerController, overrideNamespace);
                        turnTakerController.AddCardController(cardController);
                        List<string> list = new List<string>();
                        list.Add(selection.SelectedTurnTakerIdentifier);
                        list.Add(selection.SelectedIdentifier);
                        base.GameController.AddCardPropertyJournalEntry(modelCard, "OverrideTurnTaker", list);
                        if (modelCard.Definition.SharedIdentifier != null)
                        {
                            IEnumerable<CardDefinition> enumerable = from cd in deckDefinition.GetAllCardDefinitions()
                                                                     where cd.QualifiedPromoIdentifierOrIdentifier != modelCard.QualifiedPromoIdentifierOrIdentifier && cd.SharedIdentifier == modelCard.SharedIdentifier
                                                                     select cd;
                            if (modelCard.IsPromoCard && modelCard.IsModContent)
                            {
                                IEnumerable<CardDefinition> second = from cd in ModHelper.GetSharedPromoDefinitions(cardDefinition)
                                                                     where cd.QualifiedPromoIdentifierOrIdentifier != modelCard.QualifiedPromoIdentifierOrIdentifier
                                                                     select cd;
                                enumerable = enumerable.Concat(second);
                            }
                            foreach (CardDefinition item in enumerable)
                            {
                                Card card = new Card(item, turnTakerController.TurnTaker, 0, selection.UseFoilVersion);
                                turnTakerController.TurnTaker.OffToTheSide.AddCard(card);
                                overrideNamespace = modelCard.ParentDeck.QualifiedIdentifier;
                                if (!string.IsNullOrEmpty(item.Namespace))
                                {
                                    overrideNamespace = $"{item.Namespace}.{deckDefinition.Identifier}";
                                }
                                CardController card2 = CardControllerFactory.CreateInstance(card, turnTakerController, overrideNamespace);
                                turnTakerController.AddCardController(card2);
                                List<string> list2 = new List<string>();
                                list2.Add(modelCard.ParentDeck.QualifiedIdentifier);
                                list2.Add(card.QualifiedPromoIdentifierOrIdentifier);
                                base.GameController.AddCardPropertyJournalEntry(card, "OverrideTurnTaker", list2);
                            }
                        }
                    }
                    else
                    {
                        Log.Error("Could not find card definition!");
                    }
                }
                else
                {
                    cardController = FindCardController(modelCard);
                }
            }
            if (cardController != null)
            {
                GameController gameController = base.GameController;
                TurnTakerController turnTakerController2 = base.TurnTakerController;
                Card card3 = cardController.Card;
                Location nextToLocation = base.Card.NextToLocation;
                CardSource cardSource = GetCardSource();
                coroutine = gameController.PlayCard(turnTakerController2, card3, isPutIntoPlay: true, null, optional: false, nextToLocation, null, evenIfAlreadyInPlay: false, null, null, null, associateCardSource: false, fromBottom: false, canBeCancelled: true, cardSource);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }
            else
            {
                Log.Error("Could not find or create card!");
            }
        }

        public IEnumerator ExpansionTeamActionsResponse(PhaseChangeAction pca)
        {
            // "... one player plays the top card of their deck..."
            List<SelectTurnTakerDecision> results = new List<SelectTurnTakerDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectTurnTaker(DecisionMaker, SelectionType.PlayTopCard, results, additionalCriteria: (TurnTaker tt) => tt.IsPlayer, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            SelectTurnTakerDecision choice = results.FirstOrDefault((SelectTurnTakerDecision sttd) => sttd.SelectedTurnTaker != null);
            TurnTaker player = null;
            if (choice != null)
            {
                player = choice.SelectedTurnTaker;
                IEnumerator playCoroutine = base.GameController.PlayTopCard(base.GameController.FindHeroTurnTakerController(player.ToHero()), base.GameController.FindTurnTakerController(player), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            // "... and another player's hero may use the power on this character."
            // Copied from Called to Judgement
            if (_expansionTeam == null)
            {
                // Edited to use this card instead of Representative of Earth
                _expansionTeam = base.Card.NextToLocation.TopCard;
            }
            if (_expansionTeam != null)
            {
                _cardSources = new Dictionary<Power, CardSource>();
                List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                // Edited to exclude heroes belonging to the player who played their top card
                IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.UsePowerOnCard, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsHeroCharacterCard(c) && c.Location.IsHeroPlayAreaRecursive && !c.IsIncapacitatedOrOutOfGame && c.Owner != player, "heroes in this battle zone", useCardsSuffix: false), storedResults, optional: true, allowAutoDecide: false, null, includeRealCardsOnly: true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                SelectCardDecision selectCardDecision = storedResults.FirstOrDefault();
                if (selectCardDecision != null && selectCardDecision.SelectedCard != null)
                {
                    _powerUser = selectCardDecision.SelectedCard;
                    HeroCharacterCardController heroCharacterCardUsingPower = FindCardController(_powerUser) as HeroCharacterCardController;
                    HeroTurnTakerController heroController = FindHeroTurnTakerController(selectCardDecision.SelectedCard.Owner.ToHero());
                    Log.Debug("_expansionTeam.IsHeroCharacterCard: " + _expansionTeam.IsHeroCharacterCard.ToString());
                    coroutine = base.GameController.SelectAndUsePowerEx(heroController, optional: true, (Power power) => power.CardSource != null && power.CardSource.Card == _expansionTeam, 1, eliminateUsedPowers: false, null, showMessage: false, allowAnyHeroPower: true, allowReplacements: true, canBeCancelled: true, null, forceDecision: false, allowOutOfPlayPower: false, GetCardSource(), heroCharacterCardUsingPower);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(coroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(coroutine);
                    }
                }
            }
            _expansionTeam = null;
            _powerUser = null;
            _cardSources = null;
        }

        // All subsequent methods copied from Called to Judgement
        public override Card AskIfCardIsReplaced(Card card, CardSource cardSource)
        {
            //Log.Debug("ExpansionCardController.AskIfCardIsReplaced called");
            if (_expansionTeam != null && _powerUser != null && card.IsHeroCharacterCard && cardSource.AllowReplacements)
            {
                CardController value = FindCardController(_expansionTeam);
                IEnumerable<CardController> source = cardSource.CardSourceChain.Select((CardSource cs) => cs.CardController);
                if (source.Contains(value) && source.Contains(this) && _expansionTeam == cardSource.Card && card == cardSource.CardController.CardWithoutReplacements)
                {
                    //Log.Debug("ExpansionCardController.AskIfCardIsReplaced returning "  + _powerUser.Title);
                    return _powerUser;
                }
            }
            //Log.Debug("ExpansionCardController.AskIfCardIsReplaced returning null");
            return null;
        }

        public override TurnTakerController AskIfTurnTakerControllerIsReplaced(TurnTakerController ttc, CardSource cardSource)
        {
            //Log.Debug("ExpansionCardController.AskIfTurnTakerControllerIsReplaced called");
            if (_expansionTeam != null && _powerUser != null && cardSource.AllowReplacements)
            {
                TurnTakerController turnTakerControllerWithoutReplacements = FindCardController(_expansionTeam).TurnTakerControllerWithoutReplacements;
                if (ttc == turnTakerControllerWithoutReplacements && (cardSource.CardController.CardWithoutReplacements == _expansionTeam || cardSource.CardSourceChain.Any((CardSource cs) => cs.CardController == this)))
                {
                    //Log.Debug("ExpansionCardController.AskIfTurnTakerControllerIsReplaced returning TurnTakerController for " + _powerUser.Owner.Name);
                    return FindTurnTakerController(_powerUser.Owner);
                }
            }
            //Log.Debug("ExpansionCardController.AskIfTurnTakerControllerIsReplaced returning null");
            return null;
        }

        public override CardSource AskIfCardSourceIsReplaced(CardSource cardSource, GameAction gameAction = null, ITrigger trigger = null)
        {
            //Log.Debug("ExpansionCardController.AskIfCardSourceIsReplaced called");
            if (_expansionTeam != null && _powerUser != null && cardSource.AllowReplacements && FindCardController(_expansionTeam).CardWithoutReplacements == cardSource.CardController.CardWithoutReplacements)
            {
                cardSource.AddAssociatedCardSource(GetCardSource());
                //Log.Debug("ExpansionCardController.AskIfCardSourceIsReplaced returning modified cardSource");
                return cardSource;
            }
            //Log.Debug("ExpansionCardController.AskIfCardSourceIsReplaced returning null");
            return null;
        }

        public override void PrepareToUsePower(Power power)
        {
            Log.Debug("ExpansionCardController.PrepareToUsePower called");
            base.PrepareToUsePower(power);
            if (ShouldAssociateThisCard(power))
            {
                Log.Debug("ExpansionCardController.PrepareToUsePower modifying CardSources");
                _cardSources.Add(power, GetCardSource());
                power.CardController.AddAssociatedCardSource(_cardSources[power]);
            }
        }

        public override void FinishUsingPower(Power power)
        {
            Log.Debug("ExpansionCardController.FinishUsingPower called");
            base.FinishUsingPower(power);
            if (ShouldAssociateThisCard(power))
            {
                Log.Debug("ExpansionCardController.FinishUsingPower removing CardSource modifications");
                power.CardController.RemoveAssociatedCardSource(_cardSources[power]);
                _cardSources.Remove(power);
            }
        }

        private bool ShouldAssociateThisCard(Power power)
        {
            Log.Debug("ExpansionCardController.ShouldAssociateThisCard called");
            if (FindCardController(_expansionTeam) == power.CardController)
            {
                Log.Debug("ExpansionCardController.ShouldAssociateThisCard returning " + (power.CardSource != null).ToString());
                return power.CardSource != null;
            }
            Log.Debug("ExpansionCardController.ShouldAssociateThisCard returning false");
            return false;
        }
    }
}
