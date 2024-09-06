using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class StressCardController : DreadnoughtUtilityCardController
    {
        public StressCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of cards in Dreadnought's trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Trash);
            NoEffect = false;
            CardsToMove = 1;
        }

        public bool NoEffect { get; set; }
        public int CardsToMove { get; set; }

        public IEnumerator PayStress(int numCards)
        {
            yield return PayStress(numCards, numCards + 1);
        }

        public IEnumerator PayStress(int cardsRequired, int damageAmt)
        {
            // "{Dreadnought} deals herself [damageAmt] irreducible psychic damage unless you put the bottom [cardsRequired] cards on the bottom of your deck."
            List<MoveCardAction> moved = new List<MoveCardAction>();
            // If there are any cards to move:
            if (TurnTaker.Trash.Cards.Any())
            {
                // Player chooses whether to move cards, with preview of what will happen if they don't
                DealDamageAction preview = new DealDamageAction(GetCardSource(), new DamageSource(GameController, CharacterCard), CharacterCard, damageAmt, DamageType.Psychic, isIrreducible: true);
                CardsToMove = cardsRequired;
                NoEffect = TurnTaker.Trash.Cards.Count() < cardsRequired;
                YesNoDecision choice = new YesNoDecision(GameController, DecisionMaker, SelectionType.Custom, gameAction: preview, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = GameController.MakeDecisionAction(choice);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(chooseCoroutine);
                }
                // If they said yes, cards are moved
                if (DidPlayerAnswerYes(choice))
                {
                    IEnumerable<Card> toMove = TurnTaker.Trash.Cards.Take(cardsRequired);
                    IEnumerator moveCoroutine = GameController.MoveCards(TurnTakerController, toMove, TurnTaker.Deck, toBottom: true, responsibleTurnTaker: TurnTaker, storedResultsAction: moved, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
            }
            // If not enough cards were moved, Dreadnought deals herself damage
            IEnumerable<Card> wasMoved = (from MoveCardAction mca in moved where mca.WasCardMoved select mca.CardToMove).Distinct();
            if (wasMoved.Count() < cardsRequired)
            {
                IEnumerator psychicCoroutine = DealDamage(CharacterCard, CharacterCard, damageAmt, DamageType.Psychic, isIrreducible: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(psychicCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(psychicCoroutine);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (NoEffect)
            {
                return new CustomDecisionText("Your trash does not have " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + ". Do you want to move your trash to the bottom of your deck to no effect?", "deciding whether to move their trash to the bottom of their deck to no effect", "Vote for whether to move " + TurnTaker.Name + "'s trash to the bottom of their deck to no effect", "move trash to bottom of deck to no effect");
            }
            if (CardsToMove == 1)
            {
                return new CustomDecisionText("Do you want to move the bottom card of your trash to the bottom of your deck?", "deciding whether to move the bottom card of their trash to the bottom of their deck", "Vote for whether to move the bottom card of " + TurnTaker.Name + "'s trash to the bottom of their deck", "move bottom card of trash to bottom of deck");
            }
            return new CustomDecisionText("Do you want to move the bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of your trash to the bottom of your deck?", "deciding whether to move the bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of their trash to the bottom of their deck", "Vote for whether to move the bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of " + TurnTaker.Name + "'s trash to the bottom of their deck", "move bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of trash to bottom of deck");
        }
    }
}
