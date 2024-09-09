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
    public class EmeraldCharacterCardController : HeroCharacterCardController
    {
        public EmeraldCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Ongoing cards in deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => IsOngoing(c), "Ongoing")).Condition = () => !Card.IsFlipped;
            // Show list of Ongoing cards in trash
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Trash, new LinqCardCriteria((Card c) => IsOngoing(c), "Ongoing")).Condition = () => !Card.IsFlipped;
            // Show number of cards in Dreadnought's trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Trash).Condition = () => !Card.IsFlipped;
            NoEffect = false;
            CardsToMove = 1;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            if (NoEffect)
            {
                return new CustomDecisionText("Your trash does not have more than " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + ". Do you want to shuffle your trash into your deck to no effect?", "deciding whether to shuffle their trash into their deck to no effect", "Vote for whether to shuffle " + TurnTaker.Name + "'s trash into their deck to no effect", "shuffle trash into deck to no effect");
            }
            if (CardsToMove == 1)
            {
                return new CustomDecisionText("Do you want to shuffle the bottom card of your trash into your deck?", "deciding whether to shuffle the bottom card of their trash into their deck", "Vote for whether to shuffle the bottom card of " + TurnTaker.Name + "'s trash into their deck", "move bottom card of trash to bottom of deck");
            }
            return new CustomDecisionText("Do you want to shuffle the bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of your trash into your deck?", "deciding whether to shuffle the bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of their trash into their deck", "Vote for whether to shuffle the bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of " + TurnTaker.Name + "'s trash into their deck", "move bottom " + CardsToMove.ToString() + " " + CardsToMove.ToString_CardOrCards() + " of trash to bottom of deck");
        }

        public bool NoEffect { get; set; }
        public int CardsToMove { get; set; }

        public override IEnumerator UsePower(int index = 0)
        {
            switch(index)
            {
                case 0:
                    {
                        // "Discard the top 3 cards of your deck."
                        int numDiscards = GetPowerNumeral(0, 3);
                        List<MoveCardAction> actions = new List<MoveCardAction>();
                        IEnumerator discardCoroutine = GameController.DiscardTopCards(DecisionMaker, TurnTaker.Deck, numDiscards, storedResults: actions, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(discardCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(discardCoroutine);
                        }
                        break;
                    }
                case 1:
                    {
                        // "Play an Ongoing card from your trash."
                        IEnumerator playCoroutine = GameController.SelectAndPlayCard(DecisionMaker, (Card c) => TurnTaker.Trash.Cards.Contains(c) && IsOngoing(c), optional: true, cardSource: GetCardSource(), noValidCardsMessage: "There are no playable Ongoing cards in " + TurnTaker.Name + "'s trash.");
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(playCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(playCoroutine);
                        }
                        int damageAmt = GetPowerNumeral(0, 4);
                        int cardsRequired = GetPowerNumeral(1, 3);
                        // "{Emerald} deals herself 4 irreducible psychic damage unless you shuffle the bottom 3 cards of your trash into your deck."
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
                                IEnumerator shuffleCoroutine = GameController.ShuffleLocation(TurnTaker.Deck, cardSource: GetCardSource());
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
                        // If not enough cards were moved, Emerald deals herself damage
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
                        break;
                    }
                default:
                    break;
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may draw a card now."
            yield return GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption2()
        {
            // "Destroy an environment card."
            yield return GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: CharacterCard, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption3()
        {
            // "One target regains 2 HP."
            yield return GameController.SelectAndGainHP(DecisionMaker, 2, cardSource: GetCardSource());
        }
    }
}
