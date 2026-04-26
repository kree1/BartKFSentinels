using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class ObstacleAvoidanceCardController : CostCardController
    {
        public ObstacleAvoidanceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {Symphony} by non-hero sources by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.Target == CharacterCard && (dda.DamageSource == null || (dda.DamageSource.IsTurnTaker && !IsHero(dda.DamageSource.TurnTaker)) || (dda.DamageSource.IsCard && !IsHero(dda.DamageSource.Card))), (DealDamageAction dda) => 1);
            // "At the end of your turn, draw 3 cards."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => DrawCards(DecisionMaker, 3), TriggerType.DrawCard);
        }
    }
}
