﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class PitchingMachineCardController : BatterCardController
    {
        public PitchingMachineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }
    }
}
