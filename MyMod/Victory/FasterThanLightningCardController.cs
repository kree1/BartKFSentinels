﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Victory
{
    public class FasterThanLightningCardController : StressCardController
    {
        public FasterThanLightningCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether this card has reacted this turn
            SpecialStringMaker.ShowHasBeenUsedThisTurn(RespondedThisTurn, Card.Title + " has already reacted to damage this turn.", Card.Title + " has not reacted to damage this turn.").Condition = () => Card.IsInPlayAndHasGameText;
        }

        private readonly string RespondedThisTurn = "RespondedThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may play a card or use a power. If you do, {Victory} deals herself 2 irreducible psychic damage unless you put the bottom card of your trash on the bottom of your deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, BonusActionResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.UsePower, TriggerType.DestroyCard });
            // "Once per turn, when {Victory} is dealt damage by a non-hero target, she may deal that target 1 melee damage."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(RespondedThisTurn) && dda.Target == CharacterCard && dda.DidDealDamage && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.IsTarget && !IsHeroTarget(dda.DamageSource.Card), HitBackResponse, TriggerType.DealDamage, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(RespondedThisTurn), TriggerType.Hidden);
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
                // "... {Victory} deals herself 2 irreducible psychic damage unless you put the bottom card of your trash on the bottom of your deck."
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
            // "... she may deal that target 1 melee damage."
            List<DealDamageAction> results = new List<DealDamageAction>();
            IEnumerator meleeCoroutine = DealDamage(CharacterCard, dda.DamageSource.Card, 1, DamageType.Melee, optional: true, isCounterDamage: true, storedResults: results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            if (DidDealDamage(results, fromDamageSource: CharacterCard))
            {
                SetCardPropertyToTrueIfRealAction(RespondedThisTurn);
            }
        }
    }
}
