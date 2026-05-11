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
    public class SymphonyUtilityCardController : CardController
    {
        public SymphonyUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of cards in Symphony's hand
            SpecialStringMaker.ShowNumberOfCardsAtLocation(HeroTurnTaker.Hand, showInEffectsList: () => true);
        }

        public readonly string MeasureKeyword = "measure";
        public readonly string SilenceKeyword = "silence";

        public bool IsMeasure(Card c)
        {
            return GameController.DoesCardContainKeyword(c, MeasureKeyword);
        }

        public bool IsSilence(Card c)
        {
            return GameController.DoesCardContainKeyword(c, SilenceKeyword);
        }
    }
}
