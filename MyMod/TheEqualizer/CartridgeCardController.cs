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
    public abstract class CartridgeCardController : EqualizerUtilityCardController
    {
        public CartridgeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When {TheEqualizer} deals projectile damage to a target, ..."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.IsSameCard(CharacterCard) && dda.DamageType == DamageType.Projectile && dda.DidDealDamage, DealBonusDamageResponse, TriggerType.DealDamage, TriggerTiming.After);
            // "When a villain Munition is reduced to 0 or fewer HP, destroy this card."
            AddTrigger((DealDamageAction dda) => IsVillain(dda.Target) && GameController.DoesCardContainKeyword(dda.Target, MunitionKeyword) && dda.TargetHitPointsAfterBeingDealtDamage <= 0, DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
        }

        public abstract IEnumerator DealBonusDamageResponse(DealDamageAction dda);
    }
}
