using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class AdhesiveGrenadeCardController : ChipCardController
    {
        public AdhesiveGrenadeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero target would deal damage to {Planetfall} or this card, first move this card next to that hero target."
            AddTrigger((DealDamageAction dda) => dda.Amount > 0 && (dda.Target == Card || dda.Target == CharacterCard) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card), (DealDamageAction dda) => GameController.MoveCard(TurnTakerController, Card, dda.DamageSource.Card.NextToLocation, playCardIfMovingToPlayArea: false, showMessage: GetCardThisCardIsNextTo() == null || dda.DamageSource.Card != GetCardThisCardIsNextTo(), responsibleTurnTaker: TurnTaker, doesNotEnterPlay: true, cardSource: GetCardSource()), new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.MoveCard }, TriggerTiming.Before);
            // "When this card is destroyed, it deals the target next to it 3 fire damage."
            AddWhenDestroyedTrigger((DestroyCardAction dca) => DealDamage(Card, GetCardThisCardIsNextTo(), 3, DamageType.Fire, isCounterDamage: true), new TriggerType[] { TriggerType.DealDamage }, (DestroyCardAction dca) => GetCardThisCardIsNextTo() != null);
        }
    }
}
