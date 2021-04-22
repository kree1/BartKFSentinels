﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class ReturnToSenderCardController : TheGoalieUtilityCardController
    {
        public ReturnToSenderCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected const string FirstDamage = "DamageResponseOncePerTurn";
        private ITrigger DamageResponseTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time {TheGoalieCharacter} is dealt damage by a target each turn, {TheGoalieCharacter} may deal that target 1 melee damage."
            this.DamageResponseTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamage) && dda.Target == base.CharacterCard && dda.DamageSource.IsTarget && dda.Amount > 0, CounterDamageResponse, TriggerType.DealDamage, TriggerTiming.After, requireActionSuccess: true, isActionOptional: true);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = GetPowerNumeral(0, 2);
            // "Draw a card."
            IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Increase the next damage dealt by {TheGoalieCharacter} by 2."
            IncreaseDamageStatusEffect increaseEffect = new IncreaseDamageStatusEffect(powerNumeral);
            increaseEffect.SourceCriteria.IsSpecificCard = base.CharacterCard;
            increaseEffect.NumberOfUses = 1;
            increaseEffect.UntilCardLeavesPlay(base.CharacterCard);
            IEnumerator increaseCoroutine = base.AddStatusEffect(increaseEffect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }

        public IEnumerator CounterDamageResponse(DealDamageAction dda)
        {
            // "... {TheGoalieCharacter} may deal that target 1 melee damage."
            if (dda.DamageSource.IsCard)
            {
                base.SetCardPropertyToTrueIfRealAction(FirstDamage);
                IEnumerator damageCoroutine = DealDamage(base.CharacterCard, dda.DamageSource.Card, 1, DamageType.Melee, optional: true, isCounterDamage: true, cardSource: GetCardSource());
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