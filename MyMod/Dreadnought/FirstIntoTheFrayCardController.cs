using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
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
            // "Whenever you skip your play phase or power phase, put the bottom card of your trash on the bottom of your deck. If you can't, {Dreadnought} deals herself 2 irreducible psychic damage."
            AddTrigger((PhaseChangeAction pca) => pca.FromPhase.TurnTaker == TurnTaker && (pca.FromPhase.Phase == Phase.PlayCard || pca.FromPhase.Phase == Phase.UsePower) && pca.FromPhase.WasSkipped, (PhaseChangeAction pca) => PayStress(1), new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.Before);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 2);
            int meleeAmt = GetPowerNumeral(1, 2);
            int numCards = GetPowerNumeral(2, 2);
            // "{Dreadnought} deals up to 2 targets 2 melee damage each."
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), meleeAmt, DamageType.Melee, numTargets, false, 0, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "Discard the top 2 cards of your deck."
            IEnumerator discardCoroutine = GameController.DiscardTopCards(DecisionMaker, TurnTaker.Deck, numCards, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
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
