using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class HostileResponseCardController : TheGigamorphUtilityCardController
    {
        public HostileResponseCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "At the end of the environment turn, search the environment deck and trash for an Antibody card and put it into play. Then, shuffle the environment deck."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, FetchAntibody, TriggerType.PutIntoPlay);
            // "At the start of the environment turn, reveal the top card of the environment deck. If it's an Immune card, replace it and destroy this card. Otherwise, discard it."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, RevealAndCheckResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.MoveCard, TriggerType.DestroySelf, TriggerType.DiscardCard });
            base.AddTriggers();
        }

        public IEnumerator RevealAndCheckResponse(PhaseChangeAction pca)
        {
            // "... reveal the top card of the environment deck."
            List<Card> checking = new List<Card>();
            IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.TurnTaker.Deck, 1, checking, revealedCardDisplay: RevealedCardDisplay.Message, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            if (checking != null && checking.Count() > 0)
            {
                Card revealed = checking.FirstOrDefault();
                if (revealed != null)
                {
                    // "If it's an Immune card, replace it and destroy this card. Otherwise, discard it."
                    if (revealed.DoKeywordsContain("immune"))
                    {
                        string message = revealed.Title + " is an Immune card! It returns to the top of the environment deck, and the " + base.Card.Title + " ends.";
                        IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), associatedCards: revealed.ToEnumerable());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(showCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(showCoroutine);
                        }
                        IEnumerator replaceCoroutine = base.GameController.MoveCard(base.TurnTakerController, revealed, base.TurnTaker.Deck, offset: 0, responsibleTurnTaker: base.TurnTaker, actionSource: pca, doesNotEnterPlay: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(replaceCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(replaceCoroutine);
                        }
                        IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: false, actionSource: pca, responsibleCard: base.Card, associatedCards: revealed.ToEnumerable().ToList(), cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(selfDestructCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(selfDestructCoroutine);
                        }
                    }
                    else
                    {
                        IEnumerator discardCoroutine = base.GameController.MoveCard(base.TurnTakerController, revealed, base.TurnTaker.Trash, showMessage: true, responsibleTurnTaker: base.TurnTaker, actionSource: pca, isDiscard: true, cardSource: GetCardSource());
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
            }
            yield break;
        }
    }
}
