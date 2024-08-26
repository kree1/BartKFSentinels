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
    public class BodyArmorCardController : CardController
    {
        public BodyArmorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {TheEqualizer} and this card by 1."
            AddReduceDamageTrigger((Card c) => c == CharacterCard || c == Card, 1);
            // "If {TheEqualizer} would be dealt 4 or more damage at once, prevent that damage and destroy this card."
            AddPreventDamageTrigger((DealDamageAction dda) => dda.Target == CharacterCard && dda.Amount >= 4, (DealDamageAction dda) => GameController.DestroyCard(DecisionMaker, Card, cardSource: GetCardSource()), new TriggerType[] { TriggerType.DestroySelf }, isPreventEffect: true);
        }
    }
}
