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
    public class UnderAttackCardController : TheGigamorphUtilityCardController
    {
        public UnderAttackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "At the end of the environment turn, move the Antibody with the highest HP next to the non-[b]tagged[/b] villain target with the highest HP. Then, deal each [b]tagged[/b] target and each Antibody 1 energy damage and destroy this card."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, MoveAttackDestroyResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage, TriggerType.DestroySelf });
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, search the environment deck and trash for an Antibody card and put it into play. Then, shuffle the environment deck."
            IEnumerator retrieveCoroutine = base.FetchAntibody(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(retrieveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(retrieveCoroutine);
            }
            yield break;
        }

        public IEnumerator MoveAttackDestroyResponse(PhaseChangeAction pca)
        {
            // "... move the Antibody with the highest HP next to the non-[b]tagged[/b] villain target with the highest HP."
            List<Card> nonTaggedVillains = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsVillainTarget(c) && !IsTagged(c)).ToList();
            List<Card> activeAntibodies = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("antibody")).ToList();
            int nonTaggedVillainCount = nonTaggedVillains.Count();
            int activeAntibodyCount = activeAntibodies.Count();
            if (nonTaggedVillainCount <= 0 || activeAntibodyCount <= 0)
            {
                string failMessage = "";
                if (nonTaggedVillainCount == 0)
                {
                    failMessage = "There are no non-tagged villain targets for " + base.Card.Title + " to move an Antibody next to.";
                }
                else
                {
                    failMessage = "There are no Antibody cards in play for " + base.Card.Title + " to move.";
                }
                IEnumerator showCoroutine = base.GameController.SendMessageAction(failMessage, Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
            }
            else
            {
                // Find non-tagged villain target with highest HP
                List<Card> villainChosen = new List<Card>();
                IEnumerator findVillainCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsVillainTarget(c) && !IsTagged(c), villainChosen, evenIfCannotDealDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findVillainCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findVillainCoroutine);
                }
                // Find Antibody with highest HP
                List<Card> antibodyChosen = new List<Card>();
                IEnumerator findAntibodyCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.DoKeywordsContain("antibody"), antibodyChosen, evenIfCannotDealDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findAntibodyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findAntibodyCoroutine);
                }
                // Move Antibody next to villain
                if (villainChosen != null && villainChosen.Count() > 0 && antibodyChosen != null && antibodyChosen.Count() > 0)
                {
                    Card thisVillain = villainChosen.FirstOrDefault();
                    Card thisAntibody = antibodyChosen.FirstOrDefault();
                    if (thisVillain != null && thisAntibody != null)
                    {
                        IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, thisAntibody, thisVillain.NextToLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: pca, doesNotEnterPlay: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(moveCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(moveCoroutine);
                        }
                    }
                }
            }
            // "Then, deal each [b]tagged[/b] target and each Antibody 1 energy damage..."
            IEnumerator attackCoroutine = base.GameController.DealDamage(base.DecisionMaker, base.Card, (Card c) => IsTagged(c) || c.DoKeywordsContain("antibody"), 1, DamageType.Energy, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(attackCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(attackCoroutine);
            }
            // "... and destroy this card."
            IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDestructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDestructCoroutine);
            }
            yield break;
        }
    }
}
