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
    public class SupernovaEclipseCardController : ExpansionWeatherCardController
    {
        public SupernovaEclipseCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play during a player's turn, {OwnershipCharacter} deals each hero target in that player's play area 2 fire damage. Increase damage dealt to character cards this way by 2."
            if (base.Game.ActiveTurnTaker.IsPlayer)
            {
                AddToTemporaryTriggerList(AddIncreaseDamageTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.Identifier == OwnershipIdentifier && dda.CardSource.Card == base.Card, 2));
                IEnumerator fireCoroutine = base.GameController.DealDamage(DecisionMaker, FindCard(OwnershipIdentifier), (Card c) => IsHeroTarget(c) && c.Location.IsPlayAreaOf(base.Game.ActiveTurnTaker), 2, DamageType.Fire, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(fireCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(fireCoroutine);
                }
                RemoveTemporaryTriggers();
            }
        }
    }
}