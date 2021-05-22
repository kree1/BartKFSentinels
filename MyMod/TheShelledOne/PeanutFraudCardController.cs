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
    public class PeanutFraudCardController : StrikeCardController
    {
        public PeanutFraudCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever damage that would be dealt to a hero target is reduced, this card regains 1 HP."
            AddTrigger((ReduceDamageAction rda) => rda.DealDamageAction.Target.IsHero, (ReduceDamageAction rda) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            // "At the end of the villain turn, if this card has {H} or more HP, put a token on {TheShelledOne} and set this card's HP to 0."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && base.Card.HitPoints.HasValue && base.Card.HitPoints.Value >= H, AddTokenAndResetResponse, new TriggerType[] { TriggerType.AddTokensToPool, TriggerType.GainHP });
        }
    }
}
