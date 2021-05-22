using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class UmpireCardController : CardController
    {
        public UmpireCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            //AllowFastCoroutinesDuringPretend = false;
            //ShelledOneOncePerTurn = ShelledOneOncePerTurn + base.Card.Identifier;
            //EnvironmentOncePerTurn = EnvironmentOncePerTurn + base.Card.Identifier;
            ReduceDamageToShelledOneTrigger = null;
            ReduceDamageToEnvironmentTrigger = null;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(ShelledOneOncePerTurn), () => base.Card.Title + " has already reduced damage to " + base.CharacterCard.Title + " this turn.", () => base.Card.Title + " has not yet reduced damage to " + base.CharacterCard.Title + " this turn.").Condition = () => base.Card.IsInPlayAndHasGameText && base.CharacterCard.IsTarget;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(EnvironmentOncePerTurn), () => base.Card.Title + " has already reduced damage to an environment target this turn.", () => base.Card.Title + " has not yet reduced damage to an environment target this turn.").Condition = () => base.Card.IsInPlayAndHasGameText && !base.CharacterCard.IsTarget;
        }

        protected string ShelledOneOncePerTurn = "ReduceDamageToShelledOneOncePerTurn";
        private ITrigger ReduceDamageToShelledOneTrigger;
        protected string EnvironmentOncePerTurn = "ReduceDamageToEnvironmentOncePerTurn";
        private ITrigger ReduceDamageToEnvironmentTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time {TheShelledOne} would be dealt damage each turn, reduce that damage by 1."
            //ReduceDamageToShelledOneTrigger = AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(ShelledOneOncePerTurn) && dda.Target == base.CharacterCard && dda.Amount > 0 && !dda.IsPretend, ReduceDamageToShelledOneResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.ReduceDamage }, TriggerTiming.Before);
            ReduceDamageToShelledOneTrigger = AddReduceDamageTrigger((DealDamageAction dda) => !IsPropertyTrue(ShelledOneOncePerTurn) && dda.Amount > 0, ReduceDamageToShelledOneResponse, (Card c) => c == base.CharacterCard, true);
            // "The first time an environment target would be dealt damage each turn, if {TheShelledOne} is not a target, reduce that damage by 1."
            //ReduceDamageToEnvironmentTrigger = AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(EnvironmentOncePerTurn) && dda.Target.IsEnvironmentTarget && dda.Amount > 0 && !dda.IsPretend, ReduceDamageToEnvironmentResponse, new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.ReduceDamage }, TriggerTiming.Before);
            ReduceDamageToEnvironmentTrigger = AddReduceDamageTrigger((DealDamageAction dda) => !IsPropertyTrue(EnvironmentOncePerTurn) && dda.Amount > 0, ReduceDamageToEnvironmentResponse, (Card c) => c.IsEnvironmentTarget, true);
        }

        public IEnumerator ReduceDamageToShelledOneResponse(DealDamageAction dda)
        {
            // "... reduce that damage by 1."
            SetCardPropertyToTrueIfRealAction(ShelledOneOncePerTurn);
            IEnumerator reduceEnvCoroutine = base.GameController.ReduceDamage(dda, 1, ReduceDamageToShelledOneTrigger, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceEnvCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceEnvCoroutine);
            }
            yield break;
        }

        public IEnumerator ReduceDamageToEnvironmentResponse(DealDamageAction dda)
        {
            // "... if {TheShelledOne} is not a target, reduce that damage by 1."
            SetCardPropertyToTrueIfRealAction(EnvironmentOncePerTurn);
            if (!base.CharacterCard.IsTarget)
            {
                IEnumerator reduceShelledCoroutine = base.GameController.ReduceDamage(dda, 1, ReduceDamageToEnvironmentTrigger, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(reduceShelledCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(reduceShelledCoroutine);
                }
            }
            yield break;
        }
    }
}
