using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class InfiltrationDroneCardController : EmpireUtilityCardController
    {
        public InfiltrationDroneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLowestHP(ranking: 2, numberOfTargets: () => 1, cardCriteria: new LinqCardCriteria((Card c) => c.IsCharacter && c.IsTarget, "character card target", false, false, "character card target", "character card targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals the character card target with the second lowest HP {H - 2} projectile damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DealDamageToLowestHP(base.Card, 2, (Card c) => c.IsCharacter && c.IsTarget, (Card c) => H - 2, DamageType.Projectile), TriggerType.DealDamage);
        }
    }
}
