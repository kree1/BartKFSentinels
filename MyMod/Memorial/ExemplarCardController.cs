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
    public class ExemplarCardController : RenownCardController
    {
        public ExemplarCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of this play area's turn, this hero may use a power."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker && GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsHeroCharacterCard, (PhaseChangeAction pca) => SelectAndUsePower(FindCardController(GetCardThisCardIsNextTo())), TriggerType.UsePower);
        }
    }
}
