using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class DruvahCardController : CardController
    {
        public DruvahCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.DoKeywordsContain("location") && !c.DoKeywordsContain("myth"), "non-Myth Location"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, reveal the top card of the environment deck. If it's a Location or a Choice, put it into play. If not, discard it."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayOrDiscardResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.DiscardCard });
            // "At the start of the environment turn, if a non-Myth Location is in play, shuffle this card into the environment deck. If not, each player draws a card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, ShuffleOrDrawResponse, new TriggerType[] { TriggerType.ShuffleCardIntoDeck, TriggerType.DrawCard });
        }

        public IEnumerator PlayOrDiscardResponse(GameAction ga)
        {
            // "... reveal the top card of the environment deck. If it's a Location or a Choice, put it into play. If not, discard it."
            IEnumerator revealCoroutine = RevealCard_PlayItOrDiscardIt(base.TurnTakerController, base.TurnTaker.Deck, isPutIntoPlay: true, autoPlayCriteria: new LinqCardCriteria((Card c) => c.DoKeywordsContain(new string[] { "location", "choice" })), showRevealedCards: true, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            yield break;
        }

        public IEnumerator ShuffleOrDrawResponse(GameAction ga)
        {
            // "... if a non-Myth Location is in play, shuffle this card into the environment deck. If not, each player draws a card."
            int realLocations = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("location") && !c.DoKeywordsContain("myth"), "non-Myth Location"), realCardsOnly: true, visibleToCard: GetCardSource()).Count();
            //Log.Debug("realLocations: " + realLocations.ToString());
            if (realLocations > 0)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " doesn't like to stay in one place too long, and moves on...", Priority.Medium, GetCardSource(), associatedCards: base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.DoKeywordsContain("location") && !c.DoKeywordsContain("myth"), "non-Myth Location"), realCardsOnly: true, visibleToCard: GetCardSource()), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.Deck, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
                IEnumerator shuffleCoroutine = ShuffleDeck(DecisionMaker, base.TurnTaker.Deck);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            else
            {
                IEnumerator drawCoroutine = EachPlayerDrawsACard();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
            yield break;
        }
    }
}
