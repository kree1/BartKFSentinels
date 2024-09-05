using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class BattleJoyCardController : CardController
    {
        public BattleJoyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever {Dreadnought} deals non-psychic damage to another target, {Dreadnought} regains 1 HP."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard) && dda.DidDealDamage && dda.DamageType != DamageType.Psychic, (DealDamageAction dda) => GameController.GainHP(CharacterCard, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            // "Whenever another of your Ongoing cards leaves play, you may draw a card."
            AddTrigger((MoveCardAction mca) => mca.Origin.IsInPlay && !mca.Destination.IsInPlay && mca.CardToMove.Owner == TurnTaker && IsOngoing(mca.CardToMove) && mca.CardToMove != Card, (MoveCardAction mca) => DrawCard(TurnTaker.ToHero(), optional: true), TriggerType.DrawCard, TriggerTiming.After);
        }
    }
}
