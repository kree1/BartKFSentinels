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
            SpecialStringMaker.ShowSpecialString(() => "No damage types are currently [b]monitored.[/b]", showInEffectsList: () => true).Condition = () => base.Card.IsInPlayAndHasGameText && !GetMonitoredType().HasValue;
            SpecialStringMaker.ShowSpecialString(() => GetMonitoredType().ToString() + " damage is currently [b]monitored.[/b]", showInEffectsList: () => true).Condition = () => base.Card.IsInPlayAndHasGameText && GetMonitoredType().HasValue;
            SpecialStringMaker.ShowIfElseSpecialString(() => FindCardsWhere(new LinqCardCriteria((Card c) => c.NextToLocation.Cards.Where((Card x) => x.DoKeywordsContain("antibody")).Count() > 1), visibleToCard: GetCardSource()).Count() > 0, () => "There are " + FindCardsWhere(new LinqCardCriteria((Card c) => c.NextToLocation.Cards.Where((Card x) => x.DoKeywordsContain("antibody")).Count() > 1), visibleToCard: GetCardSource()).Count().ToString() + " targets with 2 or more Antibodies.", () => "There are no targets with 2 or more Antibodies.", () => false);
        }

        private readonly string MonitoredType = "MonitoredType";
        private readonly DamageType[] typeOptions = { DamageType.Cold, DamageType.Energy, DamageType.Fire, DamageType.Infernal, DamageType.Lightning, DamageType.Melee, DamageType.Projectile, DamageType.Psychic, DamageType.Radiant, DamageType.Sonic, DamageType.Toxic };

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "After a target deals 4 or more damage at once, that damage type becomes [b]monitored[/b] and all other damage types are no longer [b]monitored.[/b]"
            // "Whenever a target deals damage of a [b]monitored[/b] type, move the Antibody with the highest HP next to that target."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsTarget && dda.DidDealDamage, MoveMonitorResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.ShowMessage }, TriggerTiming.After);
            // "At the start of the environment turn, if any target has 2 or more Antibodies next to it, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyIfStackedResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator Play()
        {
            SetCardProperty(MonitoredType, -1);
            return base.Play();
        }

        private DamageType? GetMonitoredType()
        {
            int? monitoredIndex = base.GetCardPropertyJournalEntryInteger(MonitoredType);
            if (monitoredIndex.HasValue && monitoredIndex.Value >= 0 && monitoredIndex.Value < typeOptions.Length)
            {
                return typeOptions[monitoredIndex.Value];
            }
            return null;
        }

        public IEnumerator MoveMonitorResponse(DealDamageAction dda)
        {
            // When a target deals damage, two things happen, IN ORDER:
            // 1: if the damage type was already monitored, move an Antibody next to the source
            if (GetMonitoredType().HasValue && dda.DamageType == GetMonitoredType().Value)
            {
                //Log.Debug("AllergicReactionCardController.MoveMonitorResponse: damage type (" + dda.DamageType.ToString() + ") is Monitored");
                //Log.Debug("Finding Antibody to assign...");
                IEnumerable<Card> antibodiesInPlay = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("antibody") && c.IsTarget), visibleToCard: GetCardSource());
                /*Log.Debug("(" + antibodiesInPlay.Count().ToString() + " Antibody cards in play)");
                foreach (Card antibodyTarget in antibodiesInPlay)
                {
                    string locName = antibodyTarget.Location.GetFriendlyName();
                    if (locName.StartsWith("next to"))
                    {
                        Log.Debug("Antibody found (" + antibodyTarget.Title + ") " + antibodyTarget.Location.GetFriendlyName() + ", with " + antibodyTarget.HitPoints.Value.ToString() + " HP");
                    }
                    else
                    {
                        Log.Debug("Antibody found (" + antibodyTarget.Title + ") in " + antibodyTarget.Location.GetFriendlyName() + ", with " + antibodyTarget.HitPoints.Value.ToString() + " HP");
                    }
                }*/
                if (antibodiesInPlay.Count() > 0)
                {
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction(dda.DamageSource.Card.Title + " dealt " + dda.DamageType.ToString() + " damage, which is [b]monitored![/b]{BR}Allergic Reaction moves an Antibody next to the source of the damage...", Priority.Medium, GetCardSource(), showCardSource: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                }
                Card source = dda.DamageSource.Card;
                List<Card> toAssign = new List<Card>();
                IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.DoKeywordsContain("antibody"), toAssign, evenIfCannotDealDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                //Log.Debug("findCoroutine finished");
                //Log.Debug("toAssign: " + toAssign.ToString());
                if (toAssign != null && toAssign.Count() > 0)
                {
                    //Log.Debug("toAssign != null && toAssign.Count() > 0");
                    //Log.Debug("(toAssign.Count(): " + toAssign.Count().ToString() + ")");
                    Card antibodyToMove = toAssign.FirstOrDefault();
                    //Log.Debug("Moving " + antibodyToMove.Title + " (currently at " + antibodyToMove.Location.GetFriendlyName() + ", with " + antibodyToMove.HitPoints.Value.ToString() + " HP)...");
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
            }
            else if (dda.FinalAmount >= 4)
            {
                // 2: if the damage type wasn't already monitored and the damage amount was at least 4, switch the monitored type to the type dealt
                base.SetCardProperty(MonitoredType, typeOptions.IndexOf(dda.DamageType).Value);
                //Log.Debug("AllergicReaction.MoveMonitorResponse: damage amount is " + dda.FinalAmount.ToString() + "(>=4)");
                //Log.Debug(GetMonitoredType().Value.ToString() + " damage is now [b]monitored.[/b]");
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(GetMonitoredType().Value.ToString() + " damage is now [b]monitored.[/b]", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
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
