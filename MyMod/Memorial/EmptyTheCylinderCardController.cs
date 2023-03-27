using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class EmptyTheCylinderCardController : MemorialUtilityCardController
    {
        public EmptyTheCylinderCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHighestHP(cardCriteria: new LinqCardCriteria((Card c) => IsRenownedTarget(c), "Renowned target", false, false, "Renowned target", "Renowned targets")).Condition = () => AllRenownedTargets().Any();
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override IEnumerator Play()
        {
            // "Select the [b]Renowned[/b] target with the highest HP. If there are no [b]Renowned[/b] targets, select the hero target with the highest HP instead."
            List<Card> storedResults = new List<Card>();
            IEnumerator findCoroutine = null;
            if (AllRenownedTargets().Any())
            {
                findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsRenownedTarget(c), storedResults, cardSource: GetCardSource());
            }
            else
            {
                findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsHeroTarget(c), storedResults, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (storedResults.Count() > 0)
            {
                // "{Memorial} deals the selected target 2 irreducible projectile damage {H - 1} times."
                List<DealDamageAction> bullets = new List<DealDamageAction>();
                for (int i = 0; i < H - 1; i++)
                {
                    bullets.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.CharacterCard), null, 2, DamageType.Projectile, isIrreducible: true));
                }
                IEnumerator damageCoroutine = DealMultipleInstancesOfDamage(bullets, (Card c) => c == storedResults.FirstOrDefault());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
        }
    }
}
