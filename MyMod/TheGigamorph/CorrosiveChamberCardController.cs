using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class CorrosiveChamberCardController : TheGigamorphUtilityCardController
    {
        public CorrosiveChamberCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "At the end of the environment turn, deal each target 2 toxic damage."
            base.AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => true, TargetType.All, 2, DamageType.Toxic);
            // "At the start of the environment turn, destroy each target with 2 or fewer HP. If no targets were destroyed this way, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyTargetsOrSelf, TriggerType.DestroyCard);
            base.AddTriggers();
        }

        public IEnumerator DestroyTargetsOrSelf(PhaseChangeAction pca)
        {
            // "... destroy each target with 2 or fewer HP."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            string message = base.Card.Title + " destroys each target with 2 or fewer HP...";
            LinqCardCriteria vulnerable = new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlay && c.HitPoints.Value <= 2);
            IEnumerable<Card> toDestroy = FindCardsWhere(vulnerable, visibleToCard: GetCardSource());
            if (toDestroy != null && toDestroy.Count() > 0)
            {
                IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), associatedCards: toDestroy, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
                IEnumerator destroyCoroutine = base.GameController.DestroyCards(base.DecisionMaker, vulnerable, storedResults: destroyed, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            // "If no targets were destroyed this way, destroy this card."
            if (destroyed == null || destroyed.Count() <= 0)
            {
                string failMessage = "None of the targets could be digested, so " + base.Card.Title + " destroys itself!";
                if (toDestroy.Count() <= 0)
                {
                    failMessage = "There are no targets that could be digested, so " + base.Card.Title + " destroys itself!";
                }
                IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, overrideOutput: failMessage, showOutput: true, actionSource: pca, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selfDestructCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selfDestructCoroutine);
                }
            }
            yield break;
        }
    }
}
