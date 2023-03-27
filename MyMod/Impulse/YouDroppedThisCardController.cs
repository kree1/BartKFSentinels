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
            // When leaves play: reset token pool
            AddWhenDestroyedTrigger((DestroyCardAction dca) => ResetTokenValue(), TriggerType.Hidden);
            AddTrigger((MoveCardAction mca) => mca.CardToMove == base.Card && mca.Origin.IsInPlayAndNotUnderCard && !mca.Destination.IsInPlayAndNotUnderCard, (MoveCardAction mca) => ResetTokenValue(), TriggerType.Hidden, TriggerTiming.After, outOfPlayTrigger: true);
        }

        public IEnumerator ResetTokenValue()
        {
            // Reset the token pool
            YouDroppedThisPool.SetToInitialValue();
            yield return null;
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
            if (IsHeroTarget(dda.Target))
            {
                return WasDamageToTargetAvoided(dda);
            }
            else if (IsHeroTarget(dda.OriginalTarget))
            {
                return WasDamageToTargetAvoided(dda, dda.OriginalTarget);
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
            if (!numRemoved.HasValue)
            {
                numRemoved = 0;
            }
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
    }
}
