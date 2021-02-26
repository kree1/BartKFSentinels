using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public abstract class ImpulseUtilityCardController : CardController
    {
        public ImpulseUtilityCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public IEnumerator PreventDamage(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            IEnumerator preventCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            yield break;
        }
    }
}
