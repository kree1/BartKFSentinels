using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class ClusterCardController : TorrentUtilityCardController
    {
        public ClusterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => base.NumTargetsDestroyedThisTurn() == 1, () => "1 target has been destroyed this turn.", () => base.NumTargetsDestroyedThisTurn().ToString() + " targets have been destroyed this turn.", () => true);
        }

        public const string IgnoreEntersPlay = "IgnoreEntersPlayEffects";
    }
}
