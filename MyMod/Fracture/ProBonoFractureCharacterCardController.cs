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
    public class ProBonoFractureCharacterCardController : HeroCharacterCardController
    {
        public ProBonoFractureCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Up to 2 targets regain 1 HP each."
            int numTargets = GetPowerNumeral(0, 2);
            int amtHP = GetPowerNumeral(1, 1);
            IEnumerator chooseCoroutine = base.GameController.SelectAndGainHP(base.HeroTurnTakerController, amtHP, optional: true, numberOfTargets: numTargets, cardSource: GetCardSource()) ;
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            yield break;
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
            IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, optionalSelectHero: false, optionalUsePower: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "Select a target. Reduce the next damage dealt to that target by 2."
            List<SelectCardDecision> result = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => c.IsInPlay && c.IsTarget, "target", useCardsSuffix: false), result, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            SelectCardDecision choice = result.FirstOrDefault();
            if (choice != null && choice.SelectedCard != null)
            {
                ReduceDamageStatusEffect protection = new ReduceDamageStatusEffect(2);
                protection.NumberOfUses = 1;
                protection.TargetCriteria.IsSpecificCard = choice.SelectedCard;
                IEnumerator protectCoroutine = AddStatusEffect(protection);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(protectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(protectCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "Destroy an Ongoing card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing"), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
