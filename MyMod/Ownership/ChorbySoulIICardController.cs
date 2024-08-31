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
    public class ChorbySoulIICardController : OwnershipUtilityCardController
    {
        public ChorbySoulIICardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show amount of damage dealt to non-hero targets by hero targets this turn
            SpecialStringMaker.ShowSpecialString(() => DamageDealtToNonHeroByHeroThisTurn() + " damage has been dealt to non-hero targets by hero targets this turn.");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to this card by 1."
            AddReduceDamageTrigger((Card c) => c == base.Card, 1);
            // "At the end of each hero turn, if no damage was dealt to non-hero targets by hero targets this turn, this card deals a hero character in this turn's play area 2 projectile damage."
            AddEndOfTurnTrigger((TurnTaker tt) => IsHero(tt) && DamageDealtToNonHeroByHeroThisTurn() == 0, (PhaseChangeAction pca) => base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), 2, DamageType.Projectile, 1, false, 1, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Location.IsPlayAreaOf(base.Game.ActiveTurnTaker), cardSource: GetCardSource()), TriggerType.DealDamage);
        }
    }
}
