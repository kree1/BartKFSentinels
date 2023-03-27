using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class HuddleUpCardController : TheGoalieUtilityCardController
    {
        public HuddleUpCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            IEnumerator chooseCoroutine = base.GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(base.GameController, base.HeroTurnTakerController, base.GameController.FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame), SelectionType.RevealCardsFromDeck, numberOfCards: 5, cardSource: GetCardSource()), RevealMovePlayResponse);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            yield break;
        }

        public IEnumerator RevealMovePlayResponse(TurnTaker player)
        {
            // "One player reveals the top 5 cards of their deck..."
            List<Card> revealedCards = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.GameController.FindTurnTakerController(player), player.Deck, 5, revealedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // If not enough cards were available, inform the player
            string countMessage = null;
            switch (revealedCards.Count)
            {
                case 5:
                    break;
                case 0:
                    countMessage = "No cards were revealed!";
                    break;
                case 1:
                    countMessage = "Only one card was revealed! It will automatically be put into " + player.Name + "'s hand.";
                    break;
                case 2:
                    countMessage = "Only two cards were revealed! They will automatically be put into " + player.Name + "'s hand.";
                    break;
                default:
                    countMessage = "Only " + revealedCards.Count.ToString() + " cards were revealed!";
                    break;
            }
            if (countMessage != null)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(countMessage, Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            if (revealedCards.Count > 0)
            {
                // "... puts 2 into their hand..."
                List<MoveCardAction> toHand = new List<MoveCardAction>();
                IEnumerator handCoroutine = base.GameController.SelectCardsFromLocationAndMoveThem(base.GameController.FindHeroTurnTakerController(player.ToHero()), player.Revealed, new int?(2), 2, new LinqCardCriteria(), new MoveCardDestination[] { new MoveCardDestination(player.ToHero().Hand) }, storedResultsMove: toHand, responsibleTurnTaker: player, selectionType: SelectionType.MoveCardToHand, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(handCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(handCoroutine);
                }

                Card inHand = null;
                if (DidMoveCard(toHand))
                {
                    inHand = toHand.FirstOrDefault().CardToMove;
                    revealedCards.Remove(inHand);
                }
                // "... and the rest into their trash."
                int numCardsLeft = revealedCards.Count;
                if (numCardsLeft > 0)
                {
                    Location heroRevealed = player.Revealed;
                    IEnumerator trashCoroutine = base.GameController.SelectCardsFromLocationAndMoveThem(base.GameController.FindHeroTurnTakerController(player.ToHero()), heroRevealed, numCardsLeft, numCardsLeft, new LinqCardCriteria(), new MoveCardDestination[] { new MoveCardDestination(player.Trash) }, responsibleTurnTaker: player, allowAutoDecide: true, selectionType: SelectionType.MoveCardToTrash, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(trashCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(trashCoroutine);
                    }
                }
            }
            // "That player may play a card now."
            List<PlayCardAction> played = new List<PlayCardAction>();
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.GameController.FindHeroTurnTakerController(player.ToHero()), optional: true, storedResults: played);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            if (DidPlayCards(played))
            {
                // "If they do, destroy a Goalposts card."
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, GoalpostsCards, false, responsibleCard: base.Card, cardSource: GetCardSource());
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
}
