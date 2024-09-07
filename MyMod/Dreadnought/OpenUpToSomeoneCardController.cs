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
    public class OpenUpToSomeoneCardController : StressCardController
    {
        public OpenUpToSomeoneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may discard a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => GameController.SelectAndDiscardCard(DecisionMaker, optional: true, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource()), TriggerType.DiscardCard);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int cardsInstructed = GetPowerNumeral(0, 2);
            int cardsRequired = GetPowerNumeral(1, 2);
            int cardsToDraw = GetPowerNumeral(2, 3);
            // "Put the bottom 2 cards of your trash on the bottom of your deck."
            List<MoveCardAction> moves = new List<MoveCardAction>();
            IEnumerable<Card> toMove = TurnTaker.Trash.Cards.Take(cardsInstructed);
            IEnumerator moveCoroutine = GameController.SendMessageAction("There are no cards in " + TurnTaker.Name + "'s trash for " + Card.Title + " to move.", Priority.Medium, GetCardSource());
            if (toMove.Any())
            {
                moveCoroutine = GameController.MoveCards(TurnTakerController, toMove, TurnTaker.Deck, toBottom: true, responsibleTurnTaker: TurnTaker, storedResultsAction: moves, cardSource: GetCardSource());
            }
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(moveCoroutine);
            }
            IEnumerable<Card> wasMoved = (from MoveCardAction mca in moves where mca.WasCardMoved select mca.CardToMove).Distinct();
            // "If you moved 2 cards this way, one player draws 3 cards."
            if (wasMoved.Count() >= cardsRequired)
            {
                IEnumerator drawCoroutine = GameController.SelectHeroToDrawCards(DecisionMaker, cardsToDraw, optionalDrawCards: false, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
        }
    }
}
