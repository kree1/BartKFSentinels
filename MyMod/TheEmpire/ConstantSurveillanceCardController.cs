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
    public class ConstantSurveillanceCardController : EmpireUtilityCardController
    {
        public ConstantSurveillanceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            LinqCardCriteria otherImperialTargetInPlay = new LinqCardCriteria((Card c) => c != base.Card && c.IsTarget && c.DoKeywordsContain(AuthorityKeyword) && c.IsInPlayAndHasGameText, "other Imperial targets in play", false, false, "other Imperial target in play", "other Imperial targets in play");
            SpecialStringMaker.ShowListOfCardsInPlay(otherImperialTargetInPlay);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Damage dealt by Imperial cards is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.DoKeywordsContain(AuthorityKeyword));
            // "At the end of the environment turn, if there are no other Imperial targets in play, play the top card of the environment deck."
            LinqCardCriteria otherImperialTargetInPlay = new LinqCardCriteria((Card c) => c != base.Card && c.IsTarget && c.DoKeywordsContain(AuthorityKeyword) && c.IsInPlayAndHasGameText, "other Imperial targets in play", false, false, "other Imperial target in play", "other Imperial targets in play");
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && !base.GameController.FindCardsWhere(otherImperialTargetInPlay, visibleToCard: GetCardSource()).Any(), PlayTheTopCardOfTheEnvironmentDeckResponse, TriggerType.PlayCard);
        }
    }
}
