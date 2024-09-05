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
    public class FasterThanAJetCardController : DreadnoughtUtilityCardController
    {
        public FasterThanAJetCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show total damage dealt by Dreadnought this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => DamageDealtByDreadnoughtThisTurn() > 0, () => CharacterCard.Title + " has dealt " + DamageDealtByDreadnoughtThisTurn().ToString() + " damage this turn.", () => CharacterCard.Title + " has not dealt damage this turn.");
            // If in play: show whether this card has reacted this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(FirstDamageThisTurn, Card.Title + " has already reacted to damage this turn.", Card.Title + " has not reacted to damage this turn.").Condition = () => Card.IsInPlayAndHasGameText;
        }

        private readonly string FirstDamageThisTurn = "FirstDamageThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may play a card or use a power. Then, if {Dreadnought} dealt 10 or more damage this turn, destroy an environment card and play the top card of the environment deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, BonusActionResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.UsePower, TriggerType.DestroyCard });
            // "The first time {Dreadnought} is dealt damage by any non-hero target each turn, she may deal that target 1 melee damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(FirstDamageThisTurn) && dda.Target == CharacterCard && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.IsTarget && !IsHeroTarget(dda.DamageSource.Card), HitBackResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(FirstDamageThisTurn), TriggerType.Hidden);
        }

        public IEnumerator BonusActionResponse(PhaseChangeAction pca)
        {
            // "... you may play a card or use a power."
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardSource: GetCardSource()), HeroTurnTaker.HasCardsInHand && CanPlayCards(TurnTakerController), repeatDecisionText: "play a card"));
            options.Add(new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => GameController.SelectAndUsePowerEx(DecisionMaker, true, cardSource: GetCardSource()), GameController.CanUsePowers(DecisionMaker, GetCardSource()), repeatDecisionText: "use a power"));
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
            // "Then, if {Dreadnought} dealt 10 or more damage this turn, ..."
            if (DamageDealtByDreadnoughtThisTurn() >= 10)
            {
                // "... destroy an environment card ..."
                IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: Card, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(destroyCoroutine);
                }
                // "... and play the top card of the environment deck."
                IEnumerator playEnvCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(null);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(playEnvCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(playEnvCoroutine);
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
