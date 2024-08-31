using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class UnseenHandCardController : EmpireUtilityCardController
    {
        public UnseenHandCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHighestHP(2, cardCriteria: new LinqCardCriteria((Card c) => !c.DoKeywordsContain(AuthorityKeyword), "non-Imperial", singular: "target", plural: "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals the non-Imperial target with the second highest HP {H - 2} projectile damage. Reduce damage dealt by a target dealt damage this way by 1 until the start of the environment turn."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DealDamageToHighestHP(base.Card, 2, (Card c) => !c.DoKeywordsContain(AuthorityKeyword), (Card c) => H - 2, DamageType.Projectile, addStatusEffect: ReduceDamageResponse), TriggerType.DealDamage);
        }

        public IEnumerator ReduceDamageResponse(DealDamageAction dda)
        {
            // "... Reduce damage dealt by a target dealt damage this way by 1 until the start of the environment turn."
            ReduceDamageStatusEffect stunEffect = new ReduceDamageStatusEffect(1);
            stunEffect.SourceCriteria.IsSpecificCard = dda.Target;
            stunEffect.UntilStartOfNextTurn(base.TurnTaker);
            stunEffect.UntilCardLeavesPlay(dda.Target);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(stunEffect, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }
    }
}
