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
    public class FlippersCardController : TeamModCardController
    {
        public FlippersCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero in this play area would be dealt cold damage by a villain target, prevent that damage and that hero deals 1 target 3 projectile damage."
            AddTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && dda.Target.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageSource != null && dda.DamageSource.IsCard && IsVillainTarget(dda.DamageSource.Card) && dda.DamageType == DamageType.Cold, PreventAndCounterResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.CancelAction, TriggerType.DealDamage }, TriggerTiming.Before);
        }

        public IEnumerator PreventAndCounterResponse(DealDamageAction dda)
        {
            // "... prevent that damage ..."
            IEnumerator preventCoroutine = CancelAction(dda, true, true, isPreventEffect: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            // "... and that hero deals 1 target 3 projectile damage."
            if (!dda.IsPretend)
            {
                IEnumerator projectileCoroutine = base.GameController.SelectTargetsAndDealDamage(base.GameController.FindHeroTurnTakerController(dda.Target.Owner.ToHero()), new DamageSource(base.GameController, dda.Target), 3, DamageType.Projectile, 1, false, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(projectileCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(projectileCoroutine);
                }
            }
        }
    }
}
