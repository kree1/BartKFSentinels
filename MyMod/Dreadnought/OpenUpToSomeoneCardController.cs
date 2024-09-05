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
            // "At the start of your turn, you may destroy one of your Ongoing cards. If you do, draw 2 cards and {Dreadnought} regains 2 HP."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyDrawHealResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DrawCard, TriggerType.GainHP });
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int cardsInstructed = GetPowerNumeral(0, 2);
            int cardsRequired = GetPowerNumeral(1, 2);
            int cardsToDraw = GetPowerNumeral(2, 3);
            // "Put the bottom 2 cards of your trash on the bottom of your deck."// "If you moved 2 cards this way, one player draws 3 cards."
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

        IEnumerator DestroyDrawHealResponse(PhaseChangeAction pca)
        {
            // "... you may destroy one of your Ongoing cards."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner == TurnTaker && c.IsInPlayAndHasGameText, "belonging to " + TurnTaker.Name + " in play", useCardsPrefix: true, useCardsSuffix: false, singular: "Ongoing card", plural: "Ongoing cards"), true, results, responsibleCard: Card, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If you do, draw 2 cards and {Dreadnought} regains 2 HP."
            if (DidDestroyCard(results))
            {
                IEnumerator drawCoroutine = DrawCards(DecisionMaker, 2);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(drawCoroutine);
                }
                IEnumerator healCoroutine = GameController.GainHPEx(CharacterCard, 2, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(healCoroutine);
                }
            }
        }
    }
}
