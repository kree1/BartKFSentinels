using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class RemoteActivationCardController : PlanetfallUtilityCardController
    {
        public RemoteActivationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Chips in the villain deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Deck, ChipCriteria());
            // Show number of Chips in the villain trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Trash, ChipCriteria());
            // Show whether Higgs Field Amplifier is in the villain trash
            SpecialStringMaker.ShowIfElseSpecialString(() => TurnTaker.Trash.Cards.Any((Card c) => c.Identifier == AmplifierIdentifier), () => "Higgs Field Amplifier is in the villain trash.", () => "Higgs Field Amplifier is not in the villain trash.");
        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the top of the villain deck until {H - 1} Chips are revealed."
            List<Card> revealedCards = new List<Card>();
            List<RevealCardsAction> reveals = new List<RevealCardsAction>();
            IEnumerator revealCoroutine = GameController.RevealCards(TurnTakerController, TurnTaker.Deck, ChipCriteria().Criteria, H - 1, reveals, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(revealCoroutine);
            }
            if (reveals.Any())
            {
                revealedCards.AddRange(reveals.FirstOrDefault().RevealedCards);
            }
            List<Card> actuallyRevealed = revealedCards.Where((Card c) => c.Location.IsRevealed).ToList();
            // "Discard the revealed cards."
            IEnumerator discardCoroutine = GameController.MoveCards(TurnTakerController, actuallyRevealed, TurnTaker.Trash, isDiscard: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Play the topmost {H - 1} Chips from the villain trash."
            List<Card> chipsToPlay = new List<Card>();
            LinqCardCriteria isChip = ChipCriteria();
            for (int i = TurnTaker.Trash.NumberOfCards - 1; i > 0 && chipsToPlay.Count < H - 1; i --)
            {
                if (isChip.Criteria(TurnTaker.Trash.Cards.ElementAt(i)))
                {
                    chipsToPlay.Add(TurnTaker.Trash.Cards.ElementAt(i));
                }
            }
            IEnumerator playCoroutine = GameController.SendMessageAction("No " + ChipKeyword + " cards were found in the villain trash.", Priority.Medium, GetCardSource());
            if (chipsToPlay.Any())
            {
                playCoroutine = GameController.PlayCards(DecisionMaker, (Card c) => chipsToPlay.Contains(c), false, false, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            }
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
            // "If [i]Higgs Field Amplifier[/i] is in the villain trash, play it."
            IEnumerator playHFACoroutine = PlayCardFromLocation(TurnTaker.Trash, AmplifierIdentifier, isPutIntoPlay: false);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playHFACoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playHFACoroutine);
            }
        }
    }
}
