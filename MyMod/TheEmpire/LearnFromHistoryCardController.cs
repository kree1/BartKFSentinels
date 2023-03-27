using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class LearnFromHistoryCardController : EmpireUtilityCardController
    {
        public LearnFromHistoryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            LinqCardCriteria divergence = new LinqCardCriteria((Card c) => c.DoKeywordsContain(DivergenceKeyword), "Divergence", true, false);
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, divergence);
            SpecialStringMaker.ShowListOfCardsInPlay(divergence);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, reveal cards from the top of the environment deck until a Divergence is revealed. Put that Divergence into play. Shuffle the other revealed cards back into the deck."
            LinqCardCriteria divergence = new LinqCardCriteria((Card c) => c.DoKeywordsContain(DivergenceKeyword), "Divergence", true, false);
            IEnumerator summonCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, false, true, false, divergence, 1, revealedCardDisplay: RevealedCardDisplay.Message);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(summonCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(summonCoroutine);
            }
            // "Then, if there are no Divergences in play, each player draws a card and play the top card of the environment deck."
            if (GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(DivergenceKeyword), "Divergence cards in play", false, false, "Divergence card in play", "Divergence cards in play"), visibleToCard: GetCardSource()).Count() <= 0)
            {
                IEnumerator drawCoroutine = EachPlayerDrawsACard();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
                IEnumerator playCoroutine = base.GameController.PlayTopCardOfLocation(base.TurnTakerController, base.TurnTaker.Deck, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            // "Then, destroy this card."
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
