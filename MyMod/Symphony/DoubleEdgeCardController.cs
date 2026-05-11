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
    public abstract class DoubleEdgeCardController : BenefitCardController
    {
        public DoubleEdgeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            IEnumerator effectCoroutine = OneShotEffect();
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(effectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(effectCoroutine);
            }
            // "Discard a silence card."
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(DecisionMaker, additionalCriteria: IsSilence, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Discard up to [#] cards."
            if (_toDiscard > 0)
            {
                IEnumerator benefitCoroutine = GameController.SelectAndDiscardCards(DecisionMaker, _toDiscard, false, 0, allowAutoDecide: _toDiscard >= DecisionMaker.HeroTurnTaker.Hand.Cards.Count(), cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(benefitCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(benefitCoroutine);
                }
            }
        }
    }
}
