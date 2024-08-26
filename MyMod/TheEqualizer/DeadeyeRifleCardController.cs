using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class DeadeyeRifleCardController : MunitionCardController
    {
        public DeadeyeRifleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "This damage can't be redirected by non-villain cards."
            AddTrigger((RedirectDamageAction rda) => rda.DealDamageAction.CardSource != null && rda.DealDamageAction.CardSource.Card == Card && rda.DealDamageAction.OriginalAmount == 5, CancelResponse, TriggerType.CancelAction, TriggerTiming.Before);
        }

        public override IEnumerator SalvoAttack()
        {
            // "{TheEqualizer} deals the [b][i]Marked[/i][/b] target 5 irreducible projectile damage."
            IEnumerator shootMarkedCoroutine = DealDamage(CharacterCard, ettc.MarkedTarget(GetCardSource()), 5, DamageType.Projectile, isIrreducible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(shootMarkedCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(shootMarkedCoroutine);
            }
        }
    }
}
