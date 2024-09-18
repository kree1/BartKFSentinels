using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Victory
{
    public class FirstIntoTheFrayCardController : StressCardController
    {
        public FirstIntoTheFrayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your power phase, if {Victory} dealt no damage this turn, {Victory} deals herself 2 irreducible psychic damage unless you put the bottom card of your trash on the bottom of your deck."
            AddTrigger((PhaseChangeAction pca) => pca.FromPhase.TurnTaker == TurnTaker && pca.FromPhase.Phase == Phase.UsePower && !Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.SourceCard == CharacterCard && ddje.Amount > 0).Any(), (PhaseChangeAction pca) => PayStress(1), new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.Before);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 2);
            int meleeAmt = GetPowerNumeral(1, 2);
            // "{Victory} deals up to 2 targets 2 melee damage each."
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), meleeAmt, DamageType.Melee, numTargets, false, 0, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "Discard the top card of your deck."
            IEnumerator discardCoroutine = GameController.DiscardTopCard(TurnTaker.Deck, null, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
        }
    }
}
