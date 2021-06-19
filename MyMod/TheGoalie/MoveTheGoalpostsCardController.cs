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
    public class MoveTheGoalpostsCardController : TheGoalieUtilityCardController
    {
        public MoveTheGoalpostsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, GoalpostsCards);
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, GoalpostsCards);
        }

        public override IEnumerator Play()
        {
            // "Search your deck and trash for a Goalposts card and put it into play. Shuffle your deck."
            IEnumerator searchCoroutine = base.FetchGoalpostsResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(searchCoroutine);
            }
            // "You may destroy an Ongoing or environment card."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing || c.IsEnvironment, "Ongoing or environment"), true, storedResultsAction: destroyed, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            if (destroyed != null && destroyed.Count() > 0 && DidDestroyCard(destroyed.First()))
            {
                // "If you do, you may play a card or draw a card."
                IEnumerator drawOrPlayCoroutine = DrawACardOrPlayACard(base.HeroTurnTakerController, true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawOrPlayCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawOrPlayCoroutine);
                }
            }
            yield break;
        }
    }
}
