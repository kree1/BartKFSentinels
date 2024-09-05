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
    public class StrongerThanATankCannonCardController : DreadnoughtUtilityCardController
    {
        public StrongerThanATankCannonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether Dreadnought has dealt damage this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => DamageDealtByDreadnoughtThisTurn() > 0, () => CharacterCard.Title + " has already dealt damage this turn.", () => CharacterCard.Title + " has not dealt damage this turn.");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by {Dreadnought} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard), 1);
            // "At the start of your turn, if {Dreadnought} has dealt no damage this turn, she deals each other character card target 0 psychic damage."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => DealDamage(CharacterCard, (Card c) => c.IsCharacter && c != CharacterCard, (Card c) => 0, DamageType.Psychic), TriggerType.DealDamage, (PhaseChangeAction pca) => DamageDealtByDreadnoughtThisTurn() <= 0);
        }
    }
}
