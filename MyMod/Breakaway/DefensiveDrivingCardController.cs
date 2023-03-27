using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace BartKFSentinels.Breakaway
{
    public class DefensiveDrivingCardController : CardController
    {
        public DefensiveDrivingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroCharacterCardWithLowestHP(1);
        }

        public override IEnumerator Play()
        {
            // "{Breakaway} deals the hero character card with the lowest HP 4 irreducible melee damage."
            List<SelectCardDecision> storedResultsHero = new List<SelectCardDecision>();
            List<DealDamageAction> storedResultsDamage = new List<DealDamageAction>();

            // Find the hero character card with the lowest HP; save it to rammedHero...
            LinqCardCriteria criteria = new LinqCardCriteria((Card card) => base.CanCardBeConsideredLowestHitPoints(card, (Card c) => IsHeroCharacterCard(c) && c.IsInPlayAndHasGameText && !c.IsFlipped));
            IEnumerator findCoroutine = base.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.HeroCharacterCard, criteria, storedResultsHero, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card rammedHero = storedResultsHero.FirstOrDefault().SelectedCard;

            // Breakaway deals that hero 4 irreducible melee damage; save the damage action to storedResultsDamage...
            IEnumerator damageCoroutine = base.DealDamage(base.TurnTaker.FindCard("BreakawayCharacter"), rammedHero, 4, DamageType.Melee, isIrreducible: true, storedResults: storedResultsDamage);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // Extract the amount of damage dealt IF it was dealt to rammedHero; save it to damageDealt
            int damageDealt = 0;
            if (storedResultsDamage != null)
            {
                DealDamageAction damageRecorded = storedResultsDamage.FirstOrDefault();
                if (damageRecorded != null)
                {
                    if (damageRecorded.Target == rammedHero)
                    {
                        damageDealt = damageRecorded.Amount;
                    }
                }
            }

            // "If that hero is still active, {Breakaway} loses HP equal to the damage dealt to that hero this way."
            if (rammedHero != null && !rammedHero.IsFlipped)
            {
                Card breakaway = base.TurnTaker.FindCard("BreakawayCharacter");
                IEnumerator loseHPCoroutine = base.GameController.SetHP(breakaway, breakaway.HitPoints.Value - damageDealt, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(loseHPCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(loseHPCoroutine);
                }
            }
        }
    }
}
