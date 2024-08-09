using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class PowerChaargeCardController : TeamModCardController
    {
        public PowerChaargeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a hero target in this play area deals a non-hero target exactly 2 damage, increase damage dealt by hero targets in this play area this turn by 1."
            AddTrigger((DealDamageAction dda) => !IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DidDealDamage && dda.Amount == 2, (DealDamageAction dda) => IncreaseDamageDealtByHeroTargetsInThisPlayAreaThisTurn(1), TriggerType.CreateStatusEffect, TriggerTiming.After);
        }
    }
}
