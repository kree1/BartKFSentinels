using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class SumSunCardController : ExpansionWeatherCardController
    {
        public SumSunCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a target is dealt damage, increase damage dealt this phase by 1."
            AddTrigger((DealDamageAction dda) => !dda.IsPretend && dda.DidDealDamage, IncreaseDamageThisPhaseResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        public IEnumerator IncreaseDamageThisPhaseResponse(DealDamageAction dda)
        {
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(1);
            buff.ToTurnPhaseExpiryCriteria.Phase = base.GameController.FindNextTurnPhase(base.Game.ActiveTurnPhase).Phase;
            IEnumerator statusCoroutine = AddStatusEffect(buff);
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
