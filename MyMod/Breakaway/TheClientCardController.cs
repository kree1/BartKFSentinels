using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    public class TheClientCardController : CardController
    {
        public TheClientCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has redirected damage this turn.", () => base.Card.Title + " has not redirected damage this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, if {Breakaway} has less than 25 HP, {TheClient} skips town! Remove this card from the game."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, LowHPCheck, TriggerType.RemoveFromGame);
            // "At the start of the villain turn, if {Breakaway} has more than 40 HP, {Breakaway} hands off the loot! [b]GAME OVER.[/b]"
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HighHPCheck, TriggerType.GameOver);
            // "The first time this card would be dealt damage each turn, redirect that damage to {Momentum}."
            this.RedirectDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Target == this.Card && dda.Amount > 0, this.RedirectToMomentum, TriggerType.RedirectDamage, TriggerTiming.Before);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        private string LootObtainedResultText
        {
            get
            {
                return base.TurnTaker.FindCard("BreakawayCharacter").Title + " made it to 40 HP! He hands off the loot to " + this.Card.Title + " and the heroes lose!";
            }
        }

        protected const string OncePerTurn = "RedirectOncePerTurn";
        private ITrigger RedirectDamageTrigger;

        private IEnumerator HighHPCheck(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, if {Breakaway} has more than 40 HP, ..."
            Card breakawayCard = base.TurnTaker.FindCard("BreakawayCharacter");
            if (breakawayCard.HitPoints > 40)
            {
                // "... {Breakaway} hands off the loot! [b]GAME OVER.[/b]"
                IEnumerator handOffCoroutine = GameController.GameOver(EndingResult.AlternateDefeat, LootObtainedResultText, showEndingTextAsMessage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(handOffCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(handOffCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator LowHPCheck(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, if {Breakaway} has less than 25 HP..."
            Card breakawayCard = base.TurnTaker.FindCard("BreakawayCharacter");
            if (breakawayCard.HitPoints < 25)
            {
                // "... {TheClient} skips town! Remove this card from the game."
                string clientDecision = this.Card.Title + " sees that the heroes are too close to catching " + breakawayCard.Title + " (" + breakawayCard.HitPoints.ToString() + " HP), and decides to cut their losses...";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(clientDecision, Priority.High, cardSource: GetCardSource(), showCardSource: true);
                IEnumerator removeCoroutine = base.GameController.MoveCard(this.DecisionMaker, this.Card, base.TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(messageCoroutine);
                    yield return this.GameController.StartCoroutine(removeCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(messageCoroutine);
                    this.GameController.ExhaustCoroutine(removeCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator RedirectToMomentum(DealDamageAction dda)
        {
            // "The first time this card would be dealt damage each turn, redirect that damage to {Momentum}."
            Card momentumCard = base.TurnTaker.FindCard("MomentumCharacter");
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, momentumCard, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            yield break;
        }
    }
}
