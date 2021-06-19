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
    public class AirPocketCardController : ImpulseUtilityCardController
    {
        public AirPocketCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When that target would be dealt damage, prevent that damage and destroy this card."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => !dda.IsPretend && dda.Target == GetCardThisCardIsNextTo() && dda.Amount > 0 && !base.Card.IsBeingDestroyed, DealtDamageResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.WouldBeDealtDamage }, TriggerTiming.Before);
            // "When this card is destroyed, you may draw a card or use a power."
            AddWhenDestroyedTrigger(OnDestroyResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.UsePower });
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "When this card enters play, move it next to a target."
            IEnumerator moveCoroutine = SelectCardThisCardWillMoveNextTo(new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText, "targets", useCardsSuffix: false), storedResults, isPutIntoPlay, decisionSources);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield break;
        }

        public IEnumerator DealtDamageResponse(DealDamageAction dda)
        {
            // "When that target would be dealt damage, prevent that damage and destroy this card."
            IEnumerator preventCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, actionSource: dda, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        public IEnumerator OnDestroyResponse(DestroyCardAction dca)
        {
            // "When this card is destroyed, you may draw a card or use a power."
            List<Function> options = new List<Function>();
            Function drawOption = new Function(base.DecisionMaker, "Draw a card", SelectionType.DrawCard, () => base.GameController.DrawCard(base.HeroTurnTaker, cardSource: GetCardSource()), null, null, "Draw a card");
            options.Add(drawOption);
            Function powerOption = new Function(base.DecisionMaker, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(base.HeroTurnTakerController, optional: true, null, 1, cardSource: GetCardSource()), null, null, "Use a power");
            options.Add(powerOption);
            if (options.Count > 0)
            {
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
            }
            yield break;
        }
    }
}
