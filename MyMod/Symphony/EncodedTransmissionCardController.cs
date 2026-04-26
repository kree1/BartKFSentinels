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
    public class EncodedTransmissionCardController : BenefitCardController
    {
        public EncodedTransmissionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 2;
        }

        public override IEnumerator OneShotEffect()
        {
            // "Reveal the top card of a deck, then replace it."
            List<SelectLocationDecision> choices = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = GameController.SelectADeck(DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location l) => l.IsDeck && l.IsRealDeck && l.HasCards, choices, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectLocation(choices))
            {
                Location deck = GetSelectedLocation(choices);
                IEnumerator revealCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(TurnTakerController, deck, false, false, false, new LinqCardCriteria((Card c) => true), 1, 1, shuffleSourceAfterwards: false, revealedCardDisplay: RevealedCardDisplay.ShowRevealedCards);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(revealCoroutine);
                }
            }
        }
    }
}
