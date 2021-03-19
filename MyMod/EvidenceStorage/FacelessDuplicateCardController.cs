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
            base.AddTrigger<MoveCardAction>((MoveCardAction mca) => mca.CardToMove == base.Card && mca.Destination.Cards.Any((Card c) => c.IsCharacter), BuddyUpResponse, TriggerType.MoveCard, TriggerTiming.After, isActionOptional: false);
            // "The first time that character would be dealt damage each turn, redirect that damage to this card."
            this.RedirectDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Target == GetCardThisCardIsNextTo() && dda.Amount > 0, RedirectDamageResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            // "When this card is dealt lightning damage, move it to the play area of the source of that damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageType == DamageType.Lightning && dda.Target == base.Card && dda.DamageSource != null && dda.DamageSource.Card != null && dda.DidDealDamage, FollowLightningResponse, TriggerType.MoveCard, TriggerTiming.After, isActionOptional: false);
            // [If the card this is next to leaves play, this card falls off and stays in their play area]
            base.AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false, true);
            base.AddTriggers();
        }

        public IEnumerator BuddyUpResponse(MoveCardAction mca)
        {
            // "When this card enters a play area with an active character card, move it next to that character."
            HeroTurnTakerController selector = base.DecisionMaker;
            IEnumerator buddyCoroutine = base.SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsActive && c.IsCharacter && c.Location.HighestRecursiveLocation == mca.Destination), null, false, null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(buddyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(buddyCoroutine);
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
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, dest, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: dda, doesNotEnterPlay: true, cardSource: GetCardSource());
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
