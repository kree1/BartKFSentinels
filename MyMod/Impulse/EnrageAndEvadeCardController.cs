using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class EnrageAndEvadeCardController : ImpulseUtilityCardController
    {
        public EnrageAndEvadeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "{ImpulseCharacter} deals 1 target 1 sonic damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 1, DamageType.Sonic, 1, false, 1, addStatusEffect: AddCrashEffect, selectTargetsEvenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }

        public IEnumerator AddCrashEffect(DealDamageAction dda)
        {
            // "The next time that target would deal damage, prevent that damage and you may destroy an environment card."
            Card provoked = dda.Target;
            OnDealDamageStatusEffect crashEffect = new OnDealDamageStatusEffect(base.Card, "PreventAndDestroy", "The next time " + provoked.Title + " would deal damage, prevent that damage and " + base.CharacterCard.Title + " may destroy an environment card.", new TriggerType[] { TriggerType.CancelAction, TriggerType.DestroyCard }, base.TurnTaker, base.Card);
            crashEffect.SourceCriteria.IsSpecificCard = provoked;
            crashEffect.DamageAmountCriteria.GreaterThan = 0;
            crashEffect.NumberOfUses = 1;
            crashEffect.UntilCardLeavesPlay(provoked);

            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(crashEffect, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }

        public IEnumerator PreventAndDestroy(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // "... prevent that damage..."
            IEnumerator preventCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            // "... and you may destroy an environment card."
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment && c.IsInPlay, "environment"), true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
