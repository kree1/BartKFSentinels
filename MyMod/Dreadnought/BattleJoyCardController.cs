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
            // "Whenever {Dreadnought} is dealt damage by a non-hero target, discard the top card of your deck."
            AddTrigger((DealDamageAction dda) => dda.Target == CharacterCard && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsTarget && !IsHeroTarget(dda.DamageSource.Card), (DealDamageAction dda) => GameController.DiscardTopCard(TurnTaker.Deck, null, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource()), TriggerType.DiscardCard, TriggerTiming.After);
            // "Whenever another of your Ongoing cards leaves play, you may draw a card."
            AddTrigger((MoveCardAction mca) => mca.Origin.IsInPlay && !mca.Destination.IsInPlay && mca.CardToMove.Owner == TurnTaker && IsOngoing(mca.CardToMove) && mca.CardToMove != Card, (MoveCardAction mca) => DrawCard(TurnTaker.ToHero(), optional: true), TriggerType.DrawCard, TriggerTiming.After);
        }
    }
}
