using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class FractureUtilityCardController : CardController
    {
        public FractureUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public LinqCardCriteria BreachCard()
        {
            return new LinqCardCriteria((Card c) => c.DoKeywordsContain("breach"), "Breach");
        }

        public bool IsBreach(Card c)
        {
            return c.DoKeywordsContain("breach");
        }
    }
}
