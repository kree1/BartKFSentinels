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
    public class ChronotonPulseCardController : CardController
    {
        public ChronotonPulseCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When this card ... leaves play, destroy {H - 1} hero Ongoing and/or Equipment cards."
            AddBeforeLeavesPlayAction(DestroyCardsResponse, TriggerType.DestroyCard);
            // "At the end of the villain turn, this card deals itself 1 energy damage."
            AddDealDamageAtEndOfTurnTrigger(TurnTaker, base.Card, (Card c) => c == base.Card, TargetType.All, 1, DamageType.Energy);
        }

        public override IEnumerator Play()
        {
            // "When this card enters ... play, destroy {H - 1} hero Ongoing and/or Equipment cards."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (IsOngoing(c) || IsEquipment(c)), "hero Ongoing or Equipment"), H - 1, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }

        private IEnumerator DestroyCardsResponse(GameAction ga)
        {
            // "... destroy {H - 1} hero Ongoing and/or Equipment cards."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (IsOngoing(c) || IsEquipment(c)), "hero Ongoing or Equipment"), H - 1, responsibleCard: base.Card, cardSource: GetCardSource());
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
