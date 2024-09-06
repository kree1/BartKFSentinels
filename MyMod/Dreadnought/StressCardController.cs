using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class StressCardController : DreadnoughtUtilityCardController
    {
        public StressCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of cards in Dreadnought's trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Trash);
        }

        public IEnumerator PayStress(int numCards)
        {
            yield return PayStress(numCards, numCards, numCards + 1);
        }

        public IEnumerator PayStress(int cardsInstructed, int cardsRequired, int damageAmt)
        {
            // "Put the bottom [cardsInstructed] cards of your trash on the bottom of your deck."
            List<MoveCardAction> moved = new List<MoveCardAction>();
            IEnumerable<Card> toMove = TurnTaker.Trash.Cards.Take(cardsInstructed);
            IEnumerator moveCoroutine = GameController.SendMessageAction("There are no cards in " + TurnTaker.Name + "'s trash for " + Card.Title + " to move.", Priority.Medium, GetCardSource());
            if (toMove.Any())
            {
                moveCoroutine = GameController.MoveCards(TurnTakerController, toMove, TurnTaker.Deck, toBottom: true, responsibleTurnTaker: TurnTaker, storedResultsAction: moved, cardSource: GetCardSource());
            }
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(moveCoroutine);
            }
            // "If you moved fewer than [cardsRequired] cards this way, {Dreadnought} deals herself [damageAmt] irreducible psychic damage."
            IEnumerable<Card> wasMoved = (from MoveCardAction mca in moved where mca.WasCardMoved select mca.CardToMove).Distinct();
            if (wasMoved.Any())
            {
                IEnumerator announceCoroutine = GameController.SendMessageAction(Card.Title + " moved " + wasMoved.Count().ToString() + " " + wasMoved.Count().ToString_CardOrCards() + " from " + TurnTaker.Name + "'s trash to the bottom of " + TurnTaker.Name + "'s deck.", Priority.Low, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(announceCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(announceCoroutine);
                }
            }
            if (wasMoved.Count() < cardsRequired)
            {
                IEnumerator psychicCoroutine = DealDamage(CharacterCard, CharacterCard, damageAmt, DamageType.Psychic, isIrreducible: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(psychicCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(psychicCoroutine);
                }
            }
        }
    }
}
