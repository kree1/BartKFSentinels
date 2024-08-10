using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class LightHandedCardController : TeamModCardController
    {
        public LightHandedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show number of Equipment and non-character target cards in this play area
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.Card.Location.HighestRecursiveLocation, new LinqCardCriteria((Card c) => IsEquipment(c) || (!c.IsCharacter && c.IsTarget), "Equipment and non-character target"), recursive: true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of this player's turn, remove X counters from their Stat card, where X = the number of non-character target and Equipment cards in this play area."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.OwnerTurnTaker && RelevantStatCard() != null, (PhaseChangeAction pca) => base.GameController.RemoveTokensFromPool(RelevantStatCard().FindTokenPool(WeightPoolIdentifier), base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && (IsEquipment(c) || (!c.IsCharacter && c.IsTarget)), "in " + base.Card.Location.HighestRecursiveLocation.GetFriendlyName(), useCardsPrefix: true, useCardsSuffix: false, singular: "Equipment or non-character target card", plural: "Equipment or non-character target cards"), visibleToCard: GetCardSource()).Count(), cardSource: GetCardSource()), TriggerType.AddTokensToPool);
        }
    }
}
