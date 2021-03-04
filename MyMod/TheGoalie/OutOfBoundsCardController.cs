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
    public class OutOfBoundsCardController : TheGoalieUtilityCardController
    {
        public OutOfBoundsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.PlayArea, new LinqCardCriteria((Card c) => IsGoalposts(c), "goalposts"));
        }

        public override IEnumerator Play()
        {
            // "Destroy all Goalposts cards in your play area."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.Location == base.TurnTaker.PlayArea && IsGoalposts(c)), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Draw 3 cards."
            IEnumerator drawCoroutine = base.DrawCards(base.HeroTurnTakerController, 3, optional: false, upTo: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            yield break;
        }
    }
}
