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
    public class ThickAndThinCardController : PersistentModCardController
    {
        public ThickAndThinCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has already reduced damage this turn.", () => base.Card.Title + " has not yet reduced damage this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private readonly string OncePerTurn = "ReduceDamageOncePerTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce the first damage dealt by that target to another target each turn by 2."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card == GetCardThisCardIsNextTo() && dda.Target != GetCardThisCardIsNextTo(), ReduceResponse, TriggerType.ReduceDamage, TriggerTiming.Before);
        }

        public IEnumerator ReduceResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 2, null, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            yield break;
        }
    }
}
