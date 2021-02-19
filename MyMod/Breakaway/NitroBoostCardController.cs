using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace BartKFSentinels.Breakaway
{
    public class NitroBoostCardController : CardController
    {
        public NitroBoostCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroWithMostCards(false);
        }

        public override IEnumerator Play()
        {
            // "{Breakaway} deals the hero with the most non-character cards in play {H} fire damage."
            // "A target dealt damage this way cannot deal damage until the start of the villain turn."
            List<DealDamageAction> storedResultsDamage = new List<DealDamageAction>();

            // Find the player with the most non-character cards in play, save them to storedResultsHero
            List<TurnTaker> storedResultsHero = new List<TurnTaker>();
            IEnumerator findCoroutine = base.GameController.DetermineTurnTakersWithMostOrFewest(true, 1, 1, (TurnTaker tt) => tt.IsHero, (TurnTaker tt) => GameController.FindCardsWhere((Card c) => c.IsInPlay && !c.IsCharacter && c.Owner == tt).Count(), SelectionType.DealDamage, storedResultsHero, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(findCoroutine);
            }

            TurnTaker turnTaker = storedResultsHero.FirstOrDefault();
            if (turnTaker != null)
            {
                // That player chooses one of their hero characters to take damage, save them to resultsHeroCharacter
                List<Card> resultsHeroCharacter = new List<Card>();
                IEnumerator chooseCoroutine = base.FindCharacterCardToTakeDamage(turnTaker, resultsHeroCharacter, Card, base.H, DamageType.Fire);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                // Breakaway deals that hero H fire damage and applies [can't deal damage until the start of the villain turn], saves resulting damage to storedResultsDamage
                IEnumerator damageCoroutine = base.DealDamage(base.TurnTaker.FindCard("Breakaway"), resultsHeroCharacter.First(), base.H, DamageType.Fire, addStatusEffect: base.TargetsDealtDamageCannotDealDamageUntilTheStartOfNextTurnResponse, storedResults: storedResultsDamage);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }

            // "If a hero target was dealt at least {H} damage this way, {Breakaway} regains 2 HP."
            if (storedResultsDamage != null && storedResultsDamage.Count((dda)=>dda.Target.IsHero && dda.Amount >= base.H) > 0)
            {
                IEnumerator gainHPCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("Breakaway"), 2, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(gainHPCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(gainHPCoroutine);
                }
            }

            yield break;
        }
    }
}
