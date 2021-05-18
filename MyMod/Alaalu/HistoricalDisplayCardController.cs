using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class HistoricalDisplayCardController : CardController
    {
        public HistoricalDisplayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a target is dealt damage, that target regains 2 HP."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DidDealDamage && dda.Target.IsInPlayAndHasGameText, (DealDamageAction dda) => base.GameController.GainHP(dda.Target, 2, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            // "Players may draw an additional card during their draw phase."
            AddAdditionalPhaseActionTrigger((TurnTaker tt) => ShouldIncreasePhaseActionCount(tt), Phase.DrawCard, 1);
            // "Whenever any player draws three or more cards in a single round, destroy this card and play the top card of the environment deck."
            AddTrigger<DrawCardAction>((DrawCardAction dca) => true, CheckDestructResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard }, TriggerTiming.After);
        }

        public IEnumerator CheckDestructResponse(DrawCardAction dca)
        {
            // Determine whether this is the player's 3rd (or more) draw this round
            IEnumerable<DrawCardJournalEntry> drawsThisRound = (from e in base.Journal.DrawCardEntries() where e.Round == Game.Round && e.Hero == dca.HeroTurnTaker select e);
            if (drawsThisRound.Count() >= 3)
            {
                // If so, destroy this card and play the top card of the environment deck
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, showOutput: true, actionSource: dca, responsibleCard: base.Card, postDestroyAction: PlayEnvironmentCardResponse, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator PlayEnvironmentCardResponse()
        {
            IEnumerator playCoroutine = base.GameController.PlayTopCardOfLocation(base.TurnTakerController, base.TurnTaker.Deck, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }

        public override IEnumerator Play()
        {
            IEnumerator baseCoroutine = base.Play();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(baseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(baseCoroutine);
            }
            IEnumerator increaseCoroutine = IncreasePhaseActionCountIfInPhase((TurnTaker tt) => tt.IsHero, Phase.DrawCard, 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
        }

        private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
        {
            if (tt.IsHero)
            {
                return tt.BattleZone == base.BattleZone;
            }
            return false;
        }

        public override bool AskIfIncreasingCurrentPhaseActionCount()
        {
            if (base.GameController.ActiveTurnPhase.IsDrawCard)
            {
                return ShouldIncreasePhaseActionCount(base.GameController.ActiveTurnTaker);
            }
            return false;
        }
    }
}
