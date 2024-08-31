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
    public class UprisingCardController : EmpireUtilityCardController
    {
        public UprisingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals each non-hero, non-Dissenter target 1 melee damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => !IsHeroTarget(c) && !c.DoKeywordsContain(AllyKeyword), 1, DamageType.Melee, cardSource: GetCardSource()), TriggerType.DealDamage);
            // "If there are any cards under the Timeline card, damage dealt by this card is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => base.TurnTaker.FindCard(TimelineIdentifier).UnderLocation.HasCards && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.Card);
        }
    }
}
