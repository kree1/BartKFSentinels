using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class TheHyperAcceleratedImpulseCharacterCardController : HeroCharacterCardController
    {
        public TheHyperAcceleratedImpulseCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Reveal and replace the top card of a deck."
            List<SelectLocationDecision> revealChoice = new List<SelectLocationDecision>();
            Location revealedDeck = null;
            Card revealedCard = null;
            IEnumerator chooseRevealCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.RevealTopCardOfDeck, (Location l) => l.HasCards, revealChoice, noValidLocationsMessage: "There are no decks with a top card to reveal.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseRevealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseRevealCoroutine);
            }
            if (DidSelectLocation(revealChoice))
            {
                revealedDeck = GetSelectedLocation(revealChoice);
                if (revealedDeck != null)
                {
                    List<Card> revealed = new List<Card>();
                    IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, revealedDeck, 1, revealed, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(revealCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(revealCoroutine);
                    }
                    if (revealed.Count > 0)
                    {
                        // Save this card's identity for later
                        revealedCard = revealed.FirstOrDefault();
                    }
                    IEnumerator replaceCoroutine = base.CleanupRevealedCards(revealedDeck.OwnerTurnTaker.Revealed, revealedDeck);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(replaceCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(replaceCoroutine);
                    }
                }
            }
            // "Discard or play the top card of a deck."
            // Choose a deck to manipulate.
            List<SelectLocationDecision> manipulateDeckChoice = new List<SelectLocationDecision>();
            IEnumerator chooseManipulateCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.PlayTopCard, (Location l) => l.HasCards, manipulateDeckChoice, noValidLocationsMessage: "There are no decks with a top card to play or discard.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseManipulateCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseManipulateCoroutine);
            }
            if (DidSelectLocation(manipulateDeckChoice))
            {
                Location manipulatingDeck = GetSelectedLocation(manipulateDeckChoice);
                TurnTaker deckOwner = manipulatingDeck.OwnerTurnTaker;
                // If that deck is the same one you revealed from, and if its top card is the same card you revealed, then you can look at that card while deciding to play or discard it.
                // Otherwise, you have to choose blindly- play or discard?
                List<Card> knownCards = new List<Card>();
                string playOptionName = "Play top card";
                string discardOptionName = "Discard top card";
                if (manipulatingDeck == revealedDeck && manipulatingDeck.TopCard == revealedCard)
                {
                    knownCards.Add(revealedCard);
                    playOptionName = "Play it";
                    discardOptionName = "Discard it";
                }
                else
                {
                    playOptionName = "Play the top card of " + deckOwner.Name + "'s deck";
                    discardOptionName = "Discard the top card of " + deckOwner.Name + "'s deck";
                }
                // Play or discard the top card?
                List<Function> options = new List<Function>();
                Function playOption = new Function(base.HeroTurnTakerController, playOptionName, SelectionType.PlayCard, () => base.GameController.PlayTopCard(base.HeroTurnTakerController, base.GameController.FindTurnTakerController(deckOwner), optional: false, numberOfCards: 1, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), onlyDisplayIfTrue: deckOwner.Deck.HasCards || deckOwner.Trash.HasCards);
                options.Add(playOption);
                Function discardOption = new Function(base.HeroTurnTakerController, discardOptionName, SelectionType.DiscardCard, () => base.GameController.DiscardTopCard(manipulatingDeck, null, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), onlyDisplayIfTrue: deckOwner.Deck.HasCards || deckOwner.Trash.HasCards);
                options.Add(discardOption);
                SelectFunctionDecision manipulateChoice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, false, noSelectableFunctionMessage: "There are no cards in " + deckOwner.Name + "'s deck or trash to play or discard.", associatedCards: knownCards, cardSource: GetCardSource());
                IEnumerator manipulateCoroutine = base.GameController.SelectAndPerformFunction(manipulateChoice, associatedCards: knownCards);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(manipulateCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(manipulateCoroutine);
                }
            }
            yield break;
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
            // "Select a target. Increase the next damage dealt by that target by 2."
            IEnumerator increaseCoroutine = base.GameController.SelectTargetAndIncreaseNextDamage(base.HeroTurnTakerController, 2, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "Put a card from a trash on the bottom of its deck."
            // Choose a trash with a card in it
            List<SelectLocationDecision> trashChoice = new List<SelectLocationDecision>();
            IEnumerator chooseTrashCoroutine = base.GameController.SelectATrash(base.HeroTurnTakerController, SelectionType.MoveCardOnBottomOfDeck, (Location l) => l.HasCards, storedResults: trashChoice, noValidLocationsMessage: "There are no trash piles with cards to move.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseTrashCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseTrashCoroutine);
            }
            if (trashChoice != null && trashChoice.Count > 0)
            {
                // Choose a card in the trash
                Location trash = trashChoice.FirstOrDefault().SelectedLocation.Location;
                if (trash != null && trash.HasCards)
                {
                    List<SelectCardDecision> cardChoice = new List<SelectCardDecision>();
                    IEnumerator chooseCardCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.MoveCardOnBottomOfDeck, new LinqCardCriteria((Card c) => c.IsInLocation(trash)), cardChoice, false, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(chooseCardCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(chooseCardCoroutine);
                    }
                    if (cardChoice != null && cardChoice.Count > 0)
                    {
                        // Put that card on the bottom of the deck
                        Location deck = trash.OwnerTurnTaker.Deck;
                        Card toMove = cardChoice.FirstOrDefault().SelectedCard;
                        IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, toMove, deck, toBottom: true, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(moveCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(moveCoroutine);
                        }
                    }
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "One hero may use a power now."
            IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
            yield break;
        }
    }
}
