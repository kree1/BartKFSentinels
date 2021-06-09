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
    public class PersistentModCardController : AttachedBreachCardController
    {
        public PersistentModCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of this play area's turn, {FractureCharacter} may deal herself 2 psychic damage. If she takes no damage this way, destroy this card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, (PhaseChangeAction pca) => DealDamageOrDestroyThisCardResponse(pca, base.CharacterCard, base.CharacterCard, 2, DamageType.Psychic), new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }
    }
}
