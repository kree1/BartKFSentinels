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
            int hpAmt = GetPowerNumeral(0, 1);
            int toReveal = GetPowerNumeral(1, 2);
            int toHand = GetPowerNumeral(2, 1);
            // "{Dreadnought} regains 1 HP."
            IEnumerator healCoroutine = GameController.GainHP(CharacterCard, hpAmt, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healCoroutine);
            }
            // "Reveal the top 2 cards of your deck. Put 1 into your hand and the rest into your trash."
            IEnumerator revealCoroutine = RevealCardsFromTopOfDeck_DetermineTheirLocationEx(DecisionMaker, DecisionMaker, TurnTaker.Deck, new MoveCardDestination(HeroTurnTaker.Hand), new MoveCardDestination(TurnTaker.Trash), numberOfReveals: toReveal, numberToSelect: toHand, responsibleTurnTaker: TurnTaker);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(revealCoroutine);
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
