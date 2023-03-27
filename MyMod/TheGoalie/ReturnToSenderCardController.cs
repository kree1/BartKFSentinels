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
    public class ReturnToSenderCardController : TheGoalieUtilityCardController
    {
        public ReturnToSenderCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsInPlay(GoalpostsCards);
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has already reacted to damage this turn.", () => base.Card.Title + " has not yet reacted to damage this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time any hero target is dealt damage each turn, {TheGoalieCharacter} may deal the source of that damage 1 projectile damage. Then, you may destroy a Goalposts card. If you do, redirect damage dealt by that source to {TheGoalieCharacter} until the start of your turn."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && IsHeroTarget(dda.Target) && dda.DidDealDamage, RetaliateRedirectResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard, TriggerType.CreateStatusEffect }, TriggerTiming.After);
        }

        private const string OncePerTurn = "ReactOncePerTurn";

        public IEnumerator RetaliateRedirectResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(OncePerTurn);
            // "... {TheGoalieCharacter} may deal the source of that damage 1 projectile damage."
            IEnumerator responseCoroutine = null;
            if (dda.DamageSource != null && dda.DamageSource.IsTarget)
            {
                responseCoroutine = DealDamage(base.CharacterCard, dda.DamageSource.Card, 1, DamageType.Projectile, optional: true, isCounterDamage: true, cardSource: GetCardSource());
            }
            else
            {
                responseCoroutine = base.GameController.SendMessageAction("Damage was not dealt by a target, so " + base.CharacterCard.Title + " can't retaliate...", Priority.High, GetCardSource(), associatedCards: base.CharacterCard.ToEnumerable());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(responseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(responseCoroutine);
            }
            // "Then, you may destroy a Goalposts card."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, GoalpostsCards, true, storedResultsAction: destroyed, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If you do, redirect damage dealt by that source to {TheGoalieCharacter} until the start of your turn."
            if (destroyed.FirstOrDefault() != null && destroyed.FirstOrDefault().WasCardDestroyed && dda.DamageSource != null && dda.DamageSource.IsCard)
            {
                RedirectDamageStatusEffect focus = new RedirectDamageStatusEffect();
                focus.RedirectTarget = base.CharacterCard;
                focus.SourceCriteria.IsSpecificCard = dda.DamageSource.Card;
                focus.TargetCriteria.IsNotSpecificCard = base.CharacterCard;
                focus.UntilStartOfNextTurn(base.TurnTaker);
                focus.UntilCardLeavesPlay(dda.DamageSource.Card);
                focus.TargetLeavesPlayExpiryCriteria.Card = base.CharacterCard;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(focus, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            yield break;
        }
    }
}
