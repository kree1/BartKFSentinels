using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Victory
{
    public class VictoryUtilityCardController : CardController
    {
        public VictoryUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string MantleKeyword = "mantle";

        public int DamageDealtByVictoryThisTurn()
        {
            List<int> amounts = (from DealDamageJournalEntry ddje in Journal.DealDamageEntriesThisTurn() where ddje.SourceCard == CharacterCard && ddje.Amount > 0 select ddje.Amount).ToList();
            int result = 0;
            foreach (int a in amounts)
            {
                result += a;
            }
            return result;
        }
    }
}
