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
    public class HospitalBallCardController : TheGoalieUtilityCardController
    {
        public HospitalBallCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{TheGoalieCharacter} deals 1 target 4 projectile damage."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 4, DamageType.Projectile, new int?(1), false, new int?(1), storedResultsDamage: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If that target is still in play, either destroy a Goalposts card or that target deals {TheGoalieCharacter} 2 irreducible melee damage."
            foreach(DealDamageAction dda in damageResults)
            {
                if (dda.Target.IsInPlayAndHasGameText && dda.Target.IsTarget)
                {
                    List<Function> options = new List<Function>();
                    options.Add(new Function(base.HeroTurnTakerController, "Destroy a Goalposts card", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, GoalpostsCards, false, responsibleCard: base.Card, cardSource: GetCardSource())));
                    options.Add(new Function(base.HeroTurnTakerController, dda.Target.Title + " deals the Goalie 2 irreducible melee damage", SelectionType.DealDamage, () => base.GameController.DealDamage(base.HeroTurnTakerController, dda.Target, (Card c) => c == base.CharacterCard, 2, DamageType.Melee, isIrreducible: true, cardSource: GetCardSource())));
                    SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, false, associatedCards: base.GameController.FindCardsWhere(GoalpostsInPlay), cardSource: GetCardSource());
                    IEnumerator selectCoroutine = base.GameController.SelectAndPerformFunction(choice, associatedCards: base.GameController.FindCardsWhere(GoalpostsInPlay));
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(selectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(selectCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
