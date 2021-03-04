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
    public class OvercrowdingCardController : TheGoalieUtilityCardController
    {
        public OvercrowdingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => IsGoalposts(c), "goalposts"));
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a Goalposts card enters play, it becomes indestructible until the end of the turn and you may play a Goalposts card from your trash."
            AddTrigger((CardEntersPlayAction cepa) => IsGoalposts(cepa.CardEnteringPlay), GoalpostsEntersResponse, new TriggerType[] { TriggerType.CreateStatusEffect, TriggerType.PlayCard }, TriggerTiming.After, isConditional: false, isActionOptional: false);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 2);
            int damageAmount = GetPowerNumeral(1, 2);
            int goalpostsThreshold = GetPowerNumeral(2, 3);
            // "{TheGoalieCharacter} deals up to 2 targets 2 melee damage each."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), damageAmount, DamageType.Melee, new int?(numTargets), optional: true, new int?(0), stopDealingDamage: () => true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If there are 3 or more Goalposts cards in play, return this card to your hand."
            if (NumGoalpostsInPlay() >= goalpostsThreshold)
            {
                string message = "There are " + NumGoalpostsInPlay().ToString() + " Goalposts cards in play! " + base.Card.Title + " returns itself to " + base.TurnTaker.Name + "'s hand.";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.High, GetCardSource(), showCardSource: true);
                IEnumerator returnCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.HeroTurnTaker.Hand, showMessage: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                    yield return base.GameController.StartCoroutine(returnCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                    base.GameController.ExhaustCoroutine(returnCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator GoalpostsEntersResponse(CardEntersPlayAction cepa)
        {
            // "... it becomes indestructible until the end of the turn..."
            MakeIndestructibleStatusEffect immovableStatus = new MakeIndestructibleStatusEffect();
            immovableStatus.CardsToMakeIndestructible.IsSpecificCard = cepa.CardEnteringPlay;
            immovableStatus.UntilThisTurnIsOver(base.Game);
            IEnumerator immovableCoroutine = base.GameController.AddStatusEffect(immovableStatus, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(immovableCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(immovableCoroutine);
            }
            // "... and you may play a Goalposts card from your trash."
            IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.HeroTurnTakerController, base.FindCardsWhere(new LinqCardCriteria((Card c) => IsGoalposts(c) && c.Location == base.TurnTaker.Trash)), optional: true, isPutIntoPlay: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }
    }
}
