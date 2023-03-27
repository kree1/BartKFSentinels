using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class CrowdingTheBoxCardController : TheGoalieUtilityCardController
    {
        public CrowdingTheBoxCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a non-hero target deals damage to {TheGoalieCharacter}, reduce damage dealt by that target to {TheGoalieCharacter} by 1 and increase damage dealt by {TheGoalieCharacter} to that target by 1 until the end of your next turn."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.DamageSource != null && dda.DamageSource.IsTarget && !IsHeroTarget(dda.DamageSource.Card) && dda.DidDealDamage, ApplyStatusesResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        public IEnumerator ApplyStatusesResponse(DealDamageAction dda)
        {
            // "... reduce damage dealt by that target to {TheGoalieCharacter} by 1 and increase damage dealt by {TheGoalieCharacter} to that target by 1 until the end of your next turn."
            ReduceDamageStatusEffect reduction = new ReduceDamageStatusEffect(1);
            reduction.SourceCriteria.IsSpecificCard = dda.DamageSource.Card;
            reduction.TargetCriteria.IsSpecificCard = base.CharacterCard;
            reduction.UntilCardLeavesPlay(dda.DamageSource.Card);
            reduction.UntilCardLeavesPlay(base.CharacterCard);
            reduction.UntilEndOfNextTurn(base.TurnTaker);
            IncreaseDamageStatusEffect increase = new IncreaseDamageStatusEffect(1);
            increase.SourceCriteria.IsSpecificCard = base.CharacterCard;
            increase.TargetCriteria.IsSpecificCard = dda.DamageSource.Card;
            increase.UntilCardLeavesPlay(dda.DamageSource.Card);
            increase.UntilCardLeavesPlay(base.CharacterCard);
            increase.UntilEndOfNextTurn(base.TurnTaker);
            IEnumerator reduceCoroutine = AddStatusEffect(reduction);
            IEnumerator increaseCoroutine = AddStatusEffect(increase);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }
    }
}
