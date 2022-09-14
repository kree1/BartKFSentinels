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
    public class SignalSuppressionCardController : PalmreaderUtilityCardController
    {
        public SignalSuppressionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(ReduceOncePerTurn), () => base.Card.Title + " has already reduced damage this turn.", () => base.Card.Title + " has not yet reduced damage this turn.", () => true).Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(IncreaseOncePerTurn), () => base.Card.Title + " has already increased damage this turn.", () => base.Card.Title + " has not yet increased damage this turn.", () => true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected const string ReduceOncePerTurn = "ReduceOncePerTurn";
        private ITrigger ReduceDamageTrigger;
        protected const string IncreaseOncePerTurn = "IncreaseOncePerTurn";
        private ITrigger IncreaseDamageTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce the first damage dealt to {PalmreaderCharacter} each turn by 1."
            this.ReduceDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(ReduceOncePerTurn) && dda.Target == base.CharacterCard, ReduceResponse, TriggerType.ReduceDamage, TriggerTiming.Before, isActionOptional: false);
            // "Increase the first damage dealt by {PalmreaderCharacter} each turn by 1."
            this.IncreaseDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(IncreaseOncePerTurn) && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard, IncreaseResponse, TriggerType.IncreaseDamage, TriggerTiming.Before, isActionOptional: false);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, you may play a card named [i]Signal Tap[/i] from your trash."
            IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.HeroTurnTakerController, base.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == "SignalTap" && c.Location == base.TurnTaker.Trash)), optional: true, isPutIntoPlay: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "Then, destroy all but 2 Goalposts cards."
            IEnumerator destroyCoroutine = DestroyExcessRelaysResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        public IEnumerator IncreaseResponse(DealDamageAction dda)
        {
            base.SetCardPropertyToTrueIfRealAction(IncreaseOncePerTurn);
            IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 1, cardSource: GetCardSource());
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

        public IEnumerator ReduceResponse(DealDamageAction dda)
        {
            base.SetCardPropertyToTrueIfRealAction(ReduceOncePerTurn);
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, ReduceDamageTrigger, cardSource: GetCardSource());
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
