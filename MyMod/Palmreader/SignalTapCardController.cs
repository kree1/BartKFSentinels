using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Palmreader
{
    public class SignalTapCardController : PalmreaderUtilityCardController
    {
        public SignalTapCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            RunModifyDamageAmountSimulationForThisCard = false;
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsRelay(c) && c.IsInPlayAndHasGameText), specifyPlayAreas: true).Condition = () => NumRelaysInPlay() > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Relay cards in play.").Condition = () => NumRelaysInPlay() <= 0;
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
            ShowListOfCardsAtLocationOfCardRecursive(base.Card, new LinqCardCriteria((Card c) => c.IsTarget, "target", useCardsSuffix: false, false, "target", "targets")).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private DealDamageAction MyDamageAction
        {
            get;
            set;
        }

        private SelectFunctionDecision MySelection
        {
            get;
            set;
        }

        private Card MyTarget
        {
            get;
            set;
        }

        private ITrigger _modifyDamageAmount;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When damage would be dealt to or by (PalmreaderCharacter) to or by a target in this play area, you may increase or reduce that damage by 1."
            _modifyDamageAmount = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => (dda.DamageSource != null && dda.DamageSource.IsTarget && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.Target == base.CharacterCard) || (dda.Target.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == base.CharacterCard), ModifyDamageResponse, new TriggerType[] { TriggerType.IncreaseDamage, TriggerType.ReduceDamage }, TriggerTiming.Before, isActionOptional: true);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, move it to a play area with no other Relay cards."
            List<SelectTurnTakerDecision> decisions = new List<SelectTurnTakerDecision>();
            IEnumerator directCoroutine = base.GameController.SelectTurnTaker(base.HeroTurnTakerController, SelectionType.MoveCardToPlayArea, decisions, optional: false, additionalCriteria: (TurnTaker tt) => tt.PlayArea.Cards.Where((Card c) => IsRelay(c) && c != base.Card).Count() == 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(directCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(directCoroutine);
            }
            SelectTurnTakerDecision choice = decisions.FirstOrDefault();
            if (choice != null && choice.SelectedTurnTaker != null)
            {
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, choice.SelectedTurnTaker.PlayArea, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, doesNotEnterPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            // "You may play a Relay card from your trash."
            IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.HeroTurnTakerController, base.FindCardsWhere(new LinqCardCriteria((Card c) => IsRelay(c) && c.Location == base.TurnTaker.Trash)), optional: true, isPutIntoPlay: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "Then, destroy all but 2 Relay cards."
            IEnumerator destroyCoroutine = DestroyExcessRelaysResponse();
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

        public IEnumerator ModifyDamageResponse(DealDamageAction dda)
        {
            // "... you may increase or reduce that damage by 1."
            MyDamageAction = dda;
            if (base.GameController.PretendMode || dda.Target != MyTarget)
            {
                IEnumerable<Function> options = new Function[2]
                {
                    new Function(base.HeroTurnTakerController, "Increase by 1", SelectionType.IncreaseDamage, IncreaseFunction),
                    new Function(base.HeroTurnTakerController, "Reduce by 1", SelectionType.ReduceDamageTaken, ReduceFunction)
                };
                List<SelectFunctionDecision> decisions = new List<SelectFunctionDecision>();
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, dda, cardSource: GetCardSource());
                IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice, decisions);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (decisions.Count() > 0)
                {
                    MySelection = decisions.FirstOrDefault();
                }
                MyTarget = dda.Target;
            }
            else if (MySelection.SelectedFunction != null)
            {
                IEnumerator executeCoroutine = MySelection.SelectedFunction.FunctionToExecute();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(executeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(executeCoroutine);
                }
            }

            if (!base.GameController.PretendMode)
            {
                MySelection = null;
                MyTarget = null;
            }
            yield break;
        }

        private IEnumerator IncreaseFunction()
        {
            IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(MyDamageAction, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }

        private IEnumerator ReduceFunction()
        {
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(MyDamageAction, 1, _modifyDamageAmount, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            yield break;
        }
    }
}
