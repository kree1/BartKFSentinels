using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class SedativePayloadCardController : CardController
    {
        public SedativePayloadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {Memorial} deals 2 or more damage to a target, reduce the next damage dealt by that target by 2."
            AddTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(base.CharacterCard) && dda.DidDealDamage && dda.FinalAmount >= 2, ReduceNextDamageResponse, TriggerType.AddStatusEffectToDamage, TriggerTiming.After);
        }

        private IEnumerator ReduceNextDamageResponse(DealDamageAction dda)
        {
            // "... reduce the next damage dealt by that target by 2."
            ReduceDamageStatusEffect stun = new ReduceDamageStatusEffect(2);
            stun.SourceCriteria.IsSpecificCard = dda.Target;
            stun.NumberOfUses = 1;
            IEnumerator stunCoroutine = AddStatusEffect(stun);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(stunCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(stunCoroutine);
            }
            yield break;
        }
    }
}
