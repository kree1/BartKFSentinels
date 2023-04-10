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
    public class RelegatesNaosCardController : CardController
    {
        public RelegatesNaosCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowVillainTargetWithLowestHP(ranking: 1, numberOfTargets: 1);
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c.DoKeywordsContain("lone power"), "the Lone Power", useCardsSuffix: false));
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
        }

        private ITrigger reduceDamageToLowest;
        private ITrigger reduceDamageByLowest;
        private ITrigger reduceDamageToLoneOne;
        private ITrigger reduceDamageByLoneOne;

        private bool? PerformReduceBy { get; set; }
        private bool? PerformReduceTo { get; set; }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to and by the villain target with the lowest HP by 2."
            reduceDamageToLowest = AddTrigger((DealDamageAction dda) => CanCardBeConsideredLowestHitPoints(dda.Target, (Card c) => IsVillainTarget(c)), MaybeReduceDamageTakenResponse, TriggerType.ReduceDamage, TriggerTiming.Before);
            reduceDamageByLowest = AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && CanCardBeConsideredLowestHitPoints(dda.DamageSource.Card, (Card c) => IsVillainTarget(c)), MaybeReduceDamageDealtResponse, TriggerType.ReduceDamage, TriggerTiming.Before);
            // "Reduce damage dealt to and by the [i]Lone Power[/i] by 2."
            reduceDamageToLoneOne = AddReduceDamageTrigger((Card c) => c.DoKeywordsContain("lone power"), 2);
            reduceDamageByLoneOne = AddReduceDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.DoKeywordsContain("lone power"), (DealDamageAction dda) => 2);
            // "At the start of the environment turn, 1 player may discard 2 cards. If they do, put the [i]Lone Power[/i] from the environment trash into play and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardToLeaveResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay, TriggerType.DestroySelf });
        }

        public IEnumerator MaybeReduceDamageDealtResponse(DealDamageAction dda)
        {
            if (base.GameController.PretendMode)
            {
                List<bool> response = new List<bool>();
                IEnumerator checkCoroutine = DetermineIfGivenCardIsTargetWithLowestOrHighestHitPoints(dda.DamageSource.Card, false, (Card c) => IsVillainTarget(c), dda, response);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(checkCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(checkCoroutine);
                }
                PerformReduceBy = response.Count() > 0 && response.First();
            }
            if (PerformReduceBy.HasValue && PerformReduceBy.Value)
            {
                IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 2, reduceDamageByLowest, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(reduceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(reduceCoroutine);
                }
            }
            if (!base.GameController.PretendMode)
            {
                PerformReduceBy = null;
            }
        }

        public IEnumerator MaybeReduceDamageTakenResponse(DealDamageAction dda)
        {
            if (base.GameController.PretendMode)
            {
                List<bool> response = new List<bool>();
                IEnumerator checkCoroutine = DetermineIfGivenCardIsTargetWithLowestOrHighestHitPoints(dda.Target, false, (Card c) => IsVillainTarget(c), dda, response);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(checkCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(checkCoroutine);
                }
                PerformReduceTo = response.Count() > 0 && response.First();
            }
            if (PerformReduceTo.HasValue && PerformReduceTo.Value)
            {
                IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 2, reduceDamageToLowest, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(reduceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(reduceCoroutine);
                }
            }
            if (!base.GameController.PretendMode)
            {
                PerformReduceTo = null;
            }
        }

        public IEnumerator DiscardToLeaveResponse(GameAction ga)
        {
            // "... 1 player may discard 2 cards."
            List<DiscardCardAction> discarded = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCards(DecisionMaker, 2, 2, optionalSelectHero: true, optionalDiscardCard: true, storedResultsDiscard: discarded, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discarded, numberExpected: 2, orMore: true))
            {
                // "If they do, put the [i]Lone Power[/i] from the environment trash into play and destroy this card."
                IEnumerator freeCoroutine = PlayCardsFromLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain("lone power"), "Lone Power"), numberOfCards: 1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(freeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(freeCoroutine);
                }
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, showOutput: true, actionSource: discarded.First(), responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }
    }
}
