using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class UnlikelyExplosiveCardController : EvidenceStorageUtilityCardController
    {
        public UnlikelyExplosiveCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            // "Whenever a card from that target's deck is played, this card deals the non-environment target with the highest HP in each other play area 1 energy damage, then deals itself 1 energy damage."
            base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner == cepa.CardEnteringPlay.Owner && !cepa.CardEnteringPlay.IsCharacter, ExplosionResponse, TriggerType.DealDamage, TriggerTiming.After, isActionOptional: false);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, move it next to the target other than itself in this play area with the highest HP."
            // Identify that target
            List<Card> highestList = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c != base.Card && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, highestList, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (highestList != null && highestList.Count() > 0)
            {
                IEnumerator grabCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, highestList.FirstOrDefault().NextToLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(grabCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(grabCoroutine);
                }
                // "This card deals that target 2 energy damage."
                IEnumerator zapCoroutine = base.GameController.DealDamage(base.DecisionMaker, base.Card, (Card c) => c == GetCardThisCardIsNextTo(), 2, DamageType.Energy, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(zapCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(zapCoroutine);
                }
            }
            else
            {
                string message = base.Card.Location.HighestRecursiveLocation.OwnerName + "'s play area has no other targets for " + base.Card.Title + " to move next to.";
                IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator GrabResponse(MoveCardAction mca)
        {
            yield break;
        }

        public IEnumerator SparkResponse(Location loc)
        {
            // // "... this card deals the non-environment target with the highest HP in [this play area] 1 energy damage."
            if (loc.Cards.Any((Card c) => c.IsTarget))
            {
                IEnumerator zapCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => c.Location.HighestRecursiveLocation == loc, (Card c) => 1, DamageType.Energy, numberOfTargets: () => 1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(zapCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(zapCoroutine);
                }
            }
            else
            {
                string message = "There are no targets in " + loc.OwnerName + "'s play area for " + base.Card.Title + " to deal damage to.";
                IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator ExplosionResponse(CardEntersPlayAction cepa)
        {
            // "... this card deals the non-environment target with the highest HP in each other play area 1 energy damage, ..."
            IEnumerator explodeCoroutine = base.GameController.SelectLocationsAndDoAction(base.DecisionMaker, SelectionType.DealDamage, (Location loc) => loc.IsInPlay && loc.HighestRecursiveLocation == loc, SparkResponse, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(explodeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(explodeCoroutine);
            }
            // "then deals itself 1 energy damage."
            IEnumerator selfDamageCoroutine = base.DealDamage(base.Card, base.Card, 1, DamageType.Energy, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDamageCoroutine);
            }
            yield break;
        }
    }
}
