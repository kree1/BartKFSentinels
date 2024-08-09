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
    public class UnderseaCardController : TeamModCardController
    {
        public UnderseaCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever damage that would be dealt by a hero target in this play area is reduced to 0 or less, increase damage dealt by hero targets in this play area this turn by 1."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && !dda.IsPretend && dda.Amount <= 0 && dda.DamageModifiers.Any((ModifyDealDamageAction mdda) => mdda is ReduceDamageAction), (DealDamageAction dda) => IncreaseDamageDealtByHeroTargetsInThisPlayAreaThisTurn(1), TriggerType.CreateStatusEffect, TriggerTiming.After);
        }
    }
}
