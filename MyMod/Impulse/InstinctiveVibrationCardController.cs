using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class InstinctiveVibrationCardController : ImpulseUtilityCardController
    {
        public InstinctiveVibrationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever {ImpulseCharacter} is dealt damage, prevent all damage that would be dealt to {ImpulseCharacter} until the end of the current turn."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target == base.CharacterCard && dda.IsSuccessful && dda.FinalAmount > 0, CreatePreventStatus, TriggerType.CreateStatusEffect, TriggerTiming.After, requireActionSuccess: true, isActionOptional: false);
        }

        public IEnumerator CreatePreventStatus(DealDamageAction dda)
        {
            OnDealDamageStatusEffect preventStatus = new OnDealDamageStatusEffect(base.Card, "PreventDamage", "Prevent all damage that would be dealt to " + base.CharacterCard.Title + " this turn.", new TriggerType[] { TriggerType.CancelAction }, base.TurnTaker, base.Card);
            preventStatus.TargetCriteria.IsSpecificCard = base.CharacterCard;
            preventStatus.UntilThisTurnIsOver(base.Game);
            preventStatus.UntilCardLeavesPlay(base.CharacterCard);
            IEnumerator statusCoroutine = AddStatusEffect(preventStatus);
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
