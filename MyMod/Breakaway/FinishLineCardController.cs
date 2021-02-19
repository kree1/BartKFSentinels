using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace BartKFSentinels.Breakaway
{
    public class FinishLineCardController : CardController
    {
        public FinishLineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // TODO: SpecialStringMaker that identifies the location of The Client
            // TODO: SpecialStringMaker that says if The Client is already at max HP?
        }

        public override IEnumerator Play()
        {
            // "Search the villain deck and trash for {TheClient} and put it into play."
            List<Card> clientsActive = TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndNotUnderCard && c.Identifier == "TheClient").ToList();
            int clientsInPlay = clientsActive.Count();

            if (clientsInPlay > 0)
            {
                // If The Client is in play, we can skip all of this.

                IEnumerator coroutine1 = GameController.SendMessageAction("The Client is already in play.", Priority.High, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(coroutine1);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(coroutine1);
                }
            }
            else
            {
                List<Card> clientsFound = TurnTaker.GetCardsWhere((Card c) => (c.IsInDeck || c.IsInTrash) && c.Identifier == "TheClient").ToList();
                int numClients = clientsFound.Count();
                if (numClients < 1)
                {
                    // If The Client is not in play OR in the deck or trash, display a failure message.
                    IEnumerator coroutine2 = GameController.SendMessageAction("The Client was not found in the villain deck or trash.", Priority.High, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(coroutine2);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(coroutine2);
                    }
                }
                else
                {
                    var locations = new Location[] { base.TurnTaker.Deck, base.TurnTaker.Trash };
                    IEnumerator coroutine3 = base.PlayCardFromLocations(locations, "TheClient", isPutIntoPlay: true, showMessageIfFailed: false, shuffleAfterwardsIfDeck: false);
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(coroutine3);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(coroutine3);
                    }
                }
            }

            // "Shuffle the villain deck."
            IEnumerator coroutine4 = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(coroutine4);
            }
            else
            {
                this.GameController.ExhaustCoroutine(coroutine4);
            }

            clientsActive = TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndNotUnderCard && c.Identifier == "TheClient").ToList();
            clientsInPlay = clientsActive.Count();

            if (clientsInPlay > 0)
            {
                // "If {TheClient} is in play, they regain {H + 1} HP."

                Card activeClient = clientsActive.FirstOrDefault();
                IEnumerator coroutine5 = base.GameController.GainHP(activeClient, H + 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(coroutine5);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(coroutine5);
                }
            }
            else
            {
                // "Otherwise, discard cards from the top of the villain deck until you discard a One-Shot."

                // As long as a One-Shot hasn't been discarded, discard the next card
                // There has to be an easier way (see Shifting Biomes) but I don't know what yet
                bool oneShotFound = false;
                List<MoveCardAction> storedMoves = new List<MoveCardAction>();
                Card discarded = null;
                IEnumerator coroutine6 = base.GameController.DiscardTopCard(base.TurnTaker.Deck, storedMoves, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                while (!oneShotFound)
                {
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(coroutine6);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(coroutine6);
                    }
                    MoveCardAction lastMove = storedMoves.FirstOrDefault();
                    if (lastMove != null)
                    {
                        discarded = lastMove.CardToMove;
                        if (discarded != null)
                        {
                            oneShotFound = base.GameController.DoesCardContainKeyword(discarded, "one-shot");
                        }
                    }
                }

                // "Put that card into play."
                IEnumerator coroutine7 = base.GameController.PlayCard(base.TurnTakerController, discarded, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(coroutine7);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(coroutine7);
                }
            }

            yield break;
        }
    }
}
