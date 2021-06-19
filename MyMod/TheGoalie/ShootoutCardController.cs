using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class ShootoutCardController : TheGoalieUtilityCardController
    {
        public ShootoutCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
		{
			// "Each non-hero target deals {TheGoalieCharacter} 1 projectile damage."
			IEnumerator incomingCoroutine = DealDamage((Card c) => c.IsInPlay && c.IsTarget && !c.IsHero, (Card c) => c == base.CharacterCard, (Card c) => 1, DamageType.Projectile);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(incomingCoroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(incomingCoroutine);
			}
			// "{TheGoalieCharacter} deals each non-hero target 3 projectile damage."
			IEnumerator outgoingCoroutine = DealDamage(base.CharacterCard, (Card c) => c.IsInPlay && c.IsTarget && !c.IsHero, 3, DamageType.Projectile);
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(outgoingCoroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(outgoingCoroutine);
			}
			yield break;
        }
    }
}
