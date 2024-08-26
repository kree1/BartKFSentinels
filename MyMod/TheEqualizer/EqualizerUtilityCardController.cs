using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class EqualizerUtilityCardController : CardController
    {
        public EqualizerUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public const string MunitionKeyword = "munition";
        public const string CartridgeKeyword = "cartridge";
        public const string ObjectiveIdentifier = "LucrativeContract";
        public const string SalvoName = "salvo";

        public bool IsMarked(Card c)
        {
            if (c.IsInPlayAndHasGameText)
            {
                return c.NextToLocation.Cards.Any((Card x) => x.Identifier == ObjectiveIdentifier);
            }
            return false;
        }

        public Card MarkedTarget(CardSource looking)
        {
            return GameController.FindCardsWhere((Card c) => IsMarked(c), visibleToCard: looking).FirstOrDefault();
        }
    }
}
