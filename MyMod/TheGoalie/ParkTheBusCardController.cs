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
    public class ParkTheBusCardController : TheGoalieUtilityCardController
    {
        public ParkTheBusCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsInPlay(GoalpostsCards);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "As long as a Goalposts card is in play, {TheGoalieCharacter} is immune to damage and you may not play cards or use powers."
            AddImmuneToDamageTrigger((DealDamageAction dda) => base.GameController.FindCardsWhere(GoalpostsInPlay).Any() && dda.Target == base.CharacterCard);
            CannotPlayCards((TurnTakerController ttc) => base.GameController.FindCardsWhere(GoalpostsInPlay).Any() && ttc == base.TurnTakerController);
            CannotUsePowers((TurnTakerController ttc) => base.GameController.FindCardsWhere(GoalpostsInPlay).Any() && ttc == base.TurnTakerController);
            // Display messages to this effect when the value of "a Goalposts card is in play" changes, or when this card leaves play while that value is True
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => IsGoalposts(cepa.CardEnteringPlay) && base.GameController.FindCardsWhere(GoalpostsInPlay).Count() == 1, GoalpostsEntersPlayResponse, TriggerType.ShowMessage, TriggerTiming.After);
            AddTrigger<MoveCardAction>((MoveCardAction mca) => IsGoalposts(mca.CardToMove) && mca.Origin.IsInPlayAndNotUnderCard && !mca.Destination.IsInPlayAndNotUnderCard && base.GameController.FindCardsWhere(GoalpostsInPlay).Count() <= 0, GoalpostsLeavesPlayResponse, TriggerType.ShowMessage, TriggerTiming.After);
            AddTrigger<BulkMoveCardsAction>((BulkMoveCardsAction bmca) => bmca.CardsToMove.Where((Card c) => IsGoalposts(c) && c.IsInPlayAndNotUnderCard).Any() && bmca.CardsToMove.Where((Card c) => IsGoalposts(c) && c.IsInPlayAndNotUnderCard).Count() >= base.GameController.FindCardsWhere(GoalpostsInPlay).Count() && !bmca.Destination.IsInPlayAndNotUnderCard, GoalpostsLeavesPlayResponse, TriggerType.ShowMessage, TriggerTiming.After);
            AddWhenDestroyedTrigger(LeavesPlayResponse, TriggerType.ShowMessage);
            // "At the start of your turn, you may destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, YouMayDestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public override IEnumerator Play()
        {
            // If a Goalposts card is already in play, notify the player
            IEnumerable<Card> goalposts = base.GameController.FindCardsWhere(GoalpostsInPlay);
            if (goalposts.Any())
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.CharacterCard.Title + " parks it in the Goalposts, becoming immune to damage but unable to play cards or use powers.", Priority.High, GetCardSource(), associatedCards: goalposts, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator GoalpostsEntersPlayResponse(CardEntersPlayAction cepa)
        {
            return base.GameController.SendMessageAction(base.CharacterCard.Title + " retreats to the Goalposts, becoming immune to damage but unable to play cards or use powers...", Priority.High, GetCardSource(), associatedCards: cepa.CardEnteringPlay.ToEnumerable(), showCardSource: true);
        }

        public IEnumerator GoalpostsLeavesPlayResponse(GameAction ga)
        {
            IEnumerable<Card> associated = null;
            if (ga is MoveCardAction)
            {
                associated = (ga as MoveCardAction).CardToMove.ToEnumerable();
            }
            else if (ga is BulkMoveCardsAction)
            {
                associated = (ga as BulkMoveCardsAction).CardsToMove.Where((Card c) => IsGoalposts(c));
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.CharacterCard.Title + " emerges from the Goalposts, losing immunity to damage but regaining the ability to play cards and use powers.", Priority.High, GetCardSource(), associatedCards: associated, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            yield break;
        }

        public IEnumerator LeavesPlayResponse(GameAction ga)
        {
            // If a Goalposts card is still in play, notify the player
            IEnumerable<Card> goalposts = base.GameController.FindCardsWhere(GoalpostsInPlay);
            if (goalposts.Any())
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.CharacterCard.Title + " drops the defensive stance!\nThough still empowered within the Goalposts, she loses immunity to damage but regains the ability to play cards and use powers.", Priority.High, GetCardSource(), associatedCards: goalposts, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }
    }
}
