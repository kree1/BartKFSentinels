using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class NanoFabricationCardController : CardController
    {
        public NanoFabricationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of non-one-shot cards in Symphony's deck
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"));
            // Show list of non-one-shot cards in Symphony's trash
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Trash, new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"));
        }

        public override IEnumerator Play()
        {
            // "Another player may discard a card."
            List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectHeroToDiscardCard(DecisionMaker, additionalHeroCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != TurnTaker), storedResultsDiscard: discardResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discardResults))
            {
                // "If they do, they may play a non-one-shot card."
                DiscardCardAction success = discardResults.FirstOrDefault((DiscardCardAction dca) => DidDiscardCards(dca.ToEnumerable()));
                TurnTaker tt = success.ResponsibleTurnTaker;
                IEnumerator playCoroutine = GameController.SelectAndPlayCardFromHand(FindTurnTakerController(tt).ToHero(), true, cardCriteria: new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"), cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            else
            {
                // "If not, search your deck and trash for a non-one-shot card and put it into your hand or into play. If you searched your deck, shuffle it."
                IEnumerator searchCoroutine = SearchForCardsEx(TurnTakerController, true, true, 1, 1, new LinqCardCriteria((Card c) => !c.IsOneShot, "non-one-shot"), true, true, false);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(searchCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(searchCoroutine);
                }
            }
        }
    }
}
