using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class TuneUpCardController : BenefitCardController
    {
        public TuneUpCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 5;
        }

        public override IEnumerator OneShotEffect()
        {
            // "Draw 3 cards."
            return GameController.DrawCards(DecisionMaker, 3, cardSource: GetCardSource());
        }
    }
}
