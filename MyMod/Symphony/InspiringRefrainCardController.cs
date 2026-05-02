using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class InspiringRefrainCardController : CostCardController
    {
        public InspiringRefrainCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, draw 3 cards and up to 3 targets regain 2 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DrawCardsHealTargetsResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.GainHP });
        }

        public IEnumerator DrawCardsHealTargetsResponse(PhaseChangeAction pca)
        {
            // "... draw 5 cards..."
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, 5);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "... and up to 3 targets regain 2 HP."
            IEnumerator healCoroutine = GameController.SelectAndGainHP(DecisionMaker, 2, numberOfTargets: 3, requiredDecisions: 0, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healCoroutine);
            }
        }
    }
}
