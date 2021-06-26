using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class QueltPeliaenCardController : CardController
    {
        public QueltPeliaenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has reduced damage this turn", () => base.Card.Title + " has not reduced damage this turn").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected const string OncePerTurn = "ReduceFirstDamage";
        private ITrigger ReduceDamageTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce the first damage dealt each turn by 1."
            this.ReduceDamageTrigger = AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Amount > 0, ReduceFirstDamageResponse, TriggerType.ReduceDamage, TriggerTiming.Before);
            // "At the end of the environment turn, each other target regains 1 HP. Then, each non=environment target regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, MassHealResponse, TriggerType.GainHP);
            // "At the start of the environment turn, 1 player may discard a card. If they do, destroy all Myths."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardToDestroyResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroyCard });
        }

        public IEnumerator MassHealResponse(GameAction ga)
        {
            // "... each other target regains 1 HP."
            IEnumerator allHealCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => c != base.Card, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(allHealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(allHealCoroutine);
            }
            // "Then, each non-environment target regains 1 HP."
            IEnumerator weakHealCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => c.IsNonEnvironmentTarget, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(weakHealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(weakHealCoroutine);
            }
            yield break;
        }

        public IEnumerator DiscardToDestroyResponse(GameAction ga)
        {
            // "... 1 player may discard a card. If they do, destroy all Myths."
            List<DiscardCardAction> discarded = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalSelectHero: true, storedResultsDiscard: discarded, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discarded))
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.DoKeywordsContain("myth"), "Myth", false, false, "Myth", "Myths"), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator ReduceFirstDamageResponse(DealDamageAction dda)
        {
            // "Reduce the first damage dealt each turn by 1."
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, ReduceDamageTrigger, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            yield break;
        }
    }
}
