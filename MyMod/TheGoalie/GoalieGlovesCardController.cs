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
    public class GoalieGlovesCardController : TheGoalieUtilityCardController
    {
        public GoalieGlovesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a Goalposts card enters play, increase melee damage dealt by {TheGoalieCharacter} this turn by 2."
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => IsGoalposts(cepa.CardEnteringPlay), IncreaseMeleeResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "{TheGoalieCharacter} deals up to 2 targets 1 melee damage each."
            int numTargets = GetPowerNumeral(0, 2);
            int amtMelee = GetPowerNumeral(1, 1);
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), amtMelee, DamageType.Melee, numTargets, false, 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            yield break;
        }

        public IEnumerator IncreaseMeleeResponse(GameAction ga)
        {
            // "... increase melee damage dealt by {TheGoalieCharacter} this turn by 2."
            IncreaseDamageStatusEffect targetLock = new IncreaseDamageStatusEffect(2);
            targetLock.SourceCriteria.IsSpecificCard = base.CharacterCard;
            targetLock.DamageTypeCriteria.AddType(DamageType.Melee);
            targetLock.UntilThisTurnIsOver(base.Game);
            targetLock.UntilCardLeavesPlay(base.CharacterCard);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(targetLock, true, GetCardSource());
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
    }
}
