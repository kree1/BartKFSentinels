using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class PlanetfallUtilityCardController : CardController
    {
        public PlanetfallUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string HugeKeyword = "huge";
        public readonly string TinyKeyword = "tiny";
        public readonly string ChipKeyword = "chip";

        public LinqCardCriteria ChipCriteria()
        {
            return new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, ChipKeyword), "chip");
        }

        public LinqCardCriteria ChipInTrashCriteria()
        {
            return new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, ChipKeyword) && c.Location == TurnTaker.Trash, "chip", singular: "card in the villain trash", plural: "cards in the villain trash");
        }
    }
}
