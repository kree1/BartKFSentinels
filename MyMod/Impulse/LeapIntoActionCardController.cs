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
    public class LeapIntoActionCardController : ImpulseUtilityCardController
    {
        public LeapIntoActionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "{ImpulseCharacter} deals 1 target 3 melee damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 3, DamageType.Melee, 1, false, 1, addStatusEffect: AddTackleEffect, selectTargetsEvenIfCannotDealDamage: true, cardSource: GetCardSource());
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

        public IEnumerator AddTackleEffect(DealDamageAction dda)
        {
            // "Until the start of your next turn, whenever that target would deal damage, reduce that damage to 1."
            Card pinned = dda.Target;
            OnDealDamageStatusEffect tackleEffect = new OnDealDamageStatusEffect(base.Card, "ReduceToOne", "Until the start of " + base.CharacterCard.Title + "'s next turn, whenever " + pinned.Title + " would deal damage, reduce that damage to 1.", new TriggerType[] { TriggerType.ReduceDamage }, base.TurnTaker, base.Card);
            tackleEffect.SourceCriteria.IsSpecificCard = pinned;
            tackleEffect.UntilStartOfNextTurn(base.TurnTaker);
            tackleEffect.DamageAmountCriteria.GreaterThan = 1;
            tackleEffect.UntilCardLeavesPlay(pinned);

            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(tackleEffect, true, GetCardSource());
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

        public IEnumerator ReduceToOne(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // "... reduce that damage to 1."
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, dda.Amount - 1, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            yield break;
        }
    }
}
