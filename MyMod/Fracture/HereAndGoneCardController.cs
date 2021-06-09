using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class HereAndGoneCardController : AttachedBreachCardController
    {
        public HereAndGoneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When the target next to this card deals damage, you may redirect that damage to a target of your choice. Then, destroy this card."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == GetCardThisCardIsNextTo(), RedirectDestroyResponse, new TriggerType[] { TriggerType.RedirectDamage, TriggerType.DestroySelf }, TriggerTiming.Before);
        }

        public override IEnumerator Play()
        {
            // "{FractureCharacter} deals that target 2 melee damage."
            IEnumerator meleeCoroutine = base.GameController.DealDamageToTarget(new DamageSource(base.GameController, base.CharacterCard), GetCardThisCardIsNextTo(), (Card c) => 2, DamageType.Melee, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            yield break;
        }

        public IEnumerator RedirectDestroyResponse(DealDamageAction dda)
        {
            // "... you may redirect that damage to a target of your choice."
            IEnumerator redirectCoroutine = base.GameController.SelectTargetAndRedirectDamage(base.HeroTurnTakerController, (Card c) => c.IsTarget, dda, optional: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            // "Then, destroy this card."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            yield break;
        }
    }
}
