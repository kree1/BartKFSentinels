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
    public class EqualSunCardController : OwnershipUtilityCardController
    {
        public EqualSunCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show total damage dealt to non-hero targets by hero targets this turn
            SpecialStringMaker.ShowSpecialString(() => DamageDealtToNonHeroByHeroThisTurn() + " damage has been dealt to non-hero targets by hero targets this turn.");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of each turn, if exactly 7 damage was dealt to non-hero targets by hero targets last turn, increase the next damage dealt by a hero target by 7."
            AddStartOfTurnTrigger((TurnTaker tt) => true, (PhaseChangeAction pca) => IncreaseNextDamageDealtByAHeroTarget(7), TriggerType.CreateStatusEffect, (PhaseChangeAction pca) => (from ddje in base.Journal.DealDamageEntriesOnTurn(base.Game.TurnIndex.Value - 1) where !IsHeroTarget(ddje.TargetCard) && ddje.SourceCard != null && IsHeroTarget(ddje.SourceCard) select ddje).Sum((DealDamageJournalEntry ddje) => ddje.Amount) == 7);
        }
    }
}
