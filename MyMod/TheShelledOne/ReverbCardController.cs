using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class ReverbCardController : BlaseballWeatherCardController
    {
        public ReverbCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of each player's turn, discard the top card of their deck. If a One-Shot is discarded this way, that player skips their power phase this turn and uses a power immediately."
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame, DiscardReorderResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.UsePower, TriggerType.SkipPhase });
        }

        public IEnumerator DiscardReorderResponse(PhaseChangeAction pca)
        {
            // "... discard the top card of [the active player's] deck."
            TurnTaker currentPlayer = pca.ToPhase.TurnTaker;
            List<MoveCardAction> discardResults = new List<MoveCardAction>();
            IEnumerator discardCoroutine = base.GameController.DiscardTopCard(currentPlayer.Deck, discardResults, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If a One-Shot is discarded this way, that player skips their power phase this turn and uses a power immediately."
            MoveCardAction discard = discardResults.FirstOrDefault();
            if (discard != null && discard.CardToMove != null && discard.WasCardMoved && discard.CardToMove.DoKeywordsContain("one-shot"))
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("Reverberations are at high levels! " + currentPlayer.Name + " had their turn shuffled in the Reverb!", Priority.Medium, GetCardSource(), discard.CardToMove.ToEnumerable(), true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                OnPhaseChangeStatusEffect skipsPower = new OnPhaseChangeStatusEffect(base.Card, nameof(SkipPowerPhaseResponse), currentPlayer.Name + " skips their power phase this turn.", new TriggerType[] { TriggerType.SkipPhase }, base.Card);
                skipsPower.TurnTakerCriteria.IsSpecificTurnTaker = currentPlayer;
                skipsPower.TurnIndexCriteria.EqualTo = base.Game.TurnIndex;
                skipsPower.TurnPhaseCriteria.Phase = Phase.UsePower;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(skipsPower, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
                IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(base.GameController.FindHeroTurnTakerController(currentPlayer.ToHero()), optional: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator SkipPowerPhaseResponse(PhaseChangeAction pca, OnPhaseChangeStatusEffect sourceEffect)
        {
            Log.Debug("ReverbCardController.SkipPowerPhaseResponse activated");
            IEnumerator skipCoroutine = base.GameController.PreventPhaseAction(pca.ToPhase, showMessage: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(skipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(skipCoroutine);
            }
        }
    }
}
