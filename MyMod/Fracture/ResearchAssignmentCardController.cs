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
    public class ResearchAssignmentCardController : FractureUtilityCardController
    {
        public ResearchAssignmentCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Reveal the top card of a deck, then replace it."
            List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.RevealTopCardOfDeck, (Location deck) => true, storedResults, cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }

            if (DidSelectLocation(storedResults))
            {
                Location selectedLocation = GetSelectedLocation(storedResults);
                List<Card> list = new List<Card>();
                //Reveal the top card of 1 deck, then replace it.
                IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, selectedLocation, 1, list, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards, cardSource: base.GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }

                IEnumerator replaceCoroutine = base.CleanupRevealedCards(selectedLocation.OwnerTurnTaker.Revealed, selectedLocation);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(replaceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(replaceCoroutine);
                }
            }
            // "Draw 2 cards."
            IEnumerator drawCoroutine = base.GameController.DrawCards(base.HeroTurnTakerController, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "You may discard 2 cards. If you do, one player may draw a card and play a card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCards(base.HeroTurnTakerController, 2, true, 2, storedResults: discards, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, numberExpected: 2))
            {
                List<SelectTurnTakerDecision> playerChoice = new List<SelectTurnTakerDecision>();
                IEnumerator chooseCoroutine = base.GameController.SelectHeroTurnTaker(base.HeroTurnTakerController, SelectionType.DrawCard, false, false, playerChoice, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (DidSelectTurnTaker(playerChoice))
                {
                    HeroTurnTaker chosenPlayer = playerChoice.FirstOrDefault((SelectTurnTakerDecision dec) => DidSelectTurnTaker(dec.ToEnumerable())).SelectedTurnTaker.ToHero();
                    drawCoroutine = base.GameController.DrawCard(chosenPlayer, optional: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(drawCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(drawCoroutine);
                    }
                    IEnumerator playCoroutine = base.GameController.SelectAndPlayCardFromHand(base.GameController.FindHeroTurnTakerController(chosenPlayer), true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
