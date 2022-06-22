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
    public class GeniusCardController : RenownCardController
    {
        public GeniusCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of this play area's turn, this hero's player may draw a card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == Card.Location.HighestRecursiveLocation.OwnerTurnTaker, (PhaseChangeAction pca) => DrawCard(GetCardThisCardIsNextTo().Owner.ToHero(), optional: true), TriggerType.DrawCard);
        }
    }
}
