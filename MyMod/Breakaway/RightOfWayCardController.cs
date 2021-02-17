using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    class RightOfWayCardController : CardController
    {
        public RightOfWayCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever damage would be dealt to {Momentum}, redirect it to the target next to this card. If you can't, destroy this card and prevent that damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target == base.TurnTaker.FindCard("Momentum"), this.DealDamageResponse, new TriggerType[] { TriggerType.RedirectDamage, TriggerType.CancelAction, TriggerType.DestroySelf }, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, reveal the environment deck."
            Location environmentDeck = FindLocationsWhere((Location loc) => loc.IsDeck && loc.IsEnvironment && base.GameController.IsLocationVisibleToSource(loc, cardSource: GetCardSource())).First();
            List<RevealCardsAction> revealActions = new List<RevealCardsAction>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, environmentDeck, (Card c) => false, 1, revealActions, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }

            // "Put one of the 2 revealed cards with the highest HP into play and move this card next to it."
            // Get all the maximum HP values of revealed cards, store them in hpValues
            List<int> hpValues = new List<int>();
            List<Card> revealedTargets = GetRevealedCards(revealActions).Where((Card c) => c.IsTarget).ToList();
            foreach (Card c in revealedTargets)
            {
                hpValues.Add((int)c.MaximumHitPoints);
            }

            List<Card> revealedOptions = new List<Card>();
            if (hpValues.Count >= 2)
            {
                // If there are at least 2 revealed cards with HP, then the revealed cards with HP >= the second highest number in hpValues are valid options

                // Sort hpValues descending
                hpValues.Sort();
                hpValues.Reverse();
                // Get the second highest value
                int threshold = hpValues.ElementAt(1);
                // revealedOptions is all revealed cards with max HP >= threshold
                revealedOptions = GetRevealedCards(revealActions).Where((Card c) => c.IsTarget && c.MaximumHitPoints >= threshold).ToList();
            }
            else
            {
                // Otherwise, all revealed cards are valid options

                revealedOptions = GetRevealedCards(revealActions).ToList();
            }

            // Let the players choose which of revealedOptions to put into play, and move this card next to it
            PlayCardAction environmentPlayed = null;
            if (revealedOptions.Any())
            {
                List<PlayCardAction> storedResults = new List<PlayCardAction>();
                IEnumerator environmentPlayCoroutine = base.GameController.SelectAndPlayCard(this.DecisionMaker, revealedOptions, isPutIntoPlay: true, storedResults: storedResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(environmentPlayCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(environmentPlayCoroutine);
                }

                environmentPlayed = storedResults.First();
                if (environmentPlayed != null && environmentPlayed.WasCardPlayed)
                {
                    IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, environmentPlayed.CardToPlay.NextToLocation, cardSource: GetCardSource());
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

            // "Shuffle the other revealed cards into the environment deck."
            List<Card> revealedNotPlayed = GetRevealedCards(revealActions).Where((Card c) => c.Location.IsRevealed).ToList();
            IEnumerator reshuffleCoroutine = CleanupRevealedCards(revealedNotPlayed.FirstOrDefault().Location, environmentDeck, shuffleAfterwards: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reshuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reshuffleCoroutine);
            }

            // "If no targets entered play this way, play the top card of the villain deck."
            if (environmentPlayed == null || !environmentPlayed.WasCardPlayed || !environmentPlayed.CardToPlay.IsTarget)
            {
                IEnumerator playCoroutine = base.GameController.PlayTopCard(this.DecisionMaker, base.TurnTakerController, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }

            yield break;
        }

        private IEnumerator DealDamageResponse(DealDamageAction dda)
        {
            // "Whenever damage would be dealt to {Momentum}, redirect it to the target next to this card. If you can't, destroy this card and prevent that damage."
            // Determine whether damage can be redirected to a card this one is next to.
            if (GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsTarget && GetCardThisCardIsNextTo().IsInPlayAndHasGameText)
            {
                // Redirect damage to the card this card is next to
                IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, GetCardThisCardIsNextTo(), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(redirectCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(redirectCoroutine);
                }
            }
            else
            {
                // Prevent damage and destroy this card
                IEnumerator preventCoroutine = CancelAction(dda);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(preventCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(preventCoroutine);
                }

                IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(this.DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(selfDestructCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(selfDestructCoroutine);
                }
            }

            yield break;
        }
    }
}
