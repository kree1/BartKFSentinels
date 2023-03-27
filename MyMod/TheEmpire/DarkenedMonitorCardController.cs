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
    public class DarkenedMonitorCardController : EmpireUtilityCardController
    {
        public DarkenedMonitorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHighestHP(ranking: 1, numberOfTargets: () => H - 1, cardCriteria: new LinqCardCriteria((Card c) => !c.DoKeywordsContain(AuthorityKeyword)));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, destroy a hero Ongoing card. Then, this card deals the {H - 1} non-Imperial targets with the highest HP 1 lightning damage each."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyDamageSequence, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DealDamage });
        }

        public IEnumerator DestroyDamageSequence(PhaseChangeAction pca)
        {
            // "... destroy a hero Ongoing card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c), "hero Ongoing", true), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Then, this card deals the {H - 1} non-Imperial targets with the highest HP 1 lightning damage each."
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => !c.DoKeywordsContain(AuthorityKeyword), (Card c) => 1, DamageType.Lightning, numberOfTargets: () => H - 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
