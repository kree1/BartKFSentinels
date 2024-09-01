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
    public class AggressionBoosterCardController : CardController
    {
        public AggressionBoosterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show environment target with highest HP
            SpecialStringMaker.ShowEnvironmentTargetWithHighestHP();
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this next to the environment target with the highest HP."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsEnvironmentTarget, results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highest = results.FirstOrDefault();
            if (highest == null)
            {
                storedResults?.Add(new MoveCardDestination(TurnTaker.PlayArea));
            }
            else
            {
                storedResults?.Add(new MoveCardDestination(highest.NextToLocation));
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Redirect damage that would be dealt to this card to that target."
            AddRedirectDamageTrigger((DealDamageAction dda) => dda.Target == Card && GetCardThisCardIsNextTo() != null, () => GetCardThisCardIsNextTo());
            // "Increase damage dealt by environment targets to hero targets by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsEnvironmentTarget, 1);
        }
    }
}
