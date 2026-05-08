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
    public class ExpandedEnsembleCardController : CostCardController
    {
        public ExpandedEnsembleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.IncreasePhaseActionCount);
        }

        public override bool DoesHaveActivePlayMethod => false;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "You may play an additional card during your play phase."
            AddAdditionalPhaseActionTrigger((TurnTaker tt) => tt == TurnTaker, Phase.PlayCard, 1);
            // "At the end of your turn, draw 2 cards."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => DrawCards(DecisionMaker, 2), TriggerType.DrawCard);
        }

        public override IEnumerator Play()
        {
            return IncreasePhaseActionCountIfInPhase((TurnTaker tt) => tt == TurnTaker, Phase.PlayCard, 1);
        }

        public override bool AskIfIncreasingCurrentPhaseActionCount()
        {
            return GameController.ActiveTurnPhase.Phase == Phase.PlayCard && GameController.ActiveTurnTaker == TurnTaker;
        }
    }
}
