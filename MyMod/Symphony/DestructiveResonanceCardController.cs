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
            // "Destroy an ongoing, equipment, or non-target environment card. If a non-hero card was destroyed this way, draw 3 cards."
            int numDraws = GetPowerNumeral(0, 3);
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c) || (c.IsEnvironment && !c.IsTarget), "ongoing, equipment, or non-target environment"), false, storedResultsAction: destroyResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            if (destroyResults.Any((DestroyCardAction dca) => dca.WasCardDestroyed && !IsHero(dca.CardToDestroy.Card)))
            {
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
}
