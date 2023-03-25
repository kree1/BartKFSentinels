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
    public class LivingThunderheadCardController : CardController
    {
        public LivingThunderheadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroTargetWithHighestHP(numberOfTargets: 2);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals the 2 hero targets with the highest HP {H - 1} sonic damage each."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DealDamageToHighestHP(base.Card, 1, (Card c) => c.IsHero, (Card c) => H - 1, DamageType.Sonic, numberOfTargets: () => 2), TriggerType.DealDamage);
            // "At the start of the villain turn, destroy a hero Ongoing or Equipment card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyCardResponse, TriggerType.DestroyCard);
        }

        private IEnumerator DestroyCardResponse(PhaseChangeAction pca)
        {
            // "... destroy a hero Ongoing or Equipment card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (IsOngoing(c) || IsEquipment(c)), "hero Ongoing or Equipment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
