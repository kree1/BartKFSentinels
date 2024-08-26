using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public abstract class MunitionCardController : EqualizerUtilityCardController
    {
        public MunitionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When another villain Munition enters play, activate this card's [u]salvo[/u] text, then destroy this card."
            AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsVillain && GameController.DoesCardContainKeyword(cepa.CardEnteringPlay, MunitionKeyword), FireAndForgetResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf }, TriggerTiming.After);
        }

        public abstract IEnumerator SalvoAttack();

        public override IEnumerator ActivateAbilityEx(CardDefinition.ActivatableAbilityDefinition definition)
        {
            IEnumerator coroutine = null;
            if (definition.Name == SalvoName)
            {
                coroutine = SalvoAttack();
            }
            if (coroutine != null)
            {
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
                // "This card deals itself 2 projectile damage."
                IEnumerator selfDamageCoroutine = DealDamage(Card, Card, 2, DamageType.Projectile, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(selfDamageCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(selfDamageCoroutine);
                }
            }
        }

        IEnumerator FireAndForgetResponse(CardEntersPlayAction cepa)
        {
            // "... activate this card's [u]salvo[/u] text, ..."
            IEnumerator activateCoroutine = GameController.ActivateAbility(this.GetActivatableAbilities(SalvoName).First(), GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(activateCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(activateCoroutine);
            }
            // "... then destroy this card."
            IEnumerator destructCoroutine = GameController.DestroyCard(DecisionMaker, Card, responsibleCard: Card, associatedCards: cepa.CardEnteringPlay.ToEnumerable().ToList(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
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
