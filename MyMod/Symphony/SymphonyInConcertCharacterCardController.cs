using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class SymphonyInConcertCharacterCardController : HeroCharacterCardController
    {
        public SymphonyInConcertCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Discard a card. {Symphony} deals 1 target X sonic damage, where X = the number of your equipment cards in play plus 1."
            int numTargets = GetPowerNumeral(0, 1);
            int baseX = GetPowerNumeral(1, 1);
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(DecisionMaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            IEnumerator sonicCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), (Card t) => baseX + FindCardsWhere((Card c) => c.IsInPlay && IsEquipment(c) && c.Owner == TurnTaker).Count(), DamageType.Sonic, () => numTargets, false, null, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(sonicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(sonicCoroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may draw a card."
            return GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption2()
        {
            // "One player may play a card."
            return GameController.SelectHeroToPlayCard(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption3()
        {
            // "Select a target. Increase the next damage dealt by that target by 2."
            List<SelectCardDecision> targetChoices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.IncreaseNextDamage, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget), targetChoices, false, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (targetChoices.Any())
            {
                Card selected = GetSelectedCard(targetChoices);
                IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(2);
                buff.SourceCriteria.IsSpecificCard = selected;
                buff.NumberOfUses = 1;
                buff.UntilTargetLeavesPlay(selected);
                IEnumerator statusCoroutine = AddStatusEffect(buff);
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
