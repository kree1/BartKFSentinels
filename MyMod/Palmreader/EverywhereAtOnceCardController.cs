using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Palmreader
{
    public class EverywhereAtOnceCardController : PalmreaderUtilityCardController
    {
        public EverywhereAtOnceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Redirect all damage that would be dealt to hero targets to {PalmreaderCharacter}."
            AddRedirectDamageTrigger((DealDamageAction dda) => IsHeroTarget(dda.Target) && dda.Target != base.CharacterCard, () => base.CharacterCard);
            // "Reduce damage dealt to {PalmreaderCharacter} by 1."
            AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);
            // "At the start of your turn, discard 3 cards or destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardOrDestroyResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroySelf });
        }

        public IEnumerator DiscardOrDestroyResponse(PhaseChangeAction pca)
        {
            // "At the start of your turn, discard 3 cards or destroy this card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = SelectAndDiscardCards(base.HeroTurnTakerController, 3, optional: true, storedResults: discards, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if(!DidDiscardCards(discards, 3))
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }
    }
}
