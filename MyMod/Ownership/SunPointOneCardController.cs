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
            // "Whenever a power is used, a non-villain card is played, or a card is drawn, increase damage dealt this turn by 1."
            AddTrigger((UsePowerAction upa) => true, IncreaseResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
            AddTrigger((CardEntersPlayAction cepa) => !IsVillain(cepa.CardEnteringPlay) && !cepa.IsPutIntoPlay, IncreaseResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
            AddTrigger((DrawCardAction dca) => true, IncreaseResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        public IEnumerator IncreaseResponse(GameAction ga)
        {
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
