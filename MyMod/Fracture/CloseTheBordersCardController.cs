using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class CloseTheBordersCardController : FractureUtilityCardController
    {
        public CloseTheBordersCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(BreachCard());
        }

        public override IEnumerator Play()
        {
            // "You may destroy an Ongoing or environment card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing || c.IsEnvironment, "ongoing or environment"), true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Destroy all Breach cards. For each card destroyed this way, you may either destroy an Ongoing or environment card or draw a card."
            List<DestroyCardAction> closedBreaches = new List<DestroyCardAction>();
            IEnumerator closeCoroutine = base.GameController.DestroyCards(base.HeroTurnTakerController, BreachCard(), storedResults: closedBreaches, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(closeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(closeCoroutine);
            }
            int numClosed = closedBreaches.Where((DestroyCardAction dca) => dca.WasCardDestroyed).Count();
            IEnumerator displayCoroutine = base.GameController.SendMessageAction(base.Card.Title + " destroyed " + numClosed.ToString() + " Breach cards!", Priority.High, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(displayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(displayCoroutine);
            }
            for (int i = 0; i < numClosed; i++)
            {
                IEnumerator decisionCoroutine = DestroyOrDrawResponse();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(decisionCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(decisionCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DestroyOrDrawResponse()
        {
            // "... you may either destroy an Ongoing or environment card or draw a card."
            List<Function> options = new List<Function>();
            Function destroy = new Function(base.HeroTurnTakerController, "Destroy an ongoing or environment card", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing || c.IsEnvironment, "ongoing or environment"), false, responsibleCard: base.Card, cardSource: GetCardSource()), FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && (c.IsOngoing || c.IsEnvironment)).Count() > 0);
            options.Add(destroy);
            Function draw = new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => base.GameController.DrawCard(base.HeroTurnTaker, cardSource: GetCardSource()));
            options.Add(draw);
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            yield break;
        }
    }
}
