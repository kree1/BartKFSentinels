using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class SunSunCardController : OwnershipUtilityCardController
    {
        public SunSunCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, this card regains {H + 1} HP."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.GainHP(base.Card, H + 1, cardSource: GetCardSource()), TriggerType.GainHP);
            // "When a hero character is dealt 4 or more fire damage, this card deals itself 3 infernal damage."
            AddTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && dda.DamageType == DamageType.Fire && dda.Amount >= 4 && dda.DidDealDamage, (DealDamageAction dda) => DealDamage(base.Card, base.Card, 3, DamageType.Infernal, cardSource: GetCardSource()), TriggerType.DealDamage, TriggerTiming.After);
            // "When another villain Sun card is played, this card deals itself 5 infernal damage."
            AddTrigger((CardEntersPlayAction cepa) => !cepa.IsPutIntoPlay && IsVillain(cepa.CardEnteringPlay) && base.GameController.GetAllKeywords(cepa.CardEnteringPlay).Contains(SunKeyword), (CardEntersPlayAction cepa) => DealDamage(base.Card, base.Card, 5, DamageType.Infernal, cardSource: GetCardSource()), TriggerType.DealDamage, TriggerTiming.After);
        }
    }
}
