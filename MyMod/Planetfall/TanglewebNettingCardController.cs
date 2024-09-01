using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class TanglewebNettingCardController : CardController
    {
        public TanglewebNettingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero with lowest HP
            SpecialStringMaker.ShowHeroCharacterCardWithLowestHP();
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this next to the hero with the lowest HP."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = GameController.FindTargetWithLowestHitPoints(1, (Card c) => IsHeroCharacterCard(c) && !c.IsIncapacitatedOrOutOfGame, results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            Card lowest = results.FirstOrDefault();
            if (lowest != null)
            {
                storedResults?.Add(new MoveCardDestination(lowest.NextToLocation));
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Redirect damage dealt by that hero to this card."
            AddRedirectDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(GetCardThisCardIsNextTo()), () => Card);
            // "Reduce damage dealt to this card by 1."
            AddReduceDamageTrigger((Card c) => c == Card, 1);
        }
    }
}
