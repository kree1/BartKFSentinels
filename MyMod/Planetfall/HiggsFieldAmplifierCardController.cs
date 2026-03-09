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
    public class HiggsFieldAmplifierCardController : PlanetfallUtilityCardController
    {
        public HiggsFieldAmplifierCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "If {Planetfall} is Tiny, increase damage dealt by {Planetfall} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard) && GameController.DoesCardContainKeyword(CharacterCard, TinyKeyword), (DealDamageAction dda) => 1);
            // "If {Planetfall} is Huge, reduce damage dealt to {Planetfall} by 1."
            AddReduceDamageTrigger((Card c) => c == CharacterCard && GameController.DoesCardContainKeyword(c, HugeKeyword), 1);
        }
    }
}
