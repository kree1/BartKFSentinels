using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class CombatShotgunCardController : MunitionCardController
    {
        public CombatShotgunCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show H-2 non-Marked hero targets with highest HP
            SpecialStringMaker.ShowHighestHP(numberOfTargets: () => H - 2, cardCriteria: new LinqCardCriteria((Card c) => IsHeroTarget(c) && !IsMarked(c), "non-[b][i]Marked[/i][/b] hero", singular: "target", plural: "targets"));
        }

        public override IEnumerator SalvoAttack()
        {
            // "{TheEqualizer} deals the [b][i]Marked[/i][/b] target 4 projectile damage, ..."
            IEnumerator shootMarkedCoroutine = DealDamage(CharacterCard, MarkedTarget(GetCardSource()), 4, DamageType.Projectile, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(shootMarkedCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(shootMarkedCoroutine);
            }
            // "... then deals the {H - 2} non-[b][i]Marked[/i][/b] hero targets with the highest HP 2 projectile damage each."
            IEnumerator shootOtherCoroutine = DealDamageToHighestHP(CharacterCard, 1, (Card c) => IsHeroTarget(c) && !IsMarked(c), (Card c) => 2, DamageType.Projectile, numberOfTargets: () => H - 2);
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(shootOtherCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(shootOtherCoroutine);
            }
        }
    }
}
