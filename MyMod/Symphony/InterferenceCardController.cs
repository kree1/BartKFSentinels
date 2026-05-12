using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class InterferenceCardController : DoubleEdgeCardController
    {
        public InterferenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 2;
        }

        public override IEnumerator OneShotEffect()
        {
            // "{Symphony} deals 1 target 1 sonic damage. Until the start of your turn, reduce damage dealt by that target by 1."
            return GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 1, DamageType.Sonic, 1, false, null, addStatusEffect: (DealDamageAction dda) => ReduceDamageDealtByThatTargetUntilTheStartOfYourNextTurnResponse(dda, 1), selectTargetsEvenIfCannotDealDamage: true, cardSource: GetCardSource());
        }
    }
}
