using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class LotsOfBirdsCardController : BlaseballWeatherCardController
    {
        public LotsOfBirdsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroCharacterCardWithHighestHP(ranking: 2);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, the hero with the second highest HP may have the environment deal them {H + 1} projectile damage. If that hero takes damage this way, destroy a villain Ongoing card and the environment deals {TheShelledOne} 4 projectile damage."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageUnshellResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        public IEnumerator DamageUnshellResponse(GameAction ga)
        {
            // "... the hero with the second highest HP may have the environment deal them {H + 1} projectile damage."
            List<Card> secondHighestChoice = new List<Card>();
            DealDamageAction sample = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, FindEnvironment().TurnTaker), null, H + 1, DamageType.Projectile);
            IEnumerator findSecondHighestCoroutine = base.GameController.FindTargetWithHighestHitPoints(2, (Card c) => IsHeroCharacterCard(c), secondHighestChoice, gameAction: sample, dealDamageInfo: sample.ToEnumerable(), evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findSecondHighestCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findSecondHighestCoroutine);
            }
            Card secondHighest = secondHighestChoice.FirstOrDefault();
            if (secondHighest != null)
            {
                List<DealDamageAction> damageResult = new List<DealDamageAction>();
                IEnumerator heroDamageCoroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, FindEnvironment().TurnTaker), secondHighest, H + 1, DamageType.Projectile, optional: true, storedResults: damageResult, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(heroDamageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(heroDamageCoroutine);
                }
                DealDamageAction heroDamage = damageResult.FirstOrDefault();
                if (heroDamage != null && heroDamage.DidDealDamage && heroDamage.Target == secondHighest)
                {
                    Card shell = base.TurnTaker.FindCard("GiantPeanutShell");
                    // "If that hero takes damage this way, destroy a villain Ongoing card..."
                    IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsVillain && IsOngoing(c), "villain Ongoing"), 1, requiredDecisions: 1, responsibleCard: base.Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                    // "... and the environment deals {TheShelledOne} 4 projectile damage."
                    IEnumerator damageCoroutine = null;
                    if (base.CharacterCard.IsTarget)
                    {
                        damageCoroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, FindEnvironment().TurnTaker), base.CharacterCard, 4, DamageType.Projectile, cardSource: GetCardSource());
                    }
                    else
                    {
                        damageCoroutine = base.GameController.SendMessageAction(base.CharacterCard.Title + " is not a target.", Priority.Medium, GetCardSource(), showCardSource: true);
                    }
                    if (damageCoroutine != null)
                    {
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(damageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(damageCoroutine);
                        }
                    }
                }
            }
        }
    }
}
