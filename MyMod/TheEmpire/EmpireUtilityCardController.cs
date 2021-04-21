using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class EmpireUtilityCardController : CardController
    {
        public EmpireUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public const string TimelineIdentifier = "RevisionistHistory";
        public const string DivergenceKeyword = "divergence";
        public const string AuthorityKeyword = "imperial";
        public const string AllyKeyword = "dissenter";
    }
}
