using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class WithinAndWithoutCardController : FractureUtilityCardController
    {
        public WithinAndWithoutCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Each player may name a keyword other than One-Shot. Each player who does reveals cards from the top of their deck until they reveal a card with the keyword they named, puts that card into play or into their hand, then shuffles the other revealed cards into their deck."
            IEnumerator massChooseCoroutine = base.GameController.SelectTurnTakersAndDoAction(base.HeroTurnTakerController, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame), SelectionType.RevealCardsFromDeck, (TurnTaker tt) => NameSearchResponse(tt), requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(massChooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(massChooseCoroutine);
            }
            yield break;
        }

        public IEnumerator NameSearchResponse(TurnTaker tt)
        {
            // If the player's deck is empty, this does nothing regardless of their choices, so skip everything
            if (tt.Deck.Cards.Any())
            {
                // "[This player] may name a keyword other than One-Shot."
                IOrderedEnumerable<string> keywords = from s in tt.Deck.Cards.SelectMany((Card c) => base.GameController.GetAllKeywords(c)).Distinct().Where((string s) => s.ToLower() != "one-shot") orderby s select s;
                keywords = keywords.Concat("Another keyword - find nothing, shuffle your deck".ToEnumerable()).OrderBy((string s) => s);
                List<SelectWordDecision> choice = new List<SelectWordDecision>();
                IEnumerator selectCoroutine = base.GameController.SelectWord(base.GameController.FindHeroTurnTakerController(tt.ToHero()), keywords, SelectionType.SelectKeyword, choice, true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
                if (DidSelectWord(choice))
                {
                    string chosen = GetSelectedWord(choice);
                    // "[This player] reveals cards from the top of their deck until they reveal a card with the keyword they named, puts that card into play or into their hand, then shuffles the other revealed cards into their deck."
                    List<RevealCardsAction> revealedCards = new List<RevealCardsAction>();
                    IEnumerator revealCoroutine = base.GameController.RevealCards(base.GameController.FindTurnTakerController(tt), tt.Deck, (Card c) => c.DoKeywordsContain(chosen), 1, revealedCards, revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(revealCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(revealCoroutine);
                    }
                    RevealCardsAction revealed = revealedCards.FirstOrDefault();
                    if (revealed != null && revealed.FoundMatchingCards)
                    {
                        Card selectedCard = revealed.MatchingCards.FirstOrDefault();
                        List<MoveCardDestination> choices = new List<MoveCardDestination>();
                        choices.Add(new MoveCardDestination(tt.PlayArea));
                        choices.Add(new MoveCardDestination(tt.ToHero().Hand));
                        if (selectedCard != null)
                        {
                            IEnumerator moveCoroutine = base.GameController.SelectLocationAndMoveCard(base.GameController.FindHeroTurnTakerController(tt.ToHero()), selectedCard, choices, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(moveCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(moveCoroutine);
                            }
                            revealed.MatchingCards.Remove(selectedCard);
                        }
                        IEnumerator replaceCoroutine = base.GameController.MoveCards(base.GameController.FindTurnTakerController(tt), revealed.RevealedCards.Where((Card c) => c != selectedCard), tt.Deck, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(replaceCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(replaceCoroutine);
                        }
                        IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(tt.Deck, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(shuffleCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(shuffleCoroutine);
                        }
                    }
                }
            }
            else
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(tt.Name + "'s deck has no cards for " + base.Card.Title + " to reveal.", Priority.Medium, GetCardSource());
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
    }
}
