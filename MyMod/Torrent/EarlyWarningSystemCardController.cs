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
    public class EarlyWarningSystemCardController : TorrentUtilityCardController
    {
        public EarlyWarningSystemCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "Whenever a Cluster would be dealt damage, you may redirect that damage to {TorrentCharacter}."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.DoKeywordsContain("cluster"), (DealDamageAction dda) => base.GameController.RedirectDamage(dda, base.CharacterCard, isOptional: true, cardSource: GetCardSource()), new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.RedirectDamage }, TriggerTiming.Before, isActionOptional: true);
            base.AddTriggers();
        }
    }
}
