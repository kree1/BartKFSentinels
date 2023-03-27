using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class LeadByNecessityCardController : FractureUtilityCardController
    {
        public LeadByNecessityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has already reacted to damage this turn.", () => base.Card.Title + " has not yet reacted to damage this turn.");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time any hero target is dealt damage each turn, reduce damage dealt to {FractureCharacter} this turn by 1 and {FractureCharacter} may deal the source of that damage 1 psychic damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Target != null && IsHeroTarget(dda.Target) && dda.DidDealDamage, ReduceRetaliateResponse, new TriggerType[] { TriggerType.CreateStatusEffect, TriggerType.DealDamage }, TriggerTiming.After);
        }

        private const string OncePerTurn = "ReactOncePerTurn";

        public IEnumerator ReduceRetaliateResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(OncePerTurn);
            // "... reduce damage dealt to {FractureCharacter} this turn by 1..."
            ReduceDamageStatusEffect protection = new ReduceDamageStatusEffect(1);
            protection.TargetCriteria.IsSpecificCard = base.CharacterCard;
            protection.UntilThisTurnIsOver(base.Game);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(protection, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            // "... and {FractureCharacter} may deal the source of that damage 1 psychic damage."
            if (dda.DamageSource != null && dda.DamageSource.IsTarget)
            {
                IEnumerator damageCoroutine = DealDamage(base.CharacterCard, dda.DamageSource.Card, 1, DamageType.Psychic, optional: true, isCounterDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }
    }
}
