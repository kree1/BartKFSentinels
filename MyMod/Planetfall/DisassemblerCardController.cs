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
    public class DisassemblerCardController : DestructiveChipCardController
    {
        public DisassemblerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToDeal = DamageType.Melee;
            Relevant = new LinqCardCriteria((Card c) => IsEquipment(c), "Equipment");
            SpecialStringMaker.ShowHeroWithMostCards(false, Relevant);
        }
    }
}
