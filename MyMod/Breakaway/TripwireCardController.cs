﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    public class TripwireCardController : CardController
    {
        public TripwireCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card is destroyed, each hero target deals itself 2 melee damage. Reduce damage dealt by targets dealt damage this way by 1 until the start of the villain turn."
            base.AddWhenDestroyedTrigger(EveryoneTripsResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.CreateStatusEffect });

            // "When {Breakaway} loses HP or is dealt damage, destroy this card."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DidDealDamage && dda.Target == base.TurnTaker.FindCard("BreakawayCharacter"), base.DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
            // I THINK this line only activates if Breakaway's HP is set to lower than it was?
            // TODO: check if this explodes if he gains HP
            base.AddTrigger<SetHPAction>((SetHPAction sha) => sha.HpGainer == base.TurnTaker.FindCard("BreakawayCharacter") && sha.AmountActuallyChanged < 0 && sha.IsSuccessful, base.DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        private IEnumerator EveryoneTripsResponse(DestroyCardAction dca)
        {
            // "... each hero target deals itself 2 melee damage. Reduce damage dealt by targets dealt damage this way by 1 until the start of the villain turn."
            IEnumerator damageCoroutine = base.GameController.DealDamageToSelf(this.DecisionMaker, (Card c) => IsHeroTarget(c), 2, DamageType.Melee, addStatusEffect: ReduceDamageResponse, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }

        private IEnumerator ReduceDamageResponse(DealDamageAction dd)
        {
            // "Reduce damage dealt by targets dealt damage this way by 1 until the start of the villain turn."
            if (dd.DidDealDamage)
            {
                ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(1);
                reduceDamageStatusEffect.SourceCriteria.IsSpecificCard = dd.Target;
                reduceDamageStatusEffect.UntilStartOfNextTurn(base.TurnTaker);
                IEnumerator statusCoroutine = base.AddStatusEffect(reduceDamageStatusEffect);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
        }
    }
}
