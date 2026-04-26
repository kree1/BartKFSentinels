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
            // "You may destroy 1 of your non-character cards and/or 1 ongoing or non-target environment card. Draw 3 cards."
            int numYours = GetPowerNumeral(0, 1);
            int numTheirs = GetPowerNumeral(1, 1);
            int numDraws = GetPowerNumeral(2, 3);
            IEnumerator destroyYoursCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.Owner == TurnTaker && !c.IsCharacter, "belonging to " + TurnTaker.Name, singular: "non-character card", plural: "non-character cards", useCardsPrefix: true, useCardsSuffix: false), true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyYoursCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyYoursCoroutine);
            }
            IEnumerator destroyTheirsCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) || (c.IsEnvironment && !c.IsTarget), "ongoing or non-target environment"), true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyTheirsCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyTheirsCoroutine);
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
