using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class HarderThanDiamondCardController : StressCardController
    {
        public HarderThanDiamondCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce non-lightning damage dealt to {Dreadnought} by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.Target == CharacterCard && dda.DamageType != DamageType.Lightning, (DealDamageAction dda) => 1);
            // "Whenever a target is reduced from 1 or more to 0 or fewer HP or a non-target environment card is destroyed, {Dreadnought} deals herself 2 irreducible psychic damage unless you put the bottom card of your trash on the bottom of your deck."
            AddTrigger((DealDamageAction dda) => dda.TargetHitPointsBeforeBeingDealtDamage > 0 && dda.TargetHitPointsAfterBeingDealtDamage <= 0, (DealDamageAction dda) => PayStress(1), new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.After);
            AddTrigger((DestroyCardAction dca) => dca.WasCardDestroyed && !dca.CardToDestroy.Card.IsTarget && dca.CardToDestroy.Card.IsEnvironment, (DestroyCardAction dca) => PayStress(1), new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.After);
        }
    }
}
