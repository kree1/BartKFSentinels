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
            // "Reveal the top card of a deck. Replace or discard it."
            List<SelectLocationDecision> deckChoice = new List<SelectLocationDecision>();
            IEnumerator chooseDeckCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.RevealTopCardOfDeck, (Location l) => l.HasCards, deckChoice, noValidLocationsMessage: "There are no decks with a top card to reveal.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseDeckCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseDeckCoroutine);
            }
            Location chosenDeck = GetSelectedLocation(deckChoice);
            if (chosenDeck != null)
            {
                IEnumerator revealCoroutine = RevealCard_DiscardItOrPutItOnDeck(base.HeroTurnTakerController, base.TurnTakerController, chosenDeck, toBottom: false, fromBottom: false);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
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
                    IEnumerator chooseCardCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.MoveCardOnBottomOfDeck, new LinqCardCriteria(), cardChoice, false, cardSource: GetCardSource());
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
