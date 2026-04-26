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
    public abstract class BenefitCardController : CardController
    {
        public BenefitCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 0;
        }

        protected int _toDiscard { get; set; }

        public abstract IEnumerator OneShotEffect();

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
            // "Discard up to [#] cards."
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
