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
    public class SunPointOneCardController : ExpansionWeatherCardController
    {
        public SunPointOneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a phase ends, increase damage dealt this turn by 1."
            AddTrigger((PhaseChangeAction pca) => !pca.IsPretend, IncreaseResponse, TriggerType.CreateStatusEffect, TriggerTiming.Before);
        }

        public IEnumerator IncreaseResponse(GameAction ga)
        {
            if (ga is PhaseChangeAction pca)
            {
                Log.Debug("SunPointOneCardController.IncreaseResponse called with pca: " + pca.ToString());
            }
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(1);
            buff.UntilThisTurnIsOver(base.Game);
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
