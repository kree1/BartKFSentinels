using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class TributeToTheHallCardController : StrikeCardController
    {
        public TributeToTheHallCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, each player reveals the top card of their deck. They may discard it or remove it from the game. If {H} cards are removed from the game this way, this card regains 1 HP. Then, if this card has 2 or more HP, put a token on {TheShelledOne} and set this card's HP to 0."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardRemoveResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.DiscardCard, TriggerType.RemoveFromGame, TriggerType.GainHP, TriggerType.AddTokensToPool });
        }

        public IEnumerator DiscardRemoveResponse(GameAction ga)
        {
            // "... each player reveals the top card of their deck. They may discard it or remove it from the game."
            List<MoveCardAction> removed = new List<MoveCardAction>();
            List<Card> showCards = new List<Card>();
            showCards.Add(base.Card);
            IEnumerator teamChoiceCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame), SelectionType.RevealTopCardOfDeck, (TurnTaker tt) => DiscardOrRemoveChoice(tt, removed), allowAutoDecide: true, associatedCards: showCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(teamChoiceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(teamChoiceCoroutine);
            }
            // "If {H} cards are removed from the game this way, this card regains 1 HP."
            if (removed.Where((MoveCardAction mca) => mca.WasCardMoved).Count() >= H)
            {
                IEnumerator healCoroutine = base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
            }
            // "Then, if this card has 2 or more HP, put a token on {TheShelledOne} and set this card's HP to 0."
            if (base.Card.HitPoints.HasValue && base.Card.HitPoints.Value >= 2)
            {
                IEnumerator incrementCoroutine = base.AddTokenAndResetResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(incrementCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(incrementCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DiscardOrRemoveChoice(TurnTaker tt, List<MoveCardAction> moveResults)
        {
            // "... reveals the top card of their deck. They may discard it or remove it from the game."
            HeroTurnTakerController player = base.GameController.FindHeroTurnTakerController(tt.ToHero());
            List<Card> revealed = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(player, tt.Deck, 1, revealed, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            Card revealedCard = GetRevealedCard(revealed);
            if (revealedCard != null)
            {
                List<Function> options = new List<Function>();
                options.Add(new Function(player, "Discard it", SelectionType.DiscardCard, () => base.GameController.DiscardCard(player, revealedCard, null, responsibleTurnTaker: tt, cardSource: GetCardSource())));
                options.Add(new Function(player, "Remove it from the game", SelectionType.RemoveCardFromGame, () => base.GameController.MoveCard(player, revealedCard, tt.OutOfGame, showMessage: true, responsibleTurnTaker: tt, storedResults: moveResults, cardSource: GetCardSource())));
                List<Card> relevantCards = new List<Card>();
                relevantCards.Add(revealedCard);
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, player, options, false, associatedCards: relevantCards, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice, null, relevantCards);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
            }
            IEnumerator cleanCoroutine = CleanupRevealedCards(tt.Revealed, tt.Deck);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cleanCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cleanCoroutine);
            }
            yield break;
        }
    }
}
