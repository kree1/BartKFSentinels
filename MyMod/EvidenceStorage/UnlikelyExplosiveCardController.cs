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
            base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner == cepa.CardEnteringPlay.Owner && !cepa.CardEnteringPlay.IsCharacter && !cepa.IsPutIntoPlay, ExplosionResponse, TriggerType.DealDamage, TriggerTiming.After, isActionOptional: false);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, move it next to the target other than itself in this play area with the highest HP."
            Log.Debug(base.Card.Title + " entered play! Moving it next to a target...");
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

        public IEnumerator ExplosionResponse(CardEntersPlayAction cepa)
        {
            // "... this card deals the non-environment target with the highest HP in each other play area 1 energy damage, ..."
            DealDamageAction previewDamage = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, 1, DamageType.Energy);
            List<Location> playAreas = new List<Location>();
            // Get each other play area
            foreach (TurnTaker tt in base.GameController.AllTurnTakers)
            {
                Location ttPlayArea = tt.PlayArea;
                if (ttPlayArea.HighestRecursiveLocation != base.Card.Location.HighestRecursiveLocation)
                {
                    playAreas.Add(ttPlayArea);
                }
            }
            // For each other play area, find its non-environment target with the highest HP
            List<Card> highestTargets = new List<Card>();
            foreach (Location playArea in playAreas)
            {
                if (playArea.Cards.Any((Card c) => c.IsTarget && !c.IsEnvironment))
                {
                    List<Card> localHighest = new List<Card>();
                    IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.Location.HighestRecursiveLocation == playArea && !c.IsEnvironment, localHighest, dealDamageInfo: previewDamage.ToEnumerable(), cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(findCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(findCoroutine);
                    }
                    if (localHighest != null && localHighest.Count() > 0)
                    {
                        highestTargets.Add(localHighest.FirstOrDefault());
                    }
                }
            }
            IEnumerator explodeCoroutine = base.DealDamage(base.Card, (Card c) => highestTargets.Contains(c), 1, DamageType.Energy);
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
