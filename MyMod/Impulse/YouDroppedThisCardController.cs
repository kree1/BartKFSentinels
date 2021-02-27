using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class YouDroppedThisCardController : ImpulseUtilityCardController
    {
        public const string PoolID = "YouDroppedThisPool";

        public YouDroppedThisCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowTokenPool(YouDroppedThisPool).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When damage that would be dealt to a hero target is prevented or reduced to 0 or less, put a token on this card."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => CheckDamageCriteria(dda), (DealDamageAction dda) => base.GameController.AddTokensToPool(YouDroppedThisPool, 1, GetCardSource()), TriggerType.AddTokensToPool, TriggerTiming.After, isActionOptional: false);
            AddTrigger<CancelAction>((CancelAction ca) => ca.ActionToCancel is DealDamageAction && ca.IsPreventEffect && CheckDamageCriteria(ca.ActionToCancel as DealDamageAction), (CancelAction ca) => base.GameController.AddTokensToPool(YouDroppedThisPool, 1, GetCardSource()), TriggerType.AddTokensToPool, TriggerTiming.After, isActionOptional: false);
        }

        public override IEnumerator Play()
        {
            // Reset the token pool
            YouDroppedThisPool.SetToInitialValue();
            yield break;
        }

        private TokenPool YouDroppedThisPool
        {
            get
            {
                return this.Card.FindTokenPool(PoolID);
            }
        }

        private bool CheckDamageCriteria(DealDamageAction dda)
        {
            if (!dda.IsPretend && dda.Target.IsHero && !dda.DidDealDamage)
            {
                return dda.OriginalAmount > 0;
            }
            return false;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Remove X tokens from this card."
            List<int?> removed = new List<int?>();
            IEnumerator removeCoroutine = RemoveAnyNumberOfTokensFromTokenPool(YouDroppedThisPool, removed);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }

            // "If you do, {ImpulseCharacter} deals 1 target X irreducible melee damage."
            int? numRemoved = removed.FirstOrDefault();
            if (numRemoved.HasValue)
            {
                IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), numRemoved.Value, DamageType.Melee, 1, false, 1, isIrreducible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }
    }
}
