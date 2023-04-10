using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class EverythingIsFineCardController : CardController
    {
        public EverythingIsFineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Non-environment cards cannot deal damage."
            AddCannotDealDamageTrigger((Card c) => !c.IsEnvironment);
            // "At the end of the environment turn, destroy 1 hero Ongoing card and 1 villain Ongoing card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyOngoingResponse, TriggerType.DestroyCard);
            // "At the start of the environment turn, deal each target 2 psychic damage and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDestructResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        public IEnumerator DestroyOngoingResponse(GameAction ga)
        {
            // "... destroy 1 hero Ongoing card and 1 villain Ongoing card."
            IEnumerator destroyHeroCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c), "hero Ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyHeroCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyHeroCoroutine);
            }
            IEnumerator destroyVillainCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsVillain(c) && IsOngoing(c), "villain Ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyVillainCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyVillainCoroutine);
            }
            yield break;
        }

        public IEnumerator DamageDestructResponse(GameAction ga)
        {
            // "... deal each target 2 psychic damage and destroy this card."
            IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => true, 2, DamageType.Psychic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDestructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDestructCoroutine);
            }
            yield break;
        }
    }
}
