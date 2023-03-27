using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class CrucialDefeatCardController : EmpireUtilityCardController
    {
        public CrucialDefeatCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt to Dissenters by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target.DoKeywordsContain(AllyKeyword), 1);
            // "When this card is destroyed, if it has 0 or fewer HP, move it under the Timeline card."
            AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.CardToDestroy.Card == base.Card && base.Card.HitPoints <= 0 && base.GameController.IsCardInPlayAndNotUnderCard(TimelineIdentifier), EraseFromHistoryResponse, TriggerType.MoveCard, TriggerTiming.Before);
            // "This card is immune to damage dealt by non-hero cards."
            AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target == base.Card && (dda.DamageSource == null || !dda.DamageSource.IsHero));
            // "If a villain target was dealt damage this turn, reduce damage dealt to this card by 2."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.Target == base.Card && Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.TargetCard.IsVillainTarget && ddje.Amount > 0).Count() > 0, (DealDamageAction dda) => 2);
        }

        public IEnumerator EraseFromHistoryResponse(GameAction ga)
        {
            if (base.Card.HitPoints > 0)
            {
                yield break;
            }
            // "... move [this card] under the Timeline card."
            IEnumerator cancelCoroutine = base.GameController.CancelAction(ga, showOutput: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            string announcement = "The heroes provided critical aid to their allies in the past, preventing a disastrous outcome!";
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(announcement, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            string effect = "The effects of " + base.Card.Title + " are removed from history!";
            IEnumerator effectCoroutine = base.GameController.SendMessageAction(effect, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(effectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(effectCoroutine);
            }
            Location underTimeline = base.TurnTaker.FindCard(TimelineIdentifier).UnderLocation;
            if (underTimeline == null)
            {
                Log.Debug("Couldn't find underTimeline location");
            }
            else
            {
                Log.Debug("underTimeline located successfully");
            }
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, underTimeline, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield return null;
        }
    }
}
