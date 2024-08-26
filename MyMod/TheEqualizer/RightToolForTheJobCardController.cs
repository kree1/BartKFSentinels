using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class RightToolForTheJobCardController : EqualizerUtilityCardController
    {
        public RightToolForTheJobCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Cartridge cards in the villain deck
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, CartridgeKeyword), "Cartridge"));
            // Show list of Munition cards in the villain deck
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword), "Munition"));
            // Show whether any [u]salvo[/u] text has been activated this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => AnySalvoActivatedThisTurn(), () => "A [u]salvo[/u] text has already been activated this turn.", () => "No [u]salvo[/u] text has been activated this turn.");
        }

        public bool AnySalvoActivatedThisTurn()
        {
            return GameController.Game.Journal.ActivateAbilityEntries().Where((ActivateAbilityJournalEntry aaje) => aaje.TurnIndex == Game.TurnIndex && aaje.AbilityKey == SalvoName).Any();
        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the villain deck until a Cartridge and a Munition are revealed."
            List<RevealCardsAction> revealed = new List<RevealCardsAction>();
            Card firstCartridge = null;
            Card firstMunition = null;
            List<Card> allMatches = new List<Card>();
            List<string> searchTerms = new List<string>();
            searchTerms.Add(CartridgeKeyword);
            searchTerms.Add(MunitionKeyword);
            IEnumerator revealCoroutine;
            while (searchTerms.Any() && TurnTaker.Deck.HasCards)
            {
                // Reveal cards from the villain deck until you find one that has a keyword in searchTerms (or the deck runs out of cards)
                revealCoroutine = GameController.RevealCards(TurnTakerController, TurnTaker.Deck, (Card c) => c.DoKeywordsContain(searchTerms), 1, revealed, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(revealCoroutine);
                }

                List<Card> newMatches = GetRevealedCards(revealed).Where((Card c) => c.DoKeywordsContain(searchTerms)).Take(1).ToList();
                if (newMatches.Any())
                {
                    // If the card you found has a keyword that's still in searchTerms, add that card to allMatches and remove that keyword from searchTerms
                    Card firstMatch = newMatches.First();
                    if (firstMatch != null)
                    {
                        if (firstMatch.DoKeywordsContain(CartridgeKeyword) && searchTerms.Contains(CartridgeKeyword))
                        {
                            allMatches.Add(firstMatch);
                            firstCartridge = firstMatch;
                            searchTerms.Remove(CartridgeKeyword);
                        }
                        else if (firstMatch.DoKeywordsContain(MunitionKeyword) && searchTerms.Contains(MunitionKeyword))
                        {
                            allMatches.Add(firstMatch);
                            firstMunition = firstMatch;
                            searchTerms.Remove(MunitionKeyword);
                        }
                    }
                }
            }
            // "Put the first revealed Cartridge into play."
            // "Put the first revealed Munition into play."
            List<Card> cardsToPlay = new List<Card>(new Card[] {firstCartridge, firstMunition});
            foreach (Card c in cardsToPlay)
            {
                if (c != null)
                {
                    IEnumerator playCoroutine = GameController.PlayCard(TurnTakerController, c, isPutIntoPlay: true, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
            // "Shuffle the other revealed cards into the villain deck."
            List<Card> otherRevealed = GetRevealedCards(revealed).Where((Card c) => !allMatches.Contains(c)).ToList();
            if (otherRevealed.Any())
            {
                IEnumerator replaceCoroutine = GameController.MoveCards(DecisionMaker, otherRevealed, TurnTaker.Deck, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(replaceCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(replaceCoroutine);
                }

                IEnumerator shuffleCoroutine = ShuffleDeck(DecisionMaker, TurnTaker.Deck);
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            // "If no [u]salvo[/u] text has been activated this turn, {TheEqualizer} deals each non-villain target 1 irreducible projectile damage."
            if (!AnySalvoActivatedThisTurn())
            {
                IEnumerator projectileCoroutine = DealDamage(CharacterCard, (Card c) => c.IsTarget && !IsVillainTarget(c), (Card c) => 1, DamageType.Projectile, isIrreducible: true);
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(projectileCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(projectileCoroutine);
                }
            }
        }
    }
}
