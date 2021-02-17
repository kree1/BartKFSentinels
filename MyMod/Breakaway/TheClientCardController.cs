using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    class TheClientCardController : CardController
    {
        public TheClientCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => "The Client has redirected damage this turn", () => "The Client has not redirected damage this turn");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, if {Breakaway} has less than 25 HP, {TheClient} skips town! Remove this card from the game."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && base.TurnTaker.FindCard("Breakaway").HitPoints < 25, (PhaseChangeAction pca) => base.GameController.MoveCard(base.TurnTakerController, Card, base.TurnTaker.OutOfGame, cardSource: GetCardSource()), TriggerType.RemoveFromGame);
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
                return "{Breakaway} evaded the heroes long enough to hand off the loot to {TheClient}! [b]GAME OVER.[/b]";
            }
        }

        protected const string OncePerTurn = "RedirectOncePerTurn";
        private ITrigger RedirectDamageTrigger;

        private IEnumerator HighHPCheck(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, if {Breakaway} has more than 40 HP, ..."
            Card breakawayCard = base.TurnTaker.FindCard("Breakaway");
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

        private IEnumerator RedirectToMomentum(DealDamageAction dda)
        {
            // "The first time this card would be dealt damage each turn, redirect that damage to {Momentum}."
            Card momentumCard = base.TurnTaker.FindCard("Momentum");
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
