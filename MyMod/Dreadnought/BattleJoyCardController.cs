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
    public class BattleJoyCardController : StressCardController
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
            // "At the start of your turn, if you have 3 or more cards in your trash, draw a card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => DrawCard(HeroTurnTaker), TriggerType.DrawCard, additionalCriteria: (PhaseChangeAction pca) => TurnTaker.Trash.NumberOfCards >= 3);
        }
    }
}
