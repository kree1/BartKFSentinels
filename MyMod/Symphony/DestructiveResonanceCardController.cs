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
    public class DestructiveResonanceCardController : CostCardController
    {
        public DestructiveResonanceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "You may destroy 1 of your non-character cards or 1 ongoing or non-target environment card. Draw 3 cards."
            int numYours = GetPowerNumeral(0, 1);
            int numTheirs = GetPowerNumeral(1, 1);
            int numDraws = GetPowerNumeral(2, 3);
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Destroy " + numYours.ToString() + " of your non-character cards", SelectionType.DestroyCard, () => GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.Owner == TurnTaker && !c.IsCharacter, "belonging to " + TurnTaker.Name, singular: "non-character card", plural: "non-character cards", useCardsPrefix: true, useCardsSuffix: false), true, cardSource: GetCardSource()), repeatDecisionText: "destroy 1 of your non-character cards"));
            options.Add(new Function(DecisionMaker, "Destroy " + numTheirs.ToString() + " ongoing or non-target environment card", SelectionType.DestroyCard, () => GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) || (c.IsEnvironment && !c.IsTarget), "ongoing or non-target environment"), true, cardSource: GetCardSource()), repeatDecisionText: "destroy 1 ongoing or non-target environment card"));
            SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, true, cardSource: GetCardSource());
            IEnumerator destroyCoroutine = GameController.SelectAndPerformFunction(choice);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, 3);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
        }
    }
}
