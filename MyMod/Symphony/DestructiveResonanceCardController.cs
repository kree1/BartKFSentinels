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
            // "Destroy 1 ongoing, equipment, or non-target environment card. Draw 2 cards."
            int toDestroy = GetPowerNumeral(0, 1);
            int numDraws = GetPowerNumeral(1, 2);
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c) || (c.IsEnvironment && !c.IsTarget), "ongoing, equipment, or non-target environment"), toDestroy, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, numDraws);
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
