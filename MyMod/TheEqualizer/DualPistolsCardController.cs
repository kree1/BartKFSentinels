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
    public class DualPistolsCardController : MunitionCardController
    {
        public DualPistolsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator SalvoAttack()
        {
            // "{TheEqualizer} deals the [b][/i]Marked[/i][/b] target 3 projectile damage."
            IEnumerator shootMarkedCoroutine = DealDamage(CharacterCard, ettc.MarkedTarget(GetCardSource()), 3, DamageType.Projectile, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(shootMarkedCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(shootMarkedCoroutine);
            }
            // "Then, if this card has 6 or more HP, {TheEqualizer} deals the [b][/i]Marked[/i][/b] target 3 projectile damage."
            if (Card.HitPoints.HasValueGreaterThanOrEqualTo(6))
            {
                IEnumerator shootAgainCoroutine = DealDamage(CharacterCard, ettc.MarkedTarget(GetCardSource()), 3, DamageType.Projectile, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(shootAgainCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(shootAgainCoroutine);
                }
            }
        }
    }
}
