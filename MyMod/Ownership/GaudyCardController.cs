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
    public class GaudyCardController : TeamModCardController
    {
        public GaudyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Ballpark Modification cards in play
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(ModificationKeyword) && base.GameController.GetAllKeywords(c).Contains(BallparkKeyword), "Ballpark Modification"));
        }

        public readonly string BallparkKeyword = "BallparkKeyword";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by hero targets in this play area by X, where X = the number of Ballpark Modification cards in play."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, (DealDamageAction dda) => base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && base.GameController.GetAllKeywords(c).Contains(ModificationKeyword) && base.GameController.GetAllKeywords(c).Contains(BallparkKeyword), "Ballpark Modification", singular: "card in play", plural: "cards in play"), visibleToCard: GetCardSource()).Count());
        }
    }
}
