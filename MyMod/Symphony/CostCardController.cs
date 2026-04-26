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
    public class CostCardController : CardController
    {
        public CostCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your play phase, if you have 8 or more cards in hand, {Symphony} deals itself 3 irreducible psychic damage and you may destroy this card."
            AddTrigger((PhaseChangeAction pca) => pca.FromPhase.TurnTaker == TurnTaker && pca.FromPhase.Phase == Phase.PlayCard && HeroTurnTaker.Hand.Cards.Count() >= 8, DealDamageMaybeDestroyResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf }, TriggerTiming.Before);
        }

        public IEnumerator DealDamageMaybeDestroyResponse(PhaseChangeAction pca)
        {
            // "... {Symphony} deals itself 3 irreducible psychic damage..."
            IEnumerator psychicCoroutine = DealDamage(CharacterCard, CharacterCard, 3, DamageType.Psychic, isIrreducible: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(psychicCoroutine);
            }
            // "... and you may destroy this card."
            IEnumerator destructCoroutine = GameController.DestroyCard(DecisionMaker, Card, optional: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
