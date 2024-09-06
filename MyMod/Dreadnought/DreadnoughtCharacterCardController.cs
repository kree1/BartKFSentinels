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
    public class DreadnoughtCharacterCardController : HeroCharacterCardController
    {
        public DreadnoughtCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "{Dreadnought} deals 1 target 2 melee damage. Discard a card. Draw a card."
            int numTargets = GetPowerNumeral(0, 1);
            int meleeAmt = GetPowerNumeral(1, 2);
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), meleeAmt, DamageType.Melee, numTargets, false, numTargets, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(DecisionMaker, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            IEnumerator drawCoroutine = DrawCard();
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One hero may use a power now."
            yield return GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption2()
        {
            // "One player may draw 2 cards. If they do, they discard a card."
            List<SelectTurnTakerDecision> decisions = new List<SelectTurnTakerDecision>();
            List<DrawCardAction> drawResults = new List<DrawCardAction>();
            IEnumerator drawCoroutine = GameController.SelectHeroToDrawCards(DecisionMaker, numberOfCards: 2, storedResults: decisions, storedDrawResults: drawResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
            SelectTurnTakerDecision choice = decisions.FirstOrDefault();
            if (choice != null && choice.SelectedTurnTaker != null && DidDrawCards(drawResults))
            {
                IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(FindHeroTurnTakerController(choice.SelectedTurnTaker.ToHero()), responsibleTurnTaker: choice.SelectedTurnTaker, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
        }

        private IEnumerator UseIncapOption3()
        {
            // "Select a target. Reduce the next damage dealt to that target by 2."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = GameController.SelectCardAndStoreResults(HeroTurnTakerController, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => c.IsInPlay && c.IsTarget, "target", useCardsSuffix: false), choices, false, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chooseCoroutine);
            }
            SelectCardDecision choice = choices.FirstOrDefault();
            if (choice != null && choice.SelectedCard != null)
            {
                ReduceDamageStatusEffect protectEffect = new ReduceDamageStatusEffect(2);
                protectEffect.NumberOfUses = 1;
                protectEffect.TargetCriteria.IsSpecificCard = choice.SelectedCard;
                IEnumerator statusCoroutine = AddStatusEffect(protectEffect);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
        }
    }
}
