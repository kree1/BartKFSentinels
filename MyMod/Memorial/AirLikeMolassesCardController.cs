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
    public class AirLikeMolassesCardController : CardController
    {
        public AirLikeMolassesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override IEnumerator Play()
        {
            // "{Memorial} deals the hero target with the highest HP {H} energy damage."
            IEnumerator damageCoroutine = base.DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero, (Card c) => H, DamageType.Energy, numberOfTargets: () => 1, addStatusEffect: FreezeTimeResponse, selectTargetEvenIfCannotDealDamage: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }

        public IEnumerator FreezeTimeResponse(DealDamageAction dda)
        {
            // "Until the start of the villain turn, reduce damage dealt by that target by 1..."
            ReduceDamageStatusEffect reduceEffect = new ReduceDamageStatusEffect(1);
            reduceEffect.SourceCriteria.IsSpecificCard = dda.Target;
            reduceEffect.UntilStartOfNextTurn(TurnTaker);
            reduceEffect.UntilTargetLeavesPlay(dda.Target);
            IEnumerator reduceEffectCoroutine = AddStatusEffect(reduceEffect);

            // "... and increase damage dealt to that target by 1."
            IncreaseDamageStatusEffect increaseEffect = new IncreaseDamageStatusEffect(1);
            increaseEffect.TargetCriteria.IsSpecificCard = dda.Target;
            increaseEffect.UntilStartOfNextTurn(TurnTaker);
            increaseEffect.UntilTargetLeavesPlay(dda.Target);
            IEnumerator increaseEffectCoroutine = AddStatusEffect(increaseEffect);

            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceEffectCoroutine);
                yield return base.GameController.StartCoroutine(increaseEffectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceEffectCoroutine);
                base.GameController.ExhaustCoroutine(increaseEffectCoroutine);
            }
        }
    }
}
