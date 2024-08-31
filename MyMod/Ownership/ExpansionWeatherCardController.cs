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
    public class ExpansionWeatherCardController : OwnershipUtilityCardController
    {
        public ExpansionWeatherCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of each turn, destroy this card unless it entered play this turn."
            AddStartOfTurnTrigger((TurnTaker tt) => !base.Game.Journal.PlayCardEntriesThisTurn().Any((PlayCardJournalEntry pcje) => pcje.CardPlayed == base.Card), DestroyThisCardResponse, TriggerType.DestroySelf);
        }
    }
}
