using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class AdhesiveAntibodyCardController : TheGigamorphUtilityCardController
    {
        public AdhesiveAntibodyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLowestHP(3, () => 1, new LinqCardCriteria((Card c) => !c.DoKeywordsContain("antibody") && !IsTagged(c), "non-Antibody non-tagged", singular: "target", plural: "targets"), showInEffectsList: () => false);
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override void AddTriggers()
        {
            // "Increase damage dealt to that target by Immune cards by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsTarget && dda.Target == GetCardThisCardIsNextTo() && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.DoKeywordsContain("immune"), (DealDamageAction dda) => 1);
            // "At the start of that target's turn, this card deals it 1 toxic damage."
            base.AddStartOfTurnTrigger((TurnTaker tt) => GetCardThisCardIsNextTo() != null && tt == GetCardThisCardIsNextTo().Owner && GetCardThisCardIsNextTo().IsTarget, (PhaseChangeAction pca) => DealDamage(base.Card, GetCardThisCardIsNextTo(), 1, DamageType.Toxic, cardSource: GetCardSource()), TriggerType.DealDamage);
            // "When this card would leave play, instead it and the target it's next to each regain 5 HP, then move it next to the non-Antibody non-[b]tagged[/b] target with the third lowest HP and it becomes indestructible until the end of the turn."
            base.AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.CardToDestroy.Card == base.Card, WouldLeavePlayResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.MoveCard }, TriggerTiming.Before, priority: TriggerPriority.High);
            base.AddTrigger<MoveCardAction>((MoveCardAction mca) => mca.CardToMove == base.Card && mca.Origin.IsInPlay && !mca.Destination.IsInPlay, WouldLeavePlayResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.MoveCard }, TriggerTiming.Before, priority: TriggerPriority.High);
            base.AddTrigger<FlipCardAction>((FlipCardAction fca) => fca.CardToFlip.Card == base.Card, WouldLeavePlayResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.MoveCard }, TriggerTiming.Before, priority: TriggerPriority.High);
            // [If the card this is next to leaves play, this card falls off and stays in their play area]
            base.AddIfTheCardThatThisCardIsNextToLeavesPlayMoveItToTheirPlayAreaTrigger(false, true);
            base.AddTriggers();
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card next to the non-Antibody non-[b]tagged[/b] target with the third lowest HP."
            List<Card> match = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(3, (Card c) => !c.DoKeywordsContain("antibody") && !IsTagged(c), match, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (match != null && match.Count() > 0)
            {
                Card selected = match.FirstOrDefault();
                if (selected != null && storedResults != null)
                {
                    storedResults.Add(new MoveCardDestination(selected.NextToLocation));
                }
            }
        }

        public IEnumerator WouldLeavePlayResponse(GameAction ga)
        {
            // "When this card would leave play, instead..."
            IEnumerator replaceCoroutine = base.GameController.CancelAction(ga, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(replaceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(replaceCoroutine);
            }
            // "... it and the target it's next to each regain 5 HP..."
            List<Card> toHeal = new List<Card>();
            toHeal.Add(base.Card);
            if (GetCardThisCardIsNextTo() != null)
            {
                toHeal.Add(GetCardThisCardIsNextTo());
            }
            IEnumerator healCoroutine = base.GameController.GainHP(base.DecisionMaker, (Card c) => toHeal.Contains(c), 5, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "... then move it next to the non-Antibody non-[b]tagged[/b] target with the third lowest HP..."
            List<Card> match = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(3, (Card c) => !c.DoKeywordsContain("antibody") && !IsTagged(c), match, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            if (match != null && match.Count() > 0)
            {
                Card selected = match.FirstOrDefault();
                IEnumerator attachCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, selected.NextToLocation, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(attachCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(attachCoroutine);
                }
            }
            // "... and it becomes indestructible until the end of the turn."
            MakeIndestructibleStatusEffect dontDestroyAgain = new MakeIndestructibleStatusEffect();
            dontDestroyAgain.CardsToMakeIndestructible.IsSpecificCard = base.Card;
            dontDestroyAgain.UntilThisTurnIsOver(base.Game);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(dontDestroyAgain, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }
    }
}
