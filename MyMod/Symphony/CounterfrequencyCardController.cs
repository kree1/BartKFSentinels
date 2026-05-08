using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class CounterfrequencyCardController : CostCardController
    {
        public CounterfrequencyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: has flag been set this round?
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisRound(PreventedThisRound), () => Card.Title + " has already prevented damage this round.", () => Card.Title + " has not yet prevented damage this round.").Condition = () => Card.IsInPlayAndHasGameText;
            // If in play: remind player to click on Symphony's character card to use this card's power
            SpecialStringMaker.ShowSpecialString(() => "Click on " + TurnTaker.Name + "'s hero character card to use this power.").Condition = () => Card.IsInPlayAndHasGameText;
        }

        public readonly string PreventedThisRound = "PreventedThisRound";

        public override void AddTriggers()
        {
            base.AddTriggers();
            AddAsPowerContributor();
            // "Once per round, when a target next to this card would deal damage, you may prevent it and draw 2 cards."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisRound(PreventedThisRound) && GetCardThisCardIsNextTo() != null && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card == GetCardThisCardIsNextTo() && IsRealAction(dda), MayPreventDraw3, new TriggerType[] { TriggerType.CancelAction, TriggerType.DrawCard }, TriggerTiming.Before);
            AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false, false);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(PreventedThisRound), TriggerType.Hidden);
        }

        public IEnumerator MayPreventDraw3(DealDamageAction dda)
        {
            // "... you may prevent it and draw 2 cards."
            YesNoDecision yn = new YesNoDecision(GameController, DecisionMaker, SelectionType.PreventDamage, gameAction: dda, cardSource: GetCardSource());
            IEnumerator decideCoroutine = GameController.MakeDecisionAction(yn);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(decideCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(decideCoroutine);
            }
            if (DidPlayerAnswerYes(yn))
            {
                SetCardPropertyToTrueIfRealAction(PreventedThisRound, gameAction: dda);
                IEnumerator cancelCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(cancelCoroutine);
                }
                if (IsRealAction(dda))
                {
                    IEnumerator drawCoroutine = DrawCards(DecisionMaker, 2);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(drawCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(drawCoroutine);
                    }
                }
            }
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            if (TurnTakerController.CharacterCardControllers.Contains(cardController))
            {
                return new Power[] { new Power(DecisionMaker, cardController, "Move " + Card.Title + " next to a target.", UseGrantedPower(), 0, null, GetCardSource()) };
            }
            else
            {
                return null;
            }
        }

        public IEnumerator UseGrantedPower(int index = 0)
        {
            // "Move this card next to a target."
            return GameController.SelectCardAndDoAction(new SelectCardDecision(GameController, DecisionMaker, SelectionType.MoveCardNextToCard, FindCardsWhere((Card c) => c.IsInPlay && c.IsTarget), cardSource: GetCardSource()), (SelectCardDecision d) => GameController.MoveCard(TurnTakerController, Card, d.SelectedCard.NextToLocation, cardSource: GetCardSource()));
        }
    }
}
