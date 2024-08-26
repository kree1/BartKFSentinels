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
        public const string SalvoName = "salvo";

        public TheEqualizerTurnTakerController ettc => TurnTakerControllerWithoutReplacements as TheEqualizerTurnTakerController;
    }
}
