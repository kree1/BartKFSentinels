using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class LucrativeContractCardController : CardController
    {
        public LucrativeContractCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card is indestructible."
            if (card == Card)
                return true;
            return base.AskIfCardIsIndestructible(card);
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "... next to the hero character target with the second highest HP."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = GameController.FindTargetWithHighestHitPoints(2, (Card c) => IsHeroCharacterCard(c) && c.IsTarget, results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            Card toMark = results.FirstOrDefault();
            if (toMark != null)
            {
                storedResults?.Add(new MoveCardDestination(toMark.NextToLocation, showMessage: true));
            }
        }
    }
}
