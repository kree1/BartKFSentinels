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
            // "Each player may name a keyword other than One-Shot. Each player who does reveals cards from the top of their deck until they reveal a card with the keyword they named, puts that card into their hand, then shuffles the other revealed cards into their deck."
            IEnumerator massChooseCoroutine = GameController.SelectTurnTakersAndDoAction(HeroTurnTakerController, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame), SelectionType.RevealCardsFromDeck, (TurnTaker tt) => NameSearchResponse(tt), requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(massChooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(massChooseCoroutine);
            }
            // "One player may play a non-one-shot card."
            IEnumerator playCoroutine = GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && (tt as HeroTurnTaker).Hand.Cards.Any((Card c) => !c.IsOneShot)), SelectionType.PlayCard, cardSource: GetCardSource()), (TurnTaker tt) => GameController.SelectAndPlayCardFromHand(FindTurnTakerController(tt).ToHero(), true, cardCriteria: new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"), cardSource: GetCardSource()));
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        public IEnumerator NameSearchResponse(TurnTaker tt)
        {
            // If the player's deck is empty, this does nothing regardless of their choices, so skip everything
            if (tt.Deck.Cards.Any())
            {
                // "[This player] may name a keyword other than One-Shot."
                IOrderedEnumerable<string> keywords = from s in tt.Deck.Cards.SelectMany((Card c) => GameController.GetAllKeywords(c)).Distinct().Where((string s) => s.ToLower() != "one-shot") orderby s select s;
                keywords = keywords.Concat("Another keyword - find nothing, shuffle your deck".ToEnumerable()).OrderBy((string s) => s);
                List<SelectWordDecision> choice = new List<SelectWordDecision>();
                IEnumerator selectCoroutine = GameController.SelectWord(GameController.FindHeroTurnTakerController(tt.ToHero()), keywords, SelectionType.SelectKeyword, choice, true, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(selectCoroutine);
                }
                if (DidSelectWord(choice))
                {
                    string chosen = GetSelectedWord(choice);
                    // "[This player] reveals cards from the top of their deck until they reveal a card with the keyword they named, puts that card into their hand, then shuffles the other revealed cards into their deck."
                    List<RevealCardsAction> revealedCards = new List<RevealCardsAction>();
                    IEnumerator revealCoroutine = GameController.RevealCards(GameController.FindTurnTakerController(tt), tt.Deck, (Card c) => c.DoKeywordsContain(chosen), 1, revealedCards, revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(revealCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(revealCoroutine);
                    }
                    RevealCardsAction revealed = revealedCards.FirstOrDefault();
                    if (revealed != null && revealed.FoundMatchingCards)
                    {
                        Card selectedCard = revealed.MatchingCards.FirstOrDefault();
                        List<MoveCardDestination> choices = new List<MoveCardDestination>();
                        choices.Add(new MoveCardDestination(tt.ToHero().Hand));
                        if (selectedCard != null)
                        {
                            IEnumerator moveCoroutine = GameController.SelectLocationAndMoveCard(GameController.FindHeroTurnTakerController(tt.ToHero()), selectedCard, choices, isPutIntoPlay: true, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                            if (UseUnityCoroutines)
                            {
                                yield return GameController.StartCoroutine(moveCoroutine);
                            }
                            else
                            {
                                GameController.ExhaustCoroutine(moveCoroutine);
                            }
                            revealed.MatchingCards.Remove(selectedCard);
                        }
                        IEnumerator replaceCoroutine = GameController.MoveCards(GameController.FindTurnTakerController(tt), revealed.RevealedCards.Where((Card c) => c != selectedCard), tt.Deck, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(replaceCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(replaceCoroutine);
                        }
                        IEnumerator shuffleCoroutine = GameController.ShuffleLocation(tt.Deck, cardSource: GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(shuffleCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(shuffleCoroutine);
                        }
                    }
                }
            }
            else
            {
                IEnumerator messageCoroutine = GameController.SendMessageAction(tt.Name + "'s deck has no cards for " + base.Card.Title + " to reveal.", Priority.Medium, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }
    }
}
