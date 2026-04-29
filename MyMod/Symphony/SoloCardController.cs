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
    public class SoloCardController : CardController
    {
        public SoloCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override bool DoNotMoveOneShotToTrash => Card.Location.IsHand;

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            Card c = Card;
            if (decision is YesNoCardDecision ync)
            {
                c = ync.Card;
            }
            return new CustomDecisionText("Do you want to return " + c.Title + " to your hand?", "deciding whether to return " + c.Title + " to their hand", "Vote for whether to return " + c.Title + " to " + decision.DecisionMaker.Name + "'s hand", "return this card to your hand");
        }

        public override IEnumerator Play()
        {
            // "One hero may use a power."
            IEnumerator powerCoroutine = GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(powerCoroutine);
            }
            // "You may return this card to your hand."
            YesNoCardDecision yn = new YesNoCardDecision(GameController, DecisionMaker, SelectionType.Custom, Card, cardSource: GetCardSource());
            IEnumerator decideCoroutine = GameController.MakeDecisionAction(yn);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(decideCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(decideCoroutine);
            }
            if (DidPlayerAnswerYes(yn))
            {
                IEnumerator moveCoroutine = GameController.MoveCard(TurnTakerController, Card, HeroTurnTaker.Hand, decisionSources: yn.ToEnumerable(), cardSource: GetCardSource());
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
