using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class PlaceOfPowerCardController : TheGoalieUtilityCardController
    {
        public PlaceOfPowerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {TheGoalieCharacter} by 1."
            AddReduceDamageTrigger((Card c) => c == base.CharacterCard, 1);
            // "Increase non-melee damage dealt by {TheGoalieCharacter} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card == base.CharacterCard && dda.DamageType != DamageType.Melee, (DealDamageAction dda) => 1);
            // "At the end of your turn, {TheGoalieCharacter} regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.GainHP(base.CharacterCard, 1, cardSource: GetCardSource()), TriggerType.GainHP);
        }
    }
}
