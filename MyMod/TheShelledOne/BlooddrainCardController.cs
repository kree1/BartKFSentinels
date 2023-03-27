using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class BlooddrainCardController : BlaseballWeatherCardController
    {
        public BlooddrainCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, destroy {H - 1} hero Ongoing cards."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyResponse, TriggerType.DestroyCard);
            // "Whenever a card is destroyed this way, 1 villain target with less than its maximum HP regains 1 HP."
            AddTrigger((DestroyCardAction dca) => dca.CardSource != null && dca.CardSource.Card == base.Card && dca.ResponsibleCard == base.Card && dca.WasCardDestroyed, HealResponse, TriggerType.GainHP, TriggerTiming.After);
        }

        public IEnumerator DestroyResponse(GameAction ga)
        {
            // "... destroy {H - 1} hero Ongoing cards."
            LinqCardCriteria heroOngoingInPlay = new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c) && c.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), "hero Ongoing");
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, heroOngoingInPlay, H - 1, requiredDecisions: H - 1, allowAutoDecide: base.GameController.FindCardsWhere(heroOngoingInPlay, visibleToCard: GetCardSource()).Count() <= H - 1, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        public IEnumerator HealResponse(DestroyCardAction dca)
        {
            // "... 1 villain target with less than its maximum HP regains 1 HP."
            IEnumerator healVillainCoroutine = base.GameController.SelectAndGainHP(base.DecisionMaker, 1, additionalCriteria: (Card c) => c.IsVillainTarget && c.HitPoints < c.MaximumHitPoints, numberOfTargets: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(healVillainCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(healVillainCoroutine);
            }
            yield break;
        }
    }
}
