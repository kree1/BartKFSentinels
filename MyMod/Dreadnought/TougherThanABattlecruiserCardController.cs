﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class TougherThanABattlecruiserCardController : StressCardController
    {
        public TougherThanABattlecruiserCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce non-lightning damage dealt to {Dreadnought} by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.Target == CharacterCard && dda.DamageType != DamageType.Lightning, (DealDamageAction dda) => 1);
            // "Whenever a target is reduced from 1 or more to 0 or fewer HP or an environment card is destroyed, put the bottom card of your trash on the bottom of your deck. If you can't, {Dreadnought} deals herself 2 irreducible psychic damage."
            AddTrigger((DealDamageAction dda) => dda.TargetHitPointsBeforeBeingDealtDamage > 0 && dda.TargetHitPointsAfterBeingDealtDamage <= 0, (DealDamageAction dda) => StressResponse(1), new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.After);
            AddTrigger((DestroyCardAction dca) => dca.WasCardDestroyed && dca.CardToDestroy.Card.IsEnvironment, (DestroyCardAction dca) => StressResponse(1), new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.After);
        }
    }
}