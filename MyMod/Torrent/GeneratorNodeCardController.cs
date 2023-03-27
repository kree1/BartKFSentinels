using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class GeneratorNodeCardController : ClusterCardController
    {
        public GeneratorNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"), "Cluster"));
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, reveal the top X cards of a deck, where X = the number of targets destroyed this turn. Play 1 revealed card and discard the rest."
            AddWhenDestroyedTrigger(PlayRevealedCardResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.PlayCard, TriggerType.DiscardCard });
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            if (Journal.GetCardPropertiesBoolean(base.Card, IgnoreEntersPlay) != true)
            {
                // "When this card enters play, reveal the top card of your deck. If it's a Cluster, play it. Otherwise, discard it."
                List<Card> revealed = new List<Card>();
                IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, 1, revealed, revealedCardDisplay: RevealedCardDisplay.Message, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }
                Card topCard = revealed.FirstOrDefault();
                if (topCard != null)
                {
                    if (topCard.DoKeywordsContain("cluster"))
                    {
                        IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, topCard, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(playCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(playCoroutine);
                        }
                    }
                    else
                    {
                        IEnumerator discardCoroutine = base.GameController.MoveCard(base.TurnTakerController, topCard, base.TurnTaker.Trash, showMessage: true, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(discardCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(discardCoroutine);
                        }
                    }
                }
                List<Location> toClean = new List<Location>();
                toClean.Add(base.TurnTaker.Revealed);
                IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, toClean, base.TurnTaker.Deck, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cleanupCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cleanupCoroutine);
                }
            }
        }

        public IEnumerator PlayRevealedCardResponse(DestroyCardAction dca)
        {
            int x = base.NumTargetsDestroyedThisTurn();
            // "... reveal the top X cards of a deck, where X = the number of targets destroyed this turn. Play 1 revealed card and discard the rest."
            List<SelectLocationDecision> deckChoice = new List<SelectLocationDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.RevealCardsFromDeck, (Location l) => true, deckChoice, noValidLocationsMessage: "There are no decks with cards to reveal.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            Location chosenDeck = GetSelectedLocation(deckChoice);
            if (chosenDeck != null)
            {
                List<Card> revealedCards = new List<Card>();
                IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, chosenDeck, x, revealedCards, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }
                if (revealedCards.Count() > 0)
                {
                    if (revealedCards.Count() < x && revealedCards.Count() > 1)
                    {
                        string message = "There were only " + revealedCards.Count().ToString() + " cards to reveal.";
                        IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.High, GetCardSource(), revealedCards);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(messageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(messageCoroutine);
                        }
                    }
                    
                    if (revealedCards.Count() == 1)
                    {
                        Card only = revealedCards.FirstOrDefault();
                        string message = only.Title + " was the only revealed card, so " + base.Card.Title + " plays it!";
                        IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.High, GetCardSource(), revealedCards);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(messageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(messageCoroutine);
                        }

                        // Play it
                        IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, only, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(playCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(playCoroutine);
                        }
                    }
                    else
                    {
                        // Choose 1 to play, discard the rest
                        List<PlayCardAction> playResults = new List<PlayCardAction>();
                        IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.HeroTurnTakerController, revealedCards, storedResults: playResults, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(playCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(playCoroutine);
                        }
                        IEnumerator discardCoroutine = base.GameController.MoveCards(base.TurnTakerController, chosenDeck.OwnerTurnTaker.Revealed.Cards, chosenDeck.OwnerTurnTaker.Trash, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(discardCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(discardCoroutine);
                        }
                    }
                }
                List<Location> toClean = new List<Location>();
                toClean.Add(chosenDeck.OwnerTurnTaker.Revealed);
                IEnumerator cleanupCoroutine = base.GameController.CleanupCardsAtLocations(base.TurnTakerController, toClean, chosenDeck.OwnerTurnTaker.Trash, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cleanupCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cleanupCoroutine);
                }
            }
        }
    }
}
