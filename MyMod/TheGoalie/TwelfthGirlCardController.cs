using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class TwelfthGirlCardController : TheGoalieUtilityCardController
    {
        public TwelfthGirlCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(PreventDamageOncePerTurn), () => base.Card.Title + " has already prevented damage this turn.", () => base.Card.Title + " has not yet prevented damage this turn.", () => true);
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsGoalposts(c) && c.IsInPlayAndHasGameText && c.Location.IsHero), specifyPlayAreas: true).Condition = () => NumGoalpostsInHeroPlayAreas() > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Goalposts cards in hero play areas.").Condition = () => NumGoalpostsInHeroPlayAreas() <= 0;
            AllowFastCoroutinesDuringPretend = false;
        }

        public const string PreventDamageOncePerTurn = "OncePerTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Once per turn, when a hero target would be dealt exactly 1 damage, you may prevent that damage."
            base.AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(PreventDamageOncePerTurn) && dda.Target.IsHero && dda.Amount == 1, OneDamageResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.WouldBeDealtDamage }, TriggerTiming.Before);
        }

        public IEnumerator OneDamageResponse(DealDamageAction dda)
        {
            Log.Debug("TwelfthGirlCardController.OneDamageResponse activated");
            if (dda.IsPretend)
            {
                IEnumerator preventCoroutine = CancelAction(dda, showOutput: true, cancelFutureRelatedDecisions: true, null, isPreventEffect: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(preventCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(preventCoroutine);
                }
                yield break;
            }
            // "... you may..."
            YesNoDecision yesNo = new YesNoDecision(base.GameController, base.HeroTurnTakerController, SelectionType.PreventDamage, gameAction: dda, associatedCards: base.Card.ToEnumerable(), cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.MakeDecisionAction(yesNo);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (yesNo != null && yesNo.Answer.HasValue && yesNo.Answer.Value)
            {
                // "... prevent that damage."
                base.SetCardPropertyToTrueIfRealAction(PreventDamageOncePerTurn);
                IEnumerator preventCoroutine = CancelAction(dda, isPreventEffect: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(preventCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(preventCoroutine);
                }
                if (!dda.IsSuccessful)
                {
                    // "When damage is prevented this way, up to X hero targets each regain 1 HP, where X = 2 times the number of Goalposts cards in hero play areas."
                    IEnumerator healCoroutine = base.GameController.SelectAndGainHP(base.HeroTurnTakerController, 1, optional: false, (Card c) => c.IsHero, numberOfTargets: 2 * NumGoalpostsInHeroPlayAreas(), requiredDecisions: 0, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(healCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(healCoroutine);
                    }
                }
            }
            yield break;
        }

        public override bool CanOrderAffectOutcome(GameAction action)
        {
            if (action is DealDamageAction)
            {
                return (action as DealDamageAction).Target.IsHero && !HasBeenSetToTrueThisTurn(PreventDamageOncePerTurn);
            }
            else
            {
                return false;
            }
        }
    }
}
