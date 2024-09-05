using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class WovenIntoRealityCardController : StressCardController
    {
        public WovenIntoRealityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may draw a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => DrawCard(HeroTurnTaker, optional: true), TriggerType.DrawCard);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int cardsInstructed = GetPowerNumeral(0, 2);
            int cardsRequired = GetPowerNumeral(1, 2);
            int damageAmt = GetPowerNumeral(2, 3);
            // "Destroy an Ongoing or non-target environment card."
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) || (c.IsEnvironment && !c.IsTarget), "Ongoing or non-target environment"), false, responsibleCard: Card, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Put the bottom 2 cards of your trash on the bottom of your deck. If you moved fewer than 2 cards this way, {Dreadnought} deals herself 3 irreducible psychic damage."
            IEnumerator stressCoroutine = StressResponse(cardsInstructed, cardsRequired, damageAmt);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(stressCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(stressCoroutine);
            }
        }
    }
}
