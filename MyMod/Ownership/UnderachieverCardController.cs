using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class UnderachieverCardController : TeamModCardController
    {
        public UnderachieverCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever the Map card would add 2 tokens to this play area's Stat card, remove 2 tokens from that Stat card instead."
            AddTrigger((AddTokensToPoolAction tpa) => tpa.NumberOfTokensToAdd == 2 && tpa.TokenPool == RelevantStatCard().FindTokenPool(WeightPoolIdentifier) && tpa.CardSource != null && tpa.CardSource.Card.Identifier == MapCardIdentifier, PreventAndRemoveResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.AddTokensToPool }, TriggerTiming.Before);
        }

        IEnumerator PreventAndRemoveResponse(AddTokensToPoolAction tpa)
        {
            // "... remove 2 tokens from that Stat card instead."
            IEnumerator cancelCoroutine = CancelAction(tpa, isPreventEffect: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator removeCoroutine = base.GameController.RemoveTokensFromPool(tpa.TokenPool, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }
        }
    }
}
