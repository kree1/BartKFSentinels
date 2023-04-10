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
    public class NerveClusterCardController : CardController
    {
        public NerveClusterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(DamageDealtList);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of each turn, this card deals {H} melee damage to each target that dealt {H + 2} or more damage to targets other than itself this turn."
            base.AddEndOfTurnTrigger((TurnTaker tt) => true, SmackBelligerentTargetsResponse, TriggerType.DealDamage);
            // "At the start of the environment turn, each player may discard up to 3 cards. If {H + 2} or more cards are discarded this way, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardToDestroyResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroySelf });
        }

        public string DamageDealtList()
        {
            TargetEqualityComparer comparer = new TargetEqualityComparer();
            IEnumerable<DealDamageJournalEntry> record = (from e in base.Journal.DealDamageEntriesThisTurnSinceCardWasPlayed(base.Card) where e.SourceCard != null && e.SourceCard != e.TargetCard select e);
            List<Card> damageDealers = (from e in record where e.SourceCard != null && e.SourceCard.IsInPlayAndHasGameText select e.SourceCard).Distinct(comparer).ToList();
            if (damageDealers.Count() > 0)
            {
                List<string> totals = new List<string>();
                foreach (Card c in damageDealers)
                {
                    totals.Add(string.Concat(str0: c.Title, str1: " has dealt other targets ", str3: " damage", str2: (from e in record where e.SourceCard != null && TargetEqualityComparer.AreCardsTheSameTarget(c, e.SourceCard) select e.Amount).Sum().ToString()));
                }
                return totals.ToCommaList(useWordAnd: true) + " this turn.";
            }
            else
            {
                return "No targets have dealt damage to other targets this turn since " + base.Card.Title + " entered play.";
            }
        }

        public IEnumerable<Card> SelectedTargets()
        {
            TargetEqualityComparer comparer = new TargetEqualityComparer();
            IEnumerable<DealDamageJournalEntry> record = (from e in base.Journal.DealDamageEntriesThisTurnSinceCardWasPlayed(base.Card) where e.SourceCard != null && e.SourceCard != e.TargetCard select e);
            List<Card> damageDealers = (from e in record where e.SourceCard != null && e.SourceCard.IsInPlayAndHasGameText select e.SourceCard).Distinct(comparer).ToList();
            if (damageDealers.Count() > 0)
            {
                List<Card> belligerentTargets = new List<Card>();
                foreach (Card c in damageDealers)
                {
                    int totalDamageDealt = (from e in record where e.SourceCard != null && TargetEqualityComparer.AreCardsTheSameTarget(e.SourceCard, c) select e.Amount).Sum();
                    if (totalDamageDealt >= base.H + 2)
                    {
                        belligerentTargets.Add(c);
                    }
                }
                return belligerentTargets;
            }
            else
            {
                return new List<Card>();
            }
        }

        public IEnumerator SmackBelligerentTargetsResponse(PhaseChangeAction pca)
        {
            // "... this card deals {H} melee damage to each target that dealt {H + 2} or more damage to targets other than itself this turn."
            List<Card> toHit = SelectedTargets().ToList();
            IEnumerator damageCoroutine = base.DealDamage(base.Card, (Card c) => toHit.Contains(c), base.H, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }

        public IEnumerator DiscardToDestroyResponse(PhaseChangeAction pca)
        {
            // "... each player may discard up to 3 cards. If {H + 2} or more cards are discarded this way, destroy this card."
            if (FindTurnTakersWhere((TurnTaker tt) => IsHero(tt)).Any())
            {
                List<DiscardCardAction> discards = new List<DiscardCardAction>();
                IEnumerator discardCoroutine = base.GameController.EachPlayerDiscardsCards(0, 3, discards, showCounter: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                if (discards.Count() >= base.H + 2)
                {
                    IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(selfDestructCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(selfDestructCoroutine);
                    }
                }
            }
            else
            {
                string message = "There are no players in the " + base.BattleZone.Name + " to discard for " + base.Card.Title + ".";
                IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Low, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
            }
        }
    }
}
