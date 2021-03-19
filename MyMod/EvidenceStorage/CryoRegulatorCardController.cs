using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class CryoRegulatorCardController : EvidenceStorageUtilityCardController
    {
        public CryoRegulatorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " hasn't been activated since it entered play").Condition = () => base.Card.IsInPlayAndHasGameText && !MostRecentChosen().HasValue;
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " is set to protect itself and targets in " + base.Card.Location.HighestRecursiveLocation.OwnerName + "'s play area from " + MostRecentChosen().Value.ToString() + " damage.").Condition = () => base.Card.IsInPlayAndHasGameText && MostRecentChosen().HasValue;
        }

        private DamageType[] typeOptions = { DamageType.Fire, DamageType.Cold };
        private ITrigger ReduceDamageTrigger;

        public override void AddTriggers()
        {
            // "At the end of this play area's turn, choose fire damage or cold damage."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, ChooseTypeResponse, TriggerType.SelectDamageType);
            // "This card is immune to damage of the most recently chosen type."
            base.AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target == base.Card && dda.DamageType == MostRecentChosen().Value);
            // "Whenever another target in this play area would be dealt damage of the most recently chosen type, reduce that damage by 1 and change its damage type to the type that wasn't chosen."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target != base.Card && dda.Target.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageType == MostRecentChosen().Value, ReduceAndInvertResponse, new TriggerType[] { TriggerType.ReduceDamage, TriggerType.ChangeDamageType }, TriggerTiming.Before, isActionOptional: false);
            base.AddTriggers();
        }

        public IEnumerator ChooseTypeResponse(PhaseChangeAction pca)
        {
            // "... choose fire damage or cold damage."
            List<SelectDamageTypeDecision> choice = new List<SelectDamageTypeDecision>();
            // If this is a hero play area, that hero's player chooses; otherwise, the group does
            HeroTurnTakerController selector = base.DecisionMaker;
            HeroTurnTaker credited = base.Game.HeroTurnTakers.FirstOrDefault();
            if (base.Card.Location.HighestRecursiveLocation.IsHero)
            {
                selector = base.GameController.FindTurnTakerController(base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker).ToHero();
                credited = base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker.ToHero();
            }
            IEnumerator chooseCoroutine = base.GameController.SelectDamageType(selector, choice, new DamageType[] { DamageType.Fire, DamageType.Cold }, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            DamageType chosenType = choice.FirstOrDefault((SelectDamageTypeDecision sdtd) => sdtd.Completed).SelectedDamageType.Value;
            // Write the chosen type index in a Journal entry where we can retrieve it later
            base.Journal.RecordDecisionAnswer("CryoRegulatorDamageType", credited, typeOptions.IndexOf(chosenType), false, false, base.TurnTaker, base.Card, base.Card);
            yield break;
        }

        public IEnumerator ReduceAndInvertResponse(DealDamageAction dda)
        {
            // "... reduce that damage by 1 and change its damage type to the type that wasn't chosen."
            DamageType invertedType = DamageType.Energy;
            if (MostRecentChosen().Value == DamageType.Fire)
            {
                invertedType = DamageType.Cold;
            }
            else
            {
                invertedType = DamageType.Fire;
            }
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, ReduceDamageTrigger, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            IEnumerator invertCoroutine = base.GameController.ChangeDamageType(dda, invertedType, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(invertCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(invertCoroutine);
            }
            yield break;
        }

        public DamageType? MostRecentChosen()
        {
            PlayCardJournalEntry enteredPlay = base.GameController.Game.Journal.QueryJournalEntries((PlayCardJournalEntry e) => e.CardPlayed == base.Card).LastOrDefault();
            IEnumerable<DecisionAnswerJournalEntry> choiceEntries = base.Journal.DecisionAnswerEntries((DecisionAnswerJournalEntry daje) => daje.SelectedCard == base.Card && daje.DecisionIdentifier == "CryoRegulatorDamageType");
            if (enteredPlay != null)
            {
                DecisionAnswerJournalEntry latestChoice = choiceEntries.LastOrDefault();
                int? enteredIndex = base.GameController.Game.Journal.GetEntryIndex(enteredPlay);
                int? choseIndex = base.GameController.Game.Journal.GetEntryIndex(latestChoice);
                if (enteredIndex.HasValue && choseIndex.HasValue && enteredIndex.Value < choseIndex.Value)
                {
                    return typeOptions[latestChoice.AnswerIndex.Value];
                }
            }
            return null;
        }
    }
}
