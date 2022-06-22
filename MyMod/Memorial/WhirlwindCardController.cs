using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class WhirlwindCardController : RenownCardController
    {
        public WhirlwindCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of this play area's turn, this hero may deal 1 target 2 melee damage."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == Card.Location.HighestRecursiveLocation.OwnerTurnTaker && GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsHeroCharacterCard && GetCardThisCardIsNextTo().IsTarget, (PhaseChangeAction pca) => GameController.SelectTargetsAndDealDamage(GameController.FindTurnTakerController(GetCardThisCardIsNextTo().Owner).ToHero(), new DamageSource(GameController, GetCardThisCardIsNextTo()), 2, DamageType.Melee, 1, false, 0, cardSource: GetCardSource()), TriggerType.DealDamage);
        }
    }
}
