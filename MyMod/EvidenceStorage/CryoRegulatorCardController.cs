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
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " hasn't been activated since it entered play", showInEffectsList: () => base.Card.IsInPlayAndHasGameText && (!MostRecentChosen().HasValue || !activeOptions.Contains(MostRecentChosen().Value))).Condition = () => base.Card.IsInPlayAndHasGameText && (!MostRecentChosen().HasValue || !activeOptions.Contains(MostRecentChosen().Value));
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " is set to protect itself and targets in " + base.Card.Location.HighestRecursiveLocation.OwnerName + "'s play area from " + MostRecentChosen().Value.ToString() + " damage.", showInEffectsList: () => base.Card.IsInPlayAndHasGameText && MostRecentChosen().HasValue && activeOptions.Contains(MostRecentChosen().Value)).Condition = () => base.Card.IsInPlayAndHasGameText && MostRecentChosen().HasValue && activeOptions.Contains(MostRecentChosen().Value);
        }

        private DamageType[] typeOptions = { DamageType.Fire, DamageType.Cold, DamageType.Energy };
        private DamageType[] activeOptions = { DamageType.Fire, DamageType.Cold };
        private const string LastChosenType = "LastChosenType";
        private ITrigger ReduceDamageTrigger;

        public override void AddTriggers()
        {
            // "At the end of this play area's turn, choose fire damage or cold damage."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, ChooseTypeResponse, TriggerType.SelectDamageType);
            // "This card is immune to damage of the most recently chosen type."
            base.AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target == base.Card && dda.DamageType == MostRecentChosen().Value && activeOptions.Contains(MostRecentChosen().Value));
            // "Whenever another target in this play area would be dealt damage of the most recently chosen type, reduce that damage by 1 and change its damage type to the type that wasn't chosen."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target != base.Card && dda.Target.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageType == MostRecentChosen().Value && activeOptions.Contains(MostRecentChosen().Value), ReduceAndInvertResponse, new TriggerType[] { TriggerType.ReduceDamage, TriggerType.ChangeDamageType }, TriggerTiming.Before, isActionOptional: false);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // Set chosen type to energy (ignored by damage triggers)
            SetCardProperty(LastChosenType, 2);
            return base.Play();
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
            base.SetCardProperty(LastChosenType, typeOptions.IndexOf(chosenType).Value);
            string message = base.Card.Title + " will protect itself and targets in " + base.Card.Location.HighestRecursiveLocation.OwnerName + "'s play area from " + MostRecentChosen().Value.ToString() + " damage.";
            IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(showCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(showCoroutine);
            }
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
            int? lastChosenIndex = base.GetCardPropertyJournalEntryInteger(LastChosenType);
            if (lastChosenIndex.HasValue)
            {
                return typeOptions[lastChosenIndex.Value];
            }
            return null;
        }
    }
}
