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
    public class ShootoutCardController : TheGoalieUtilityCardController
    {
        public ShootoutCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsGoalposts(c) && c.IsInPlayAndHasGameText && !c.Location.IsHero), specifyPlayAreas: true).Condition = () => NumGoalpostsInNonHeroPlayAreas() > 0;
        }

        public override IEnumerator Play()
        {
            // "{TheGoalieCharacter} deals each non-hero target 2 projectile damage."
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, (Card c) => !c.IsHero, 2, DamageType.Projectile, optional: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Destroy a Goalposts card in a non-hero play area."
            if (NumGoalpostsInNonHeroPlayAreas() > 0)
            {
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsGoalposts(c) && !c.Location.IsHero), false, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }
    }
}
