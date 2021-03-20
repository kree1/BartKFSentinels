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
    public class FacelessDuplicateCardController : EvidenceStorageUtilityCardController
    {
        public FacelessDuplicateCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has redirected damage this turn", () => base.Card.Title + " has not redirected damage this turn").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected const string OncePerTurn = "RedirectOncePerTurn";
        private ITrigger RedirectDamageTrigger;

        public override void AddTriggers()
        {
            // "When this card enters a play area with an active character card, move it next to that character."
            base.AddTrigger<MoveCardAction>((MoveCardAction mca) => mca.CardToMove == base.Card && mca.Destination.IsInPlay, BuddyUpResponse, TriggerType.MoveCard, TriggerTiming.After, isActionOptional: false);
            // "The first time that character would be dealt damage each turn, redirect that damage to this card."
            this.RedirectDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Target == GetCardThisCardIsNextTo() && dda.Amount > 0, RedirectDamageResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            // "When this card is dealt lightning damage, move it to the play area of the source of that damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target == base.Card && dda.DamageSource != null && dda.DamageSource.Card != null && dda.DidDealDamage, FollowLightningResponse, TriggerType.MoveCard, TriggerTiming.After, isActionOptional: false);
            // [If the card this is next to leaves play, this card falls off and stays in their play area]
            base.AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false, true);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            Log.Debug(base.Card.Title + " entered play in " + base.Card.Location.HighestRecursiveLocation.GetFriendlyName() + ".");
            if (base.Card.Location.Cards.Any((Card c) => c.IsCharacter && c.IsActive))
            {
                // "When this card enters a play area with an active character card, move it next to that character."
                Log.Debug(base.Card.Title + " entered play in " + base.Card.Location.GetFriendlyName() + ", which has an active character. Moving it next to someone...");
                List<MoveCardDestination> storedResultsPlace = new List<MoveCardDestination>();
                IEnumerator buddyCoroutine = base.SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsActive && c.IsCharacter && c.Location.HighestRecursiveLocation == base.Card.Location), storedResultsPlace, false, null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(buddyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(buddyCoroutine);
                }
                if (storedResultsPlace != null && storedResultsPlace.Count() > 0)
                {
                    IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, storedResultsPlace.FirstOrDefault().Location, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, doesNotEnterPlay: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                    Log.Debug(base.Card.Title + " moved to " + storedResultsPlace.FirstOrDefault().Location.GetFriendlyName() + ".");
                }
            }
            yield break;
        }

        public IEnumerator BuddyUpResponse(MoveCardAction mca)
        {
            Log.Debug(base.Card.Title + " moved to " + mca.Destination.GetFriendlyName() + ".");
            if (mca.Destination.Cards.Any((Card c) => c.IsCharacter && c.IsActive))
            {
                // "When this card enters a play area with an active character card, move it next to that character."
                Log.Debug(base.Card.Title + " moved to " + mca.Destination.GetFriendlyName() + ", which has an active character. Moving it next to someone...");
                List<MoveCardDestination> storedResultsPlace = new List<MoveCardDestination>();
                IEnumerator buddyCoroutine = base.SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsActive && c.IsCharacter && c.Location.HighestRecursiveLocation == mca.Destination), storedResultsPlace, false, null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(buddyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(buddyCoroutine);
                }
                if (storedResultsPlace != null && storedResultsPlace.Count() > 0)
                {
                    IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, storedResultsPlace.FirstOrDefault().Location, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: mca, doesNotEnterPlay: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                    Log.Debug(base.Card.Title + " moved to " + storedResultsPlace.FirstOrDefault().Location.GetFriendlyName() + ".");
                }
            }
            yield break;
        }

        public IEnumerator RedirectDamageResponse(DealDamageAction dda)
        {
            // "The first time [the character this card is next to] would be dealt damage each turn, redirect that damage to this card."
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            yield break;
        }

        public IEnumerator FollowLightningResponse(DealDamageAction dda)
        {
            // "When this card is dealt lightning damage, move it to the play area of the source of that damage."
            Card source = dda.DamageSource.Card;
            Location dest = source.Location.HighestRecursiveLocation;
            string message = base.Card.Title + " follows the charge back to " + source.Title + "'s play area...";
            List<Card> associated = null;
            if (dda.CardSource.Card != null)
            {
                message = base.Card.Title + " follows the charge from " + dda.CardSource.Card.Title + " back to " + source.Title + "'s play area...";
                associated = new List<Card>();
                associated.Add(dda.CardSource.Card);
            }
            IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), associatedCards: associated, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(showCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(showCoroutine);
            }
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, dest, playCardIfMovingToPlayArea: false, showMessage: false, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: dda, doesNotEnterPlay: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield break;
        }
    }
}
