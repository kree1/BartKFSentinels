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
    public class ReloadCardController : TeamModCardController
    {
        public ReloadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero target in this play area deals a non-hero target 5 or more damage, increase the next damage dealt by a hero target by 2."
            AddTrigger((DealDamageAction dda) => !IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DidDealDamage && dda.Amount >= 5, (DealDamageAction dda) => IncreaseNextDamageDealtByAHeroTarget(2), TriggerType.CreateStatusEffect, TriggerTiming.After);
        }
    }
}
