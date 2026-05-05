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
    public class EscapeToneCardController : CardController
    {
        public EscapeToneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false, false);
            // "Redirect damage that would be dealt to that target to {Symphony}."
            AddRedirectDamageTrigger((DealDamageAction dda) => GetCardThisCardIsNextTo() != null && dda.Target == GetCardThisCardIsNextTo(), () => CharacterCard);
            // "At the start of your turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            return SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget, "", singular: "target", plural: "targets"), storedResults, isPutIntoPlay, decisionSources);
        }
    }
}
