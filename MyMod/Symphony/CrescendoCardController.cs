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
    public class CrescendoCardController : DoubleEdgeCardController
    {
        public CrescendoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 1;
        }

        public override IEnumerator OneShotEffect()
        {
            // "One player may draw a card."
            return GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
        }
    }
}
