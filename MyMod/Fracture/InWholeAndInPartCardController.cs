using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class InWholeAndInPartCardController : FractureUtilityCardController
    {
        public InWholeAndInPartCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => "No damage type has been chosen for " + base.Card.Title + ".").Condition = () => base.Card.IsInPlayAndHasGameText && base.GetCardPropertyJournalEntryInteger(LastChosenType).HasValue && (base.GetCardPropertyJournalEntryInteger(LastChosenType).Value < 0 || base.GetCardPropertyJournalEntryInteger(LastChosenType).Value >= typeOptions.Length);
            SpecialStringMaker.ShowSpecialString(() => base.Card.Title + "'s damage type is " + MostRecentChosen().Value.ToString() + ".").Condition = () => base.Card.IsInPlayAndHasGameText && base.GetCardPropertyJournalEntryInteger(LastChosenType).HasValue && base.GetCardPropertyJournalEntryInteger(LastChosenType).Value >= 0 && base.GetCardPropertyJournalEntryInteger(LastChosenType).Value < typeOptions.Length;
        }

        private readonly string LastChosenType = "LastChosenType";
        private readonly DamageType[] typeOptions = { DamageType.Cold, DamageType.Energy, DamageType.Fire, DamageType.Infernal, DamageType.Lightning, DamageType.Melee, DamageType.Projectile, DamageType.Psychic, DamageType.Radiant, DamageType.Sonic, DamageType.Toxic };

        public DamageType? MostRecentChosen()
        {
            int? lastChosenIndex = base.GetCardPropertyJournalEntryInteger(LastChosenType);
            if (lastChosenIndex.HasValue && lastChosenIndex.Value >= 0 && lastChosenIndex.Value < typeOptions.Length)
            {
                return typeOptions[lastChosenIndex.Value];
            }
            return null;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "... at the start of your turn, you may discard a card. If you do, destroy an environment card and choose a damage type."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardDestroyChooseResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroyCard, TriggerType.ChangeDamageType });
        }

        public override IEnumerator Play()
        {
            // Set chosen damage type to None
            SetCardProperty(LastChosenType, -1);
            // "When this card enters play... you may discard a card. If you do, destroy an environment card and choose a damage type."
            IEnumerator discardCoroutine = DiscardDestroyChooseResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "If a damage type has been chosen for this card, {FractureCharacter} deals up to 2 targets 1 damage each of that type."
            int numTargets = GetPowerNumeral(0, 2);
            int numDamage = GetPowerNumeral(1, 1);
            if (MostRecentChosen().HasValue)
            {
                IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), numDamage, MostRecentChosen().Value, numTargets, false, 0, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            else
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("No damage type has been chosen for " + base.Card.Title + ", so no damage was dealt.", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            // "Choose a damage type."
            List<SelectDamageTypeDecision> typeChoice = new List<SelectDamageTypeDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectDamageType(base.HeroTurnTakerController, storedResults: typeChoice, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            /*Log.Debug("typeChoice.Any((SelectDamageTypeDecision sdtd) => sdtd.Completed): " + typeChoice.Any((SelectDamageTypeDecision sdtd) => sdtd.Completed).ToString());
            SelectDamageTypeDecision completedChoice = typeChoice.FirstOrDefault((SelectDamageTypeDecision sdtd) => sdtd.Completed);
            Log.Debug("completedChoice == null: " + (completedChoice == null).ToString());
            Log.Debug("completedChoice.SelectedDamageType: " + completedChoice.SelectedDamageType.ToString());
            Log.Debug("completedChoice.SelectedDamageType.Value: " + completedChoice.SelectedDamageType.Value.ToString());*/
            DamageType chosenType = typeChoice.FirstOrDefault((SelectDamageTypeDecision sdtd) => sdtd.Completed).SelectedDamageType.Value;
            base.SetCardProperty(LastChosenType, typeOptions.IndexOf(chosenType).Value);
            yield break;
        }

        public IEnumerator DiscardDestroyChooseResponse(GameAction ga)
        {
            // "... you may discard a card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, optional: true, storedResults: discards, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If you do, destroy an environment card and choose a damage type."
            if (DidDiscardCards(discards))
            {
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
                List<SelectDamageTypeDecision> typeChoice = new List<SelectDamageTypeDecision>();
                IEnumerator chooseCoroutine = base.GameController.SelectDamageType(base.HeroTurnTakerController, storedResults: typeChoice, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                DamageType chosenType = typeChoice.FirstOrDefault((SelectDamageTypeDecision sdtd) => sdtd.Completed).SelectedDamageType.Value;
                base.SetCardProperty(LastChosenType, typeOptions.IndexOf(chosenType).Value);
            }
            yield break;
        }
    }
}
