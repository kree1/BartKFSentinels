using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class HuddleUpCardController : TheGoalieUtilityCardController
    {
        public HuddleUpCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Reveal the top 5 cards of your deck."
            List<Card> revealedCards = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, 5, revealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // If not enough cards were available, inform the player
            string countMessage = null;
            switch (revealedCards.Count)
            {
                case 5:
                    break;
                case 0:
                    countMessage = "No cards were revealed!";
                    break;
                case 1:
                    countMessage = "Only one card was revealed! It will automatically be put into " + base.CharacterCard.Title + "'s hand.";
                    break;
                default:
                    countMessage = "Only " + revealedCards.Count.ToString() + " cards were revealed!";
                    break;
            }
            if (countMessage != null)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(countMessage, Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            if (revealedCards.Count > 0)
            {
                // "Put 1 into your hand..."
                List<MoveCardAction> toHand = new List<MoveCardAction>();
                IEnumerator handCoroutine = base.GameController.SelectCardsFromLocationAndMoveThem(base.HeroTurnTakerController, base.TurnTaker.Revealed, new int?(1), 1, new LinqCardCriteria(), new MoveCardDestination[] { new MoveCardDestination(base.HeroTurnTaker.Hand) }, storedResultsMove: toHand, responsibleTurnTaker: base.TurnTaker, selectionType: SelectionType.MoveCardToHand, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(handCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(handCoroutine);
                }

                Card inHand = null;
                if (DidMoveCard(toHand))
                {
                    inHand = toHand.FirstOrDefault().CardToMove;
                    revealedCards.Remove(inHand);
                }
                // "... and the rest into your trash."
                int numCardsLeft = revealedCards.Count;
                if (numCardsLeft > 0)
                {
                    Location heroRevealed = base.TurnTaker.Revealed;
                    IEnumerator trashCoroutine = base.GameController.SelectCardsFromLocationAndMoveThem(base.HeroTurnTakerController, heroRevealed, numCardsLeft, numCardsLeft, new LinqCardCriteria(), new MoveCardDestination[] { new MoveCardDestination(base.TurnTaker.Trash) }, responsibleTurnTaker: base.TurnTaker, allowAutoDecide: true, selectionType: SelectionType.MoveCardToTrash, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(trashCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(trashCoroutine);
                    }
                }
            }
            // "You may play a card."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, optional: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }
    }
}
