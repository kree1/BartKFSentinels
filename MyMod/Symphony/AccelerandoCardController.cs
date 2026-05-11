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
    public class AccelerandoCardController : NeutralCardController
    {
        public AccelerandoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator OneShotEffect()
        {
            // "Play the top card of a deck."
            List<SelectLocationDecision> selectResults = new List<SelectLocationDecision>();
            IEnumerator selectCoroutine = GameController.SelectADeck(DecisionMaker, SelectionType.PlayTopCard, (Location l) => l.IsDeck, selectResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidSelectLocation(selectResults))
            {
                IEnumerator playCoroutine = GameController.PlayTopCardOfLocation(TurnTakerController, GetSelectedLocation(selectResults), responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(playCoroutine);
                }
            }
        }
    }
}
