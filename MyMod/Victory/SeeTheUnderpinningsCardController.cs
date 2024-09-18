using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Victory
{
    public class SeeTheUnderpinningsCardController : CardController
    {
        public SeeTheUnderpinningsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Reveal the top 3 cards of a deck."
            List<SelectLocationDecision> locationDecisions = new List<SelectLocationDecision>();
            IEnumerator selectDeckCoroutine = GameController.SelectADeck(DecisionMaker, SelectionType.RevealCardsFromDeck, (Location l) => l.HasCards, locationDecisions, noValidLocationsMessage: "There are no decks with cards in them for " + Card.Title + " to reveal from.", cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectDeckCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectDeckCoroutine);
            }
            if (DidSelectDeck(locationDecisions))
            {
                Location deck = GetSelectedLocation(locationDecisions);
                List<Card> revealed = new List<Card>();
                IEnumerator revealCoroutine = GameController.RevealCards(DecisionMaker, deck, 3, revealed, revealedCardDisplay: RevealedCardDisplay.None, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(revealCoroutine);
                }
                // "Shuffle 2 of them back into that deck."
                SelectCardsDecision choice = new SelectCardsDecision(GameController, DecisionMaker, (Card c) => revealed.Contains(c), SelectionType.ShuffleCardIntoDeck, numberOfCards: 2, requiredDecisions: 2, eliminateOptions: true, cardSource: GetCardSource());
                List<SelectCardDecision> chosen = new List<SelectCardDecision>();
                IEnumerator shuffleCoroutine = GameController.SelectCardsAndDoAction(choice, (SelectCardDecision scd) => GameController.ShuffleCardIntoLocation(DecisionMaker, scd.SelectedCard, deck, false, cardSource: GetCardSource()), storedResults: chosen, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(shuffleCoroutine);
                }
                foreach (SelectCardDecision scd in chosen)
                {
                    revealed.Remove(scd.SelectedCard);
                }
                // "Put the remaining card on the top or bottom of that deck."
                Card remaining = revealed.FirstOrDefault();
                if (remaining != null)
                {
                    MoveCardDestination[] options = new MoveCardDestination[2] { new MoveCardDestination(deck, toBottom: false, showMessage: true), new MoveCardDestination(deck, toBottom: true, showMessage: true) };
                    IEnumerator moveCoroutine = GameController.SelectLocationAndMoveCard(DecisionMaker, remaining, options, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
            }
        }
    }
}
