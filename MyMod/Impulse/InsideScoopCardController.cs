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
    public class InsideScoopCardController : ImpulseUtilityCardController
    {
        public InsideScoopCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a card is destroyed, you may reveal the top card of its deck, then replace it."
            base.AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.WasCardDestroyed, CardDestroyedResponse, TriggerType.RevealCard, TriggerTiming.After, isActionOptional: true);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        public IEnumerator CardDestroyedResponse(DestroyCardAction dca)
        {
            // "When a card is destroyed, you may reveal the top card of its deck, then replace it."
            Card destroyed = dca.CardToDestroy.Card;
            Location toCheck = destroyed.NativeDeck;
            Card toReveal = toCheck.TopCard;
            List<YesNoCardDecision> results = new List<YesNoCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.RevealTopCardOfDeck, base.Card, action: dca, storedResults: results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }

            if (base.DidPlayerAnswerYes(results))
            {
                IEnumerator revealCoroutine = base.GameController.SendMessageAction(base.Card.Title + " revealed " + toReveal.Title + " on top of " + toCheck.GetFriendlyName() + "!", Priority.High, cardSource: GetCardSource(), associatedCards: toReveal.ToEnumerable(), showCardSource: true);
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
    }
}
