using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class EvacuationProcedureCardController : EvidenceStorageUtilityCardController
    {
        public EvacuationProcedureCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowVillainTargetWithHighestHP(1, 1);
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.DoKeywordsContain("storage"), "storage"));
        }

        public override void AddTriggers()
        {
            // "At the end of the villain turn, the villain target with the highest HP deals each Officer 3 sonic damage. Then, that target deals the {H - 1} Storage cards with the highest HP 1 sonic damage each. Then, destroy this card."
            base.AddEndOfTurnTrigger((TurnTaker tt) => IsVillain(tt), AlarmResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
            // "When this card is destroyed, shuffle all Storage cards from the environment trash into the environment deck."
            base.AddWhenDestroyedTrigger(ShuffleCrateResponse, TriggerType.MoveCard);
            base.AddTriggers();
        }

        public IEnumerator AlarmResponse(PhaseChangeAction pca)
        {
            // If there's at least one Officer or Storage target in play...
            bool anyOfficers = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("officer") && c.IsTarget), visibleToCard: GetCardSource()).Count() > 0;
            bool anyStorage = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("storage") && c.IsTarget), visibleToCard: GetCardSource()).Count() > 0;
            if (anyOfficers || anyStorage)
            {
                // Find the villain target with the highest HP to deal damage
                List<Card> maxVillains = new List<Card>();
                DealDamageAction sonicPreview = new DealDamageAction(base.GameController, null, null, 3, DamageType.Sonic);
                if (!anyOfficers)
                {
                    // No Officers to attack; preview damage to Storage instead
                    Card target = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("storage") && c.IsTarget), visibleToCard: GetCardSource()).FirstOrDefault();
                    if (target != null)
                    {
                        sonicPreview = new DealDamageAction(base.GameController, null, target, 1, DamageType.Sonic, wasOptional: false);
                    }
                }
                IEnumerator findVillainCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsVillainTarget(c), maxVillains, dealDamageInfo: sonicPreview.ToEnumerable(), evenIfCannotDealDamage: true, optional: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findVillainCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findVillainCoroutine);
                }
                Card maxVillainTarget = maxVillains.FirstOrDefault();
                if (maxVillainTarget != null)
                {
                    // "... the villain target with the highest HP deals each Officer 3 sonic damage."
                    IEnumerator officerDamageCoroutine = base.GameController.DealDamage(base.DecisionMaker, maxVillainTarget, (Card c) => c.DoKeywordsContain("officer"), 3, DamageType.Sonic, optional: false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(officerDamageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(officerDamageCoroutine);
                    }
                    // "Then, that target deals the {H - 1} Storage cards with the highest HP 1 sonic damage each."
                    IEnumerator crateDamageCoroutine = DealDamageToHighestHP(maxVillainTarget, 1, (Card c) => c.DoKeywordsContain("storage"), (Card c) => 1, DamageType.Sonic, numberOfTargets: () => Game.H - 1);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(crateDamageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(crateDamageCoroutine);
                    }
                }
            }
            // "Then, destroy this card."
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, actionSource: pca, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        public IEnumerator ShuffleCrateResponse(DestroyCardAction dca)
        {
            // "... shuffle all Storage cards from the environment trash into the environment deck."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleCardsIntoLocation(base.DecisionMaker, FindCardsWhere(new LinqCardCriteria((Card c) => c.DoKeywordsContain("storage") && base.TurnTaker.Trash.HasCard(c))), base.TurnTaker.Deck, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            yield break;
        }
    }
}
