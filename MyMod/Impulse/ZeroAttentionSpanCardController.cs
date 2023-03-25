using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class ZeroAttentionSpanCardController : ImpulseUtilityCardController
    {
        public ZeroAttentionSpanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may destroy an Ongoing card. If you don't, {ImpulseCharacter} deals himself 1 energy damage."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyOngoingOrTakeDamage, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DealDamage });
            // "At the start of your turn, you may draw a card or play a card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DrawOrPlay, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard });
        }

        public IEnumerator DestroyOngoingOrTakeDamage(PhaseChangeAction pca)
        {
            // "At the end of your turn, you may destroy an Ongoing card. If you don't, {ImpulseCharacter} deals himself 1 energy damage."
            List<DestroyCardAction> destroyActions = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => IsOngoing(c), "Ongoing"), optional: true, storedResultsAction: destroyActions, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }

            DestroyCardAction destroyAttempt = destroyActions.FirstOrDefault();
            bool didDestroy = false;
            if (destroyAttempt != null && destroyAttempt.WasCardDestroyed)
            {
                didDestroy = true;
            }

            if (!didDestroy)
            {
                IEnumerator damageCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, 1, DamageType.Energy, isOptional: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DrawOrPlay(PhaseChangeAction pca)
        {
            // "At the start of your turn, you may draw a card or play a card."
            IEnumerator chooseActionCoroutine = DrawACardOrPlayACard(base.HeroTurnTakerController, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseActionCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseActionCoroutine);
            }
            yield break;
        }
    }
}
