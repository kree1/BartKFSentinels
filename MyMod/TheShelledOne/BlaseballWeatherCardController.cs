using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class BlaseballWeatherCardController : CardController
    {
        public BlaseballWeatherCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy all other Weather Effects."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.DoKeywordsContain("weather effect") && c != base.Card, "other Weather Effects", false, false, "other Weather Effect", "other Weather Effects"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
