using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class PeristalticPassageCardController : TheGigamorphUtilityCardController
    {
        public PeristalticPassageCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Targets can't deal damage to other targets."
            base.AddPreventDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsTarget && dda.DamageSource.Card != dda.Target, isPreventEffect: false);
            // "At the start of the environment turn, each player draws a card. Then, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DrawAndDestroyResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.DestroySelf });
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy 1 hero Ongoing card and 1 villain Ongoing card."
            IEnumerator heroDestroyCoroutine = base.GameController.SelectAndDestroyCard(base.DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c), "hero Ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(heroDestroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(heroDestroyCoroutine);
            }
            IEnumerator villainDestroyCoroutine = base.GameController.SelectAndDestroyCard(base.DecisionMaker, new LinqCardCriteria((Card c) => IsVillain(c) && IsOngoing(c), "villain Ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(villainDestroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(villainDestroyCoroutine);
            }
        }

        public IEnumerator DrawAndDestroyResponse(PhaseChangeAction pca)
        {
            // " each player draws a card."
            IEnumerator drawCoroutine = EachPlayerDrawsACard();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Then, destroy this card."
            IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDestructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDestructCoroutine);
            }
        }
    }
}
