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
            SpecialStringMaker.ShowHasBeenUsedThisTurn(PreventDamageOncePerTurn);
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsGoalposts(c) && c.IsInPlayAndHasGameText && c.Location.IsHero), specifyPlayAreas: true);
            AllowFastCoroutinesDuringPretend = false;
        }

        public const string PreventDamageOncePerTurn = "OncePerTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Once per turn, when a hero target would be dealt exactly 1 damage, you may prevent that damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(PreventDamageOncePerTurn) && dda.Target.IsHero && dda.Amount == 1, OneDamageResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.GainHP }, TriggerTiming.Before, isActionOptional: true);
        }

        public IEnumerator OneDamageResponse(DealDamageAction dda)
        {
            // "... you may prevent that damage."
            bool wouldDealDamage = dda.CanDealDamage;
            IEnumerator preventCoroutine = base.GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            // "When damage is prevented this way, up to X hero targets each regain 1 HP, where X = 2 times the number of Goalposts cards in hero play areas."
            if (wouldDealDamage)
            {
                base.SetCardPropertyToTrueIfRealAction(GoalpostsKeyword);
                IEnumerator healCoroutine = base.GameController.SelectAndGainHP(base.HeroTurnTakerController, 1, optional: true, (Card c) => c.IsHero, numberOfTargets: 2 * NumGoalpostsInHeroPlayAreas(), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
            }
            yield break;
        }
    }
}
