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
    public class OffsidesCardController : TheGoalieUtilityCardController
    {
        public OffsidesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsInPlay(GoalpostsCards);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int firstAmt = GetPowerNumeral(1, 2);
            int secondAmt = GetPowerNumeral(2, 2);
            // "{TheGoalieCharacter} deals 1 target 2 projectile damage."
            IEnumerator singleCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), firstAmt, DamageType.Projectile, numTargets, false, numTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(singleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(singleCoroutine);
            }
            // "You may destroy a Goalposts card."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, GoalpostsCards, true, storedResultsAction: destroyed, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If you do, {TheGoalieCharacter} deals each non-hero target 2 melee damage."
            if (destroyed.FirstOrDefault() != null && destroyed.FirstOrDefault().WasCardDestroyed)
            {
                IEnumerator massCoroutine = DealDamage(base.CharacterCard, (Card c) => !c.IsHero, secondAmt, DamageType.Melee);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(massCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(massCoroutine);
                }
            }
            yield break;
        }
    }
}
