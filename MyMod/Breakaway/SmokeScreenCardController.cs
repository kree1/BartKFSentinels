using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    public class SmokeScreenCardController : CardController
    {
        public SmokeScreenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // TODO: find out how to get the name of the active player
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(PlayedThisTurn), () => Game.ActiveTurnTaker.ToHero().ToString() + " has played a card this turn", () => "[player name] has not played a card this turn");
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(PowerThisTurn), () => Game.ActiveTurnTaker.ToHero().ToString() + " has used a power this turn", () => "[player name] has not used a power this turn");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of each hero turn, if that player played a card and used a power this turn, {Momentum} regains 2 HP."
            base.AddTrigger<PlayCardAction>((PlayCardAction act) => act.WasCardPlayed && !act.IsPutIntoPlay && act.CardToPlay.IsHero && act.TurnTakerController.IsHero && act.TurnTakerController.IsActiveTurnTakerController, HeroPlayedResponse, TriggerType.FirstTrigger, TriggerTiming.After);
            base.AddTrigger<UsePowerAction>((UsePowerAction act) => act.IsSuccessful && act.HeroUsingPower != null && act.HeroUsingPower == base.GameController.ActiveTurnTakerController, HeroUsedPowerResponse, TriggerType.FirstTrigger, TriggerTiming.After);
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt.IsHero && HasBeenSetToTrueThisTurn(PlayedThisTurn) && HasBeenSetToTrueThisTurn(PowerThisTurn), (PhaseChangeAction action) => base.GameController.GainHP(base.TurnTaker.FindCard("MomentumCharacter"), 2, cardSource: GetCardSource()), TriggerType.GainHP);
            // "When {Momentum} flips, each player discards 1 card. Then, destroy this card."
            AddTrigger((FlipCardAction fca) => fca.CardToFlip.Card == base.TurnTaker.FindCard("MomentumCharacter"), SelfDestructResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroySelf }, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, {Breakaway} regains 1 HP."
            IEnumerator healBreakawayCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("BreakawayCharacter"), 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(healBreakawayCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(healBreakawayCoroutine);
            }

            yield break;
        }

        protected const string PlayedThisTurn = "HeroPlayedCardThisTurn";
        protected const string PowerThisTurn = "HeroUsedPowerThisTurn";
        private ITrigger heroPlayedTrigger;
        private ITrigger heroUsedPowerTrigger;

        private IEnumerator SelfDestructResponse(FlipCardAction fca)
        {
            // "When {Momentum} flips, each player discards 1 card."
            IEnumerator discardCoroutine = base.GameController.EachPlayerDiscardsCards(1, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(discardCoroutine);
            }

            // "Then, destroy this card."
            IEnumerator destroyCoroutine = base.DestroyThisCardResponse(fca);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        private IEnumerator HeroPlayedResponse(PlayCardAction pca)
        {
            // Note when a player plays a card during their turn
            base.SetCardPropertyToTrueIfRealAction(PlayedThisTurn);
            yield break;
        }

        private IEnumerator HeroUsedPowerResponse(UsePowerAction upa)
        {
            // Note when a hero uses a power during their turn
            base.SetCardPropertyToTrueIfRealAction(PowerThisTurn);
            yield break;
        }
    }
}
