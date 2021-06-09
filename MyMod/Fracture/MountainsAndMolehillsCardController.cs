using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class MountainsAndMolehillsCardController : FractureUtilityCardController
    {
        public MountainsAndMolehillsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt by non-hero targets by 2."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsTarget && !dda.DamageSource.IsHero, (DealDamageAction dda) => 2);
            // "At the start of your turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
