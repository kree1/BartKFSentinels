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
    public class AllergicReactionCardController : TheGigamorphUtilityCardController
    {
        public AllergicReactionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => "No damage types are currently monitored.", showInEffectsList: () => true).Condition = () => base.Card.IsInPlayAndHasGameText && !GetMonitoredType().HasValue;
            SpecialStringMaker.ShowSpecialString(() => GetMonitoredType().ToString() + " damage is currently monitored.", showInEffectsList: () => true).Condition = () => base.Card.IsInPlayAndHasGameText && GetMonitoredType().HasValue;
            SpecialStringMaker.ShowIfElseSpecialString(() => FindCardsWhere(new LinqCardCriteria((Card c) => c.NextToLocation.Cards.Where((Card x) => x.DoKeywordsContain("antibody")).Count() > 1), visibleToCard: GetCardSource()).Count() > 0, () => "There are " + FindCardsWhere(new LinqCardCriteria((Card c) => c.NextToLocation.Cards.Where((Card x) => x.DoKeywordsContain("antibody")).Count() > 1), visibleToCard: GetCardSource()).Count().ToString() + " targets with 2 or more Antibodies.", () => "There are no targets with 2 or more Antibodies.", () => false);
        }

        public override void AddTriggers()
        {
            // "Whenever a target deals damage of a [b]monitored[/b] type, move the Antibody with the highest HP next to that target."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => GetMonitoredType().HasValue && dda.DamageSource.IsTarget && dda.DamageType == GetMonitoredType().Value, AssignAntibodyResponse, TriggerType.MoveCard, TriggerTiming.After);
            // "At the start of the environment turn, if any target has 2 or more Antibodies next to it, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyIfStackedResponse, TriggerType.DestroySelf);
            base.AddTriggers();
        }

        private DamageType? GetMonitoredType()
        {
            // Get the most recent type that was dealt in an amount >= 4 after this card entered play
            DealDamageJournalEntry aggravated = base.GameController.Game.Journal.MostRecentDealDamageEntry((DealDamageJournalEntry dde) => dde.Amount >= 4);
            PlayCardJournalEntry monitoring = base.GameController.Game.Journal.QueryJournalEntries<PlayCardJournalEntry>((PlayCardJournalEntry pce) => pce.CardPlayed == base.Card).LastOrDefault();
            if (monitoring != null)
            {
                int? aggravatedIndex = base.GameController.Game.Journal.GetEntryIndex(aggravated);
                int? monitoringIndex = base.GameController.Game.Journal.GetEntryIndex(monitoring);
                if (aggravatedIndex.HasValue && monitoringIndex.HasValue && aggravatedIndex.Value > monitoringIndex.Value)
                {
                    return aggravated.DamageType;
                }
            }
            return null;
        }

        public IEnumerator AssignAntibodyResponse(DealDamageAction dda)
        {
            // "Whenever a target deals damage of a [b]monitored[/b] type, move the Antibody with the highest HP next to that target."
            Card source = dda.DamageSource.Card;
            MoveCardAction example = new MoveCardAction(GetCardSource(), null, source.NextToLocation, false, 0, null, base.TurnTaker, false, dda, false, true, false, true);
            List<Card> toAssign = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.DoKeywordsContain("antibody"), toAssign, gameAction: example, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (toAssign != null && toAssign.Count() > 0)
            {
                Card antibodyToMove = toAssign.FirstOrDefault();
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, antibodyToMove, source.NextToLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: dda, doesNotEnterPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DestroyIfStackedResponse(PhaseChangeAction pca)
        {
            // "At the start of the environment turn, if any target has 2 or more Antibodies next to it, destroy this card."
            List<Card> multiTaggedTargets = FindCardsWhere(new LinqCardCriteria((Card c) => c.NextToLocation.Cards.Where((Card x) => x.DoKeywordsContain("antibody")).Count() > 1), visibleToCard: GetCardSource()).ToList();
            string message = "There are " + multiTaggedTargets.Count().ToString() + " targets with 2 or more Antibodies next to them, so " + base.Card.Title + " destroys itself!";
            if (multiTaggedTargets.Count() <= 0)
            {
                message = "There are no targets with 2 or more Antibodies next to them, so " + base.Card.Title + " remains in play.";
            }
            else if (multiTaggedTargets.Count() == 1)
            {
                message = "There is 1 target with 2 or more Antibodies next to it, so " + base.Card.Title + " destroys itself!";
            }
            IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), associatedCards: multiTaggedTargets, showCardSource: multiTaggedTargets.Count() <= 0);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(showCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(showCoroutine);
            }
            if (multiTaggedTargets.Count() > 0)
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: false, responsibleCard: base.Card, associatedCards: multiTaggedTargets, cardSource: GetCardSource());
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
    }
}
