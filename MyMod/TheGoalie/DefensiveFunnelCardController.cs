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
    public class DefensiveFunnelCardController : TheGoalieUtilityCardController
    {
        public DefensiveFunnelCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, cardCriteria: GoalpostsCards);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, you may have a villain target deal {TheGoalieCharacter} 1 irreducible melee damage. If {TheGoalieCharacter} is dealt damage this way, you may play a Goalposts card from your trash."
            AddStartOfTurnTrigger((TurnTaker tt) => IsVillain(tt), PullAggroResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard });
        }

        public IEnumerator PullAggroResponse(PhaseChangeAction pca)
        {
            // "...  you may have a villain target deal {TheGoalieCharacter} 1 irreducible melee damage."
            List<SelectCardDecision> targetResults = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.CardToDealDamage, new LinqCardCriteria((Card c) => IsVillainTarget(c) && c.IsInPlayAndHasGameText, "villain targets in play", false, false, "villain target in play", "villain targets in play"), targetResults, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            Card selectedTarget = GetSelectedCard(targetResults);
            if (selectedTarget != null)
            {
                List<DealDamageAction> damageResults = new List<DealDamageAction>();
                IEnumerator meleeCoroutine = DealDamage(selectedTarget, base.CharacterCard, 1, DamageType.Melee, isIrreducible: true, storedResults: damageResults, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(meleeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(meleeCoroutine);
                }

                // "If {TheGoalieCharacter} is dealt damage this way, you may play a Goalposts card from your trash."
                if (DidDealDamage(damageResults, toSpecificTarget: base.CharacterCard))
                {
                    MoveCardDestination dest = new MoveCardDestination(base.TurnTaker.PlayArea);
                    IEnumerator playCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(base.HeroTurnTakerController, base.TurnTaker.Trash, GoalpostsCards, dest.ToEnumerable(), optional: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
            yield break;
        }

        public bool DamagedByOtherHeroesThisRound(Card c)
        {
            return Journal.DealDamageEntriesThisRound().Any((DealDamageJournalEntry ddje) => IsHeroCharacterCard(ddje.SourceCard) && ddje.SourceCard != base.CharacterCard && ddje.TargetCard == c);
        }

        private string TargetsDamagedByOtherHeroesThisRound()
        {
            string start = "Targets dealt damage by heroes other than " + base.CharacterCard.Title + " this round: ";
            IEnumerable<Card> damagedTargets = base.GameController.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && DamagedByOtherHeroesThisRound(c), visibleToCard: GetCardSource());
            string end = null;
            if (damagedTargets.FirstOrDefault() == null)
            {
                end = "None";
            }
            else
            {
                end = string.Join(", ", damagedTargets.Select(c => c.Title).ToArray());
            }
            return start + end + ".";
        }
    }
}
