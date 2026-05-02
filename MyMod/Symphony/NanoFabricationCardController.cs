using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class NanoFabricationCardController : CardController
    {
        public NanoFabricationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of non-one-shot cards in Symphony's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"));
            // Show list of non-one-shot cards in Symphony's trash
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Trash, new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"));
        }

        public IEnumerator SearchForCardsDoubleEx(TurnTakerController turnTakerController, bool searchDeck, bool searchTrash, int? minNumberOfCards, int maxNumberOfCards, LinqCardCriteria cardCriteria, bool putIntoPlay, bool putInHand, bool putOnDeck, bool optional = false, List<SelectCardDecision> storedResults = null, List<MoveCardAction> storedResultsMove = null, bool autoDecideCard = false, bool? shuffleAfterwards = null, Location overrideDestination = null)
        {
            List<MoveCardDestination> possibleDestinations = new List<MoveCardDestination>();
            if (overrideDestination == null)
            {
                if (putIntoPlay)
                {
                    possibleDestinations.Add(new MoveCardDestination(turnTakerController.TurnTaker.PlayArea));
                }
                if (putInHand && turnTakerController.IsPlayer)
                {
                    HeroTurnTakerController heroTurnTakerController = turnTakerController.ToHero();
                    possibleDestinations.Add(new MoveCardDestination(heroTurnTakerController.HeroTurnTaker.Hand));
                }
                if (putOnDeck)
                {
                    possibleDestinations.Add(new MoveCardDestination(turnTakerController.TurnTaker.Deck));
                }
            }
            else
            {
                possibleDestinations.Add(new MoveCardDestination(overrideDestination));
            }
            HeroTurnTakerController decisionMaker = (turnTakerController as HeroTurnTakerController) ?? DecisionMaker;
            Location location;
            if (searchDeck && !searchTrash)
            {
                location = turnTakerController.TurnTaker.Deck;
            }
            else if (searchTrash && !searchDeck)
            {
                location = turnTakerController.TurnTaker.Trash;
            }
            else
            {
                List<SelectLocationDecision> storedLocation = new List<SelectLocationDecision>();
                IEnumerator coroutine = GameController.SelectLocation(decisionMaker, new LocationChoice[2]
                {
            new LocationChoice(turnTakerController.TurnTaker.Deck),
            new LocationChoice(turnTakerController.TurnTaker.Trash)
                }, SelectionType.SearchLocation, storedLocation, optional: false, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
                location = storedLocation.FirstOrDefault()?.SelectedLocation.Location;
            }
            if (location != null)
            {
                GameController gameController = GameController;
                Location location2 = location;
                bool valueOrDefault = shuffleAfterwards.GetValueOrDefault(location.IsDeck);
                CardSource cardSource = GetCardSource();
                IEnumerator coroutine2 = gameController.SelectCardsFromLocationAndMoveThem(decisionMaker, location2, minNumberOfCards, maxNumberOfCards, cardCriteria, possibleDestinations, putIntoPlay, playIfMovingToPlayArea: true, valueOrDefault, optional, storedResults, storedResultsMove, autoDecideCard, flipFaceDown: false, showOutput: false, null, isDiscardIfMovingToTrash: false, allowAutoDecide: false, null, null, cardSource);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine2);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine2);
                }
            }
            else
            {
                Log.Warning("[" + Card.Title + "]: SearchForCards was called with no origin to search from.");
            }
        }

        public override IEnumerator Play()
        {
            // "Search your deck and trash for a non-one-shot card and put it into your hand or into play. If you searched your deck, shuffle it."
            List<MoveCardAction> moveResults = new List<MoveCardAction>();
            IEnumerator searchCoroutine = SearchForCardsDoubleEx(TurnTakerController, true, true, 1, 1, new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"), true, true, false, storedResultsMove: moveResults);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(searchCoroutine);
            }
            if (!moveResults.Any((MoveCardAction mca) => mca.WasCardMoved && mca.Destination.IsInPlay))
            {
                // "If no card entered play this way, another player may discard a card."
                List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
                IEnumerator discardCoroutine = GameController.SelectHeroToDiscardCard(DecisionMaker, additionalHeroCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != TurnTaker), storedResultsDiscard: discardResults, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(discardCoroutine);
                }
                if (DidDiscardCards(discardResults))
                {
                    // "If they do, they may play a non-one-shot card."
                    DiscardCardAction success = discardResults.FirstOrDefault((DiscardCardAction dca) => DidDiscardCards(dca.ToEnumerable()));
                    TurnTaker tt = success.ResponsibleTurnTaker;
                    IEnumerator playCoroutine = GameController.SelectAndPlayCardFromHand(FindTurnTakerController(tt).ToHero(), true, cardCriteria: new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"), cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
        }
    }
}
