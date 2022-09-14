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
    public class SpiritAndFleshCardController : PersistentModCardController
    {
        public SpiritAndFleshCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has already increased damage this turn.", () => base.Card.Title + " has not yet increased damage this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private readonly string OncePerTurn = "IncreaseDamageOncePerTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase the first damage dealt by that target to another target each turn by 2."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == GetCardThisCardIsNextTo() && dda.Target != GetCardThisCardIsNextTo(), IncreaseResponse, TriggerType.IncreaseDamage, TriggerTiming.Before);
        }

        public IEnumerator IncreaseResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }
    }
}
