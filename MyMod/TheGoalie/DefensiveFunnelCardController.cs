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
            SpecialStringMaker.ShowSpecialString(TargetsDamagedByOtherHeroesThisRound);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the environment turn, {TheGoalieCharacter} may deal 2 melee damage to a target that was dealt damage by another hero this round."
            AddStartOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, TeamUpResponse, TriggerType.DealDamage);
        }

        public IEnumerator TeamUpResponse(PhaseChangeAction pca)
        {
            // "If a Goalposts card is in play, increase that damage by 1."
            ITrigger increaseResponse = AddIncreaseDamageTrigger((DealDamageAction dda) => base.GameController.FindCardsWhere(GoalpostsInPlay).Any() && dda.CardSource.CardController == this, 1);

            // "... {TheGoalieCharacter} may deal 2 melee damage to a target that was dealt damage by another hero this round."
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Melee, 1, false, 0, additionalCriteria: DamagedByOtherHeroesThisRound, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }

            RemoveTrigger(increaseResponse);
            yield break;
        }

        public bool DamagedByOtherHeroesThisRound(Card c)
        {
            return Journal.DealDamageEntriesThisRound().Any((DealDamageJournalEntry ddje) => ddje.SourceCard.IsHeroCharacterCard && ddje.SourceCard != base.CharacterCard && ddje.TargetCard == c);
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
