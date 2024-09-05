using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class DreadnoughtUtilityCardController : CardController
    {
        public DreadnoughtUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string MantleKeyword = "mantle";

        public int DamageDealtByDreadnoughtThisTurn()
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
