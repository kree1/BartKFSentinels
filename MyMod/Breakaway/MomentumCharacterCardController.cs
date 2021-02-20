using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    public class MomentumCharacterCardController : VillainCharacterCardController
    {
        public MomentumCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Both sides: has Momentum flipped this turn?
            SpecialStringMaker.ShowIfElseSpecialString(() => Journal.WasCardFlippedThisTurn(base.Card), () => base.Card.Title + " has already flipped this turn", () => base.Card.Title + " has not flipped this turn");
            // Front side: 2 hero targets with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1, 2).Condition = () => !base.Card.IsFlipped;
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if(!base.Card.IsFlipped)
            {
                // Front side: Under Pressure
                // "At the start of the villain turn, if this card has more than {H + 2} HP, flip it. If this card did not flip this turn, return {H - 2} hero cards in play to their players' hands."
                base.AddSideTrigger(base.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, UnderPressureStartResponse, new TriggerType[] { TriggerType.FlipCard, TriggerType.MoveCard }));
                // "At the end of the villain turn, restore this card to its maximum HP. Then, {Breakaway} deals himself and the 2 hero targets with the highest HP 2 melee damage each. Remove 5 HP from Breakaway unless he was dealt damage this way."
                base.AddSideTrigger(base.AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, UnderPressureEndResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.Other }));
            }
            else
            {
                // Back side: Gaining Ground
                // "Increase {Breakaway}'s HP recovery by 1."
                base.AddSideTrigger(base.AddTrigger<GainHPAction>((GainHPAction gha) => gha.HpGainer == base.TurnTaker.FindCard("BreakawayCharacter"), (GainHPAction gha) => base.GameController.IncreaseHPGain(gha, 1, cardSource: GetCardSource()), TriggerType.IncreaseHPGain, TriggerTiming.Before));
                // "At the start of the villain turn, if this card has less than {H + 2} HP, flip it. If this card did not flip this turn, {Breakaway} regains 1 HP."
                base.AddSideTrigger(base.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, GainingGroundStartResponse, new TriggerType[] { TriggerType.FlipCard, TriggerType.GainHP }));
                // "At the end of the villain turn, restore this card to its maximum HP. Then, {Breakaway} regains 5 HP."
                base.AddSideTrigger(base.AddEndOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, UnderPressureEndResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.Other }));
            }
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // Both sides: "This card has a maximum HP of {H * 4}"
            IEnumerator maxHPCoroutine = base.GameController.ChangeMaximumHP(base.Card, Game.H * 4, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(maxHPCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(maxHPCoroutine);
            }

            IEnumerator determineCoroutine = base.DeterminePlayLocation(storedResults, isPutIntoPlay, decisionSources, overridePlayArea, additionalTurnTakerCriteria);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(determineCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(determineCoroutine);
            }
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            // Both sides: "This card has a maximum HP of {H * 4}"
            IEnumerator maxHPCoroutine = base.GameController.ChangeMaximumHP(base.Card, Game.H * 4, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(maxHPCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(maxHPCoroutine);
            }

            IEnumerator baseFlipResponse = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(baseFlipResponse);
            }
            else
            {
                base.GameController.ExhaustCoroutine(baseFlipResponse);
            }

            yield break;
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // Both sides: "This card ... is indestructible."
            return (base.Card == card);
        }

        public IEnumerator UnderPressureStartResponse(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, if this card has more than {H + 2} HP, flip it."
            if (this.Card.HitPoints.Value > Game.H + 2)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            // "If this card did not flip this turn, return {H - 2} hero cards in play to their players' hands."
            if (!Journal.WasCardFlippedThisTurn(base.Card))
            {
                LinqCardCriteria choices = new LinqCardCriteria((Card c) => c.IsHero && c.IsInPlay && !c.IsCharacter);
                IEnumerator returnCoroutine = base.GameController.SelectAndReturnCards(this.DecisionMaker, Game.H - 2, choices, true, false, false, Game.H - 2, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(returnCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(returnCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator UnderPressureEndResponse(PhaseChangeAction pca)
        {
            // "At the end of the villain turn, restore this card to its maximum HP."
            IEnumerator setHPCoroutine = base.GameController.SetHP(base.Card, base.Card.MaximumHitPoints.Value, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(setHPCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(setHPCoroutine);
            }
            // "Then, {Breakaway} deals himself and the 2 hero targets with the highest HP 2 melee damage each."
            List<DealDamageAction> storedResultsDamage = new List<DealDamageAction>();
            Card breakaway = base.TurnTaker.FindCard("BreakawayCharacter");
            IEnumerator selfDamageCoroutine = DealDamage(breakaway, breakaway, 2, DamageType.Melee, storedResults: storedResultsDamage, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(selfDamageCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(selfDamageCoroutine);
            }

            IEnumerator heroDamageCoroutine = DealDamageToHighestHP(breakaway, 1, (Card c) => c.IsHero && GameController.IsCardVisibleToCardSource(c, GetCardSource()), (Card c) => 2, DamageType.Melee, numberOfTargets: () => 2);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(heroDamageCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(heroDamageCoroutine);
            }
            // "Remove 5 HP from Breakaway unless he was dealt damage this way."
            bool tookDamage = false;
            List<DealDamageAction> selfDamage = storedResultsDamage.FindAll((DealDamageAction dda) => dda.Target == breakaway);
            foreach (DealDamageAction dda in selfDamage)
            {
                if (dda != null && dda.Amount > 0)
                {
                    tookDamage = true;
                }
            }
            if (!tookDamage)
            {
                IEnumerator loseHPCoroutine = base.GameController.SetHP(breakaway, breakaway.HitPoints.Value - 5, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(loseHPCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(loseHPCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator GainingGroundStartResponse(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, if this card has less than {H + 2} HP, flip it."
            if (this.Card.HitPoints.Value < Game.H + 2)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            // "If this card did not flip this turn, {Breakaway} regains 1 HP."
            if (!Journal.WasCardFlippedThisTurn(base.Card))
            {
                IEnumerator gainHPCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("BreakawayCharacter"), 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(gainHPCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(gainHPCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator GainingGroundEndResponse(PhaseChangeAction pca)
        {
            // "At the end of the villain turn, restore this card to its maximum HP."
            IEnumerator setHPCoroutine = base.GameController.SetHP(base.Card, base.Card.MaximumHitPoints.Value, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(setHPCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(setHPCoroutine);
            }
            // "Then, {Breakaway} regains 5 HP."
            IEnumerator gainHPCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("BreakawayCharacter"), 5, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(gainHPCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(gainHPCoroutine);
            }
            yield break;
        }
    }
}
