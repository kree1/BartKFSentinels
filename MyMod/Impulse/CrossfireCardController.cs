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
    public class CrossfireCardController : ImpulseUtilityCardController
    {
        public CrossfireCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever damage that would be dealt to {ImpulseCharacter} is prevented or reduced to 0 or less, the source of that damage deals 1 non-{ImpulseCharacter} target 1 damage of that type."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => CheckDamageCriteria(dda), DealDamageElsewhere, TriggerType.DealDamage, TriggerTiming.After, isActionOptional: false);
            AddTrigger<CancelAction>((CancelAction ca) => ca.ActionToCancel is DealDamageAction && ca.IsPreventEffect && CheckDamageCriteria(ca.ActionToCancel as DealDamageAction), (CancelAction ca) => DealDamageElsewhere(ca.ActionToCancel as DealDamageAction), TriggerType.DealDamage, TriggerTiming.After, isActionOptional: false);
        }

        private bool CheckDamageCriteria(DealDamageAction dda)
        {
            if (!dda.IsPretend && dda.Target == base.CharacterCard && !dda.DidDealDamage)
            {
                return dda.OriginalAmount > 0;
            }
            return false;
        }

        public IEnumerator DealDamageElsewhere(DealDamageAction dda)
        {
            // "... the source of that damage deals 1 non-{ImpulseCharacter} target 1 damage of that type."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, dda.DamageSource, 1, dda.DamageType, 1, false, 1, additionalCriteria: (Card c) => c != base.CharacterCard, cardSource: GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            // "Choose a target. Prevent the next damage that target would deal to {ImpulseCharacter}. Draw a card."
            List<SelectCardDecision> targetChoices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.SelectTarget, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && GameController.IsCardVisibleToCardSource(c, GetCardSource())), targetChoices, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }

            if (targetChoices != null && targetChoices.Count > 0)
            {
                Card targetChosen = targetChoices.FirstOrDefault().SelectedCard;
                OnDealDamageStatusEffect preventNextDamage = new OnDealDamageStatusEffect(base.Card, "PreventDamage", "Prevent the next damage " + targetChosen.Title + " would deal to " + base.CharacterCard.Title + ".", new TriggerType[] { TriggerType.CancelAction }, base.TurnTaker, base.Card);
                preventNextDamage.SourceCriteria.IsSpecificCard = targetChosen;
                preventNextDamage.TargetCriteria.IsSpecificCard = base.CharacterCard;
                preventNextDamage.DamageAmountCriteria.GreaterThan = 0;
                preventNextDamage.NumberOfUses = 1;
                preventNextDamage.UntilCardLeavesPlay(targetChosen);
                preventNextDamage.UntilCardLeavesPlay(base.CharacterCard);
                preventNextDamage.CanEffectStack = true;

                IEnumerator statusCoroutine = base.AddStatusEffect(preventNextDamage);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }

            IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            yield break;
        }
    }
}
