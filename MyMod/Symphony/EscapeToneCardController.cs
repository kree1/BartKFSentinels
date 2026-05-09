using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class EscapeToneCardController : CardController
    {
        public EscapeToneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a non-hero target would deal damage, you may redirect it to {Symphony}."
            AddRedirectDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget && !IsHeroTarget(dda.DamageSource.Card), () => CharacterCard, optional: true);
            // "Reduce damage redirected this way by 1."
            AddTrigger((RedirectDamageAction rda) => rda.CardSource != null && rda.CardSource.Card == Card, (RedirectDamageAction rda) => GameController.ReduceDamage(rda.DealDamageAction, 1, null, cardSource: GetCardSource()), TriggerType.ReduceDamage, TriggerTiming.After);
            // "At the start of your turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
