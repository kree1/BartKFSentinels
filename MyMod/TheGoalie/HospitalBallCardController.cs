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
    public class HospitalBallCardController : TheGoalieUtilityCardController
    {
        public HospitalBallCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{TheGoalieCharacter} deals 1 target 4 projectile damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 4, DamageType.Melee, new int?(1), false, new int?(1), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "The next damage dealt to {TheGoalieCharacter} by a non-hero target is irreducible."
            MakeDamageIrreducibleStatusEffect hospitalStatus = new MakeDamageIrreducibleStatusEffect();
            hospitalStatus.TargetCriteria.IsSpecificCard = base.CharacterCard;
            hospitalStatus.SourceCriteria.IsTarget = true;
            hospitalStatus.SourceCriteria.IsHero = false;
            hospitalStatus.NumberOfUses = 1;
            hospitalStatus.UntilTargetLeavesPlay(base.CharacterCard);
            IEnumerator statusCoroutine = AddStatusEffect(hospitalStatus);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }
    }
}
