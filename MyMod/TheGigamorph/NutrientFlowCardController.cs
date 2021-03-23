using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class NutrientFlowCardController : TheGigamorphUtilityCardController
    {
        public NutrientFlowCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "At the end of the environment turn, each target regains 2 HP."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealTargets2Response, TriggerType.GainHP);
            // "At the start of the environment turn, each target regains 1 HP. Then, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealTargets1AndDestroyResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DestroySelf });
            // "When this card is destroyed, play the top card of the environment deck."
            base.AddWhenDestroyedTrigger((DestroyCardAction dca) => base.PlayTheTopCardOfTheEnvironmentDeckResponse(dca), TriggerType.PlayCard);
            base.AddTriggers();
        }

        public IEnumerator HealTargets2Response(PhaseChangeAction pca)
        {
            // "... each target regains 2 HP."
            IEnumerator heal2Coroutine = base.GameController.GainHP(base.DecisionMaker, (Card c) => c.IsTarget && c.IsInPlayAndHasGameText, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(heal2Coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(heal2Coroutine);
            }
            yield break;
        }

        public IEnumerator HealTargets1AndDestroyResponse(PhaseChangeAction pca)
        {
            // "... each target regains 1 HP."
            IEnumerator heal1Coroutine = base.GameController.GainHP(base.DecisionMaker, (Card c) => c.IsTarget && c.IsInPlayAndHasGameText, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(heal1Coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(heal1Coroutine);
            }
            // "Then, destroy this card."
            IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, actionSource: pca, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDestructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDestructCoroutine);
            }
            yield break;
        }
    }
}
