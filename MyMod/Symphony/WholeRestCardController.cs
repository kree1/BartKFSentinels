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
    public class WholeRestCardController : CardController
    {
        public WholeRestCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Destroy any number of your non-character cards."
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsInPlay && c.Owner == TurnTaker && !c.IsCharacter, "belonging to " + TurnTaker.Name, singular: "non-character card", plural: "non-character cards", useCardsPrefix: true, useCardsSuffix: false), null, requiredDecisions: 0, storedResultsAction: destroyResults, allowAutoDecide: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "{Symphony} regains that much HP."
            IEnumerator selfCoroutine = GameController.GainHP(CharacterCard, GetNumberOfCardsDestroyed(destroyResults), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selfCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selfCoroutine);
            }
            // "Discard any number of cards."
            List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCards(DecisionMaker, null, false, 0, discardResults, allowAutoDecide: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "{Symphony} deals itself X irreducible psychic damage..."
            IEnumerator psychicCoroutine = DealDamage(CharacterCard, CharacterCard, GetNumberOfCardsDiscarded(discardResults), DamageType.Psychic, isIrreducible: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(psychicCoroutine);
            }
            //"... and up to X other targets regain 1 HP, where X is the number of cards discarded this way."
            IEnumerator othersCoroutine = GameController.SelectAndGainHP(DecisionMaker, 1, additionalCriteria: (Card c) => c != CharacterCard, numberOfTargets: GetNumberOfCardsDiscarded(discardResults), requiredDecisions: 0, allowAutoDecide: GetNumberOfCardsDiscarded(discardResults) >= FindCardsWhere((Card c) => c.IsTarget && c != CharacterCard && c.IsInPlayAndHasGameText).Count(), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(othersCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(othersCoroutine);
            }
        }
    }
}
