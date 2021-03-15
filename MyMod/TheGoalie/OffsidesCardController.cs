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
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsGoalposts(c) && c.IsInPlayAndHasGameText), specifyPlayAreas: true);
        }

        public override IEnumerator Play()
        {
            // "{TheGoalieCharacter} deals 1 target 2 melee damage."
            List<SelectCardDecision> firstTargeting = new List<SelectCardDecision>();
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Melee, new int?(1), false, new int?(1), storedResultsDecisions: firstTargeting, selectTargetsEvenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "If that target is in a play area with a Goalposts card, {TheGoalieCharacter} deals another target in that play area 2 irreducible projectile damage."
            if (firstTargeting != null && firstTargeting.Count > 0)
            {
                Card firstTarget = firstTargeting.FirstOrDefault().SelectedCard;
                Location playArea = firstTarget.Location;
                if (playArea.IsInPlay && NumGoalpostsAt(playArea) > 0)
                {
                    IEnumerator projectileCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Projectile, new int?(1), false, new int?(1), isIrreducible: true, additionalCriteria: (Card c) => c.Location == playArea && c != firstTarget, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(projectileCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(projectileCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
