using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class FasterThanAJetCardController : StressCardController
    {
        public FasterThanAJetCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether this card has reacted this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, Card.Title + " has already reacted to damage this turn.", Card.Title + " has not reacted to damage this turn.").Condition = () => Card.IsInPlayAndHasGameText;
        }

        private readonly string FirstDamageThisTurn = "FirstDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may play a card or use a power. If you do, {Dreadnought} deals herself 2 irreducible psychic damage unless you put the bottom card of your trash on the bottom of your deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, BonusActionResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.UsePower, TriggerType.DestroyCard });
            // "The first time {Dreadnought} is dealt damage by any non-hero target each turn, she may deal that target 1 melee damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.Target == CharacterCard && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.IsTarget && !IsHeroTarget(dda.DamageSource.Card), HitBackResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageThisTurn), TriggerType.Hidden);
        }

        public IEnumerator BonusActionResponse(PhaseChangeAction pca)
        {
            // "... you may play a card or use a power."
            List<Function> options = new List<Function>();
            List<PlayCardAction> playResults = new List<PlayCardAction>();
            List<UsePowerDecision> powerResults = new List<UsePowerDecision>();
            options.Add(new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => GameController.SelectAndPlayCardFromHand(DecisionMaker, true, storedResults: playResults, cardSource: GetCardSource()), HeroTurnTaker.HasCardsInHand && CanPlayCards(TurnTakerController), repeatDecisionText: "play a card"));
            options.Add(new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => GameController.SelectAndUsePowerEx(DecisionMaker, true, storedResults: powerResults, cardSource: GetCardSource()), GameController.CanUsePowers(DecisionMaker, GetCardSource()), repeatDecisionText: "use a power"));
            SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, true, noSelectableFunctionMessage: TurnTaker.Name + " does not have any cards that can be played or any powers that can be used.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = GameController.SelectAndPerformFunction(choice);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chooseCoroutine);
            }
            // "If you do, ..."
            if (DidPlayCards(playResults) || WasPowerUsed(powerResults))
            {
                // "... {Dreadnought} deals herself 2 irreducible psychic damage unless you put the bottom card of your trash on the bottom of your deck."
                IEnumerator destroyCoroutine = PayStress(1);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }

        public IEnumerator HitBackResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(FirstDamageThisTurn);
            // "... she may deal that target 1 melee damage."
            IEnumerator meleeCoroutine = DealDamage(CharacterCard, dda.DamageSource.Card, 1, DamageType.Melee, optional: true, isCounterDamage: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
        }
    }
}
