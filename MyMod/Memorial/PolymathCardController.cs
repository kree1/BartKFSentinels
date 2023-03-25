using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class PolymathCardController : RenownCardController
    {
        public PolymathCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            //SpecialStringMaker.ShowNumberOfCardsAtLocation(GetCardThisCardIsNextTo().Owner.ToHero().Hand).Condition = () => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner.IsHero;
            //SpecialStringMaker.ShowNumberOfCardsAtLocation(GetCardThisCardIsNextTo().Owner.ToHero().Hand, new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "Ongoing and/or Equipment")).Condition = () => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().Owner.IsHero;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of this play area's turn, this hero's player may discard a card to play an Ongoing or Equipment card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == Card.Location.HighestRecursiveLocation.OwnerTurnTaker && GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsHeroCharacterCard && GetCardThisCardIsNextTo().Owner.IsHero && GetCardThisCardIsNextTo().Owner.ToHero().Hand.HasCards, DiscardToPlayResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard });
        }

        private IEnumerator DiscardToPlayResponse(PhaseChangeAction pca)
        {
            // "... this hero's player may discard a card to play an Ongoing or Equipment card."

            /*bool shouldDiscard = true;
            // Check if they have a card in hand they can play
            if (!FindCardsWhere(new LinqCardCriteria((Card c) => c.Location == GetCardThisCardIsNextTo().Owner.ToHero().Hand && (IsOngoing(c) || IsEquipment(c))), visibleToCard: GetCardSource()).Any())
            {
                shouldDiscard = false;
                // If not, ask if they want to discard anyway for no effect
                // ...
            }*/
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(GameController.FindTurnTakerController(GetCardThisCardIsNextTo().Owner).ToHero(), optional: true, storedResults: discards, responsibleTurnTaker: GetCardThisCardIsNextTo().Owner, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, 1))
            {
                IEnumerator playCoroutine = SelectAndPlayCardFromHand(GameController.FindTurnTakerController(GetCardThisCardIsNextTo().Owner).ToHero(), cardCriteria: new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "Ongoing or Equipment"));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }
    }
}
