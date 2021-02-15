using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    class ExitStrategyCardController : CardController
    {
        private const string TERRAIN = "terrain";
        private const string HAZARD = "hazard";
        private const string ONESHOT = "one-shot";

        public ExitStrategyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((lcc) => lcc.DoKeywordsContain(TERRAIN), "Terrain"));
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((lcc) => lcc.DoKeywordsContain(HAZARD), "Hazard"));
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((lcc) => lcc.DoKeywordsContain(ONESHOT), "One-Shot"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the top of the villain deck until you reveal 1 Terrain, 1 Hazard, and 1 One-Shot."
            List<RevealCardsAction> revealed = new List<RevealCardsAction>();
            Card firstTerrain = null;
            Card firstHazard = null;
            Card firstOneShot = null;
            List<Card> allMatches = new List<Card>();
            List<String> searchKeywords = new List<String>();
            searchKeywords.Add(TERRAIN);
            searchKeywords.Add(HAZARD);
            searchKeywords.Add(ONESHOT);
            IEnumerator revealCoroutine;

            while (searchKeywords.Count() > 0 && base.TurnTaker.Deck.HasCards)
            {
                // Reveal cards from the villain deck until you find one that has a keyword in searchKeywords (or the deck runs out of cards)
                revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, (Card c) => c.DoKeywordsContain(searchKeywords), 1, revealed, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }

                List<Card> newMatches = GetRevealedCards(revealed).Where((Card c) => c.DoKeywordsContain(searchKeywords)).Take(1).ToList();
                if (newMatches.Any())
                {
                    // If the card you found has a keyword that's still in searchKeywords, add that card to allMatches and remove that keyword from searchKeywords
                    Card firstMatch = newMatches.First();
                    if (firstMatch != null)
                    {
                        if (firstMatch.DoKeywordsContain(TERRAIN) && searchKeywords.Contains(TERRAIN))
                        {
                            firstTerrain = firstMatch;
                            allMatches.Add(firstMatch);
                            searchKeywords.Remove(TERRAIN);
                        }
                        else if (firstMatch.DoKeywordsContain(HAZARD) && searchKeywords.Contains(HAZARD))
                        {
                            firstHazard = firstMatch;
                            allMatches.Add(firstMatch);
                            searchKeywords.Remove(HAZARD);
                        }
                        else if (firstMatch.DoKeywordsContain(ONESHOT) && searchKeywords.Contains(ONESHOT))
                        {
                            firstOneShot = firstMatch;
                            allMatches.Add(firstMatch);
                            searchKeywords.Remove(ONESHOT);
                        }
                    }
                }
            }

            // "Shuffle the other revealed cards into the villain deck..."
            List<Card> otherRevealed = GetRevealedCards(revealed).Where((Card c) => !allMatches.Contains(c)).ToList();
            if (otherRevealed.Any())
            {
                IEnumerator replaceCoroutine = base.GameController.MoveCards(this.DecisionMaker, otherRevealed, this.TurnTaker.Deck, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(replaceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(replaceCoroutine);
                }

                IEnumerator shuffleCoroutine = base.ShuffleDeck(this.DecisionMaker, this.TurnTaker.Deck);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }

            // "... then put the first revealed Terrain, the first revealed Hazard, and the first revealed One-Shot into play in that order."
            List<PlayCardAction> cardsPlayed = new List<PlayCardAction>();
            List<Card> cardsToPlay = new List<Card>();
            cardsToPlay.Add(firstTerrain);
            cardsToPlay.Add(firstHazard);
            cardsToPlay.Add(firstOneShot);
            foreach(Card c in cardsToPlay)
            {
                if (c != null)
                {
                    IEnumerator playCoroutine = base.GameController.PlayCard(this.DecisionMaker, c, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, storedResults: cardsPlayed, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }

            List<PlayCardAction> playSuccesses = new List<PlayCardAction>();
            if (cardsPlayed.Count() > 0)
            {
                using (List<PlayCardAction>.Enumerator enumerator = cardsPlayed.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        PlayCardAction pca = enumerator.Current;
                        if (pca.WasCardPlayed)
                        {
                            playSuccesses.Add(pca);
                        }
                    }
                }
            }
            int numEnteredPlay = playSuccesses.Count();

            // "If fewer than 3 cards entered play this way, {Breakaway} regains 3 HP."
            if (numEnteredPlay < 3)
            {
                IEnumerator hpGainCoroutine = base.GameController.GainHP(this.TurnTaker.FindCard("Breakaway"), 3, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(hpGainCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(hpGainCoroutine);
                }
            }

            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            yield break;
        }
    }
}
