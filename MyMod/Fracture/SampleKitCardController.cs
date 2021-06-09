using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class SampleKitCardController : FractureUtilityCardController
    {
        public SampleKitCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever {FractureCharacter} would deal damage, you may change the type of that damage to a type of your choice."
            AddTrigger(new ChangeDamageTypeTrigger(base.GameController, (DealDamageAction dd) => dd.DamageSource.IsSameCard(base.CharacterCard), base.SelectDamageTypeForDealDamageAction, new TriggerType[1] { TriggerType.ChangeDamageType }, null, GetCardSource()));
            // "Whenever you play a Breach card, you may draw a card."
            AddTrigger((PlayCardAction pca) => IsBreach(pca.CardToPlay) && !pca.IsPutIntoPlay && pca.ResponsibleTurnTaker == base.TurnTaker, (PlayCardAction pca) => base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource()), TriggerType.DrawCard, TriggerTiming.After);
        }
    }
}
