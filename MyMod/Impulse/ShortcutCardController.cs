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
    public class ShortcutCardController : ImpulseUtilityCardController
    {
        public ShortcutCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "Draw 2 cards."
            IEnumerator drawCoroutine = base.GameController.DrawCards(base.HeroTurnTakerController, 2, false, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }

            // "Search a deck for a card. Discard that card. Shuffle that deck."
            // Choose a deck
            List<SelectLocationDecision> deckChoice = new List<SelectLocationDecision>();
            IEnumerator chooseDeckCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.SearchDeck, (Location l) => l.HasCards, storedResults: deckChoice, optional: false, noValidLocationsMessage: "There are no cards in decks to search.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseDeckCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseDeckCoroutine);
            }
            Location deck = deckChoice.First().SelectedLocation.Location;
            Location trash = FindTrashFromDeck(deck);
            // Choose a card and discard it, then shuffle
            IEnumerator discardCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, deck, new LinqCardCriteria((Card c) => true), new List<MoveCardDestination> { new MoveCardDestination(trash) }, shuffleAfterwards: true, optional: false, showOutput: true, responsibleTurnTaker: base.TurnTaker, isDiscardIfMovingtoTrash: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            yield break;
        }
    }
}
