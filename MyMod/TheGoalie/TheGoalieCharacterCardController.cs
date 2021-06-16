﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class TheGoalieCharacterCardController : HeroCharacterCardController
    {
        public TheGoalieCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            int meleeTargets = GetPowerNumeral(0, 1);
            int meleeDamage = GetPowerNumeral(2, 0);
            int projectileDamage = GetPowerNumeral(2, 1);
            // "{TheGoalieCharacter} may deal 1 target 0 melee damage. {TheGoalieCharacter} may deal another target 1 projectile damage."
            List<DealDamageAction> damaged = new List<DealDamageAction>();
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), meleeDamage, DamageType.Melee, meleeTargets, false, 0, storedResultsDamage: damaged, cardSource: GetCardSource());
            IEnumerator projectileCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), projectileDamage, DamageType.Projectile, 1, false, 0, additionalCriteria: (Card c) => !damaged.Select((DealDamageAction dda) => dda.Target).Contains(c), storedResultsDamage: damaged, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
                yield return base.GameController.StartCoroutine(projectileCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
                base.GameController.ExhaustCoroutine(projectileCoroutine);
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
            // "Select a target. Reduce the next damage dealt to that target by 2."
            List<SelectCardDecision> choices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => c.IsInPlay && c.IsTarget, "target", useCardsSuffix: false), choices, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            SelectCardDecision choice = choices.FirstOrDefault();
            if (choice != null && choice.SelectedCard != null)
            {
                ReduceDamageStatusEffect protectEffect = new ReduceDamageStatusEffect(2);
                protectEffect.NumberOfUses = 1;
                protectEffect.TargetCriteria.IsSpecificCard = choice.SelectedCard;
                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(protectEffect, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "Select a target. Increase the next damage dealt by that target by 2."
            IEnumerator increaseCoroutine = base.GameController.SelectTargetAndIncreaseNextDamage(base.HeroTurnTakerController, 2, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "The 2 hero targets with the lowest HP regain 1 HP each."
            List<Card> lowestHeroTargets = new List<Card>();
            IEnumerator findLowestCoroutine = base.GameController.FindTargetsWithLowestHitPoints(1, 2, (Card c) => c.IsHero, lowestHeroTargets, evenIfCannotDealDamage: true, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findLowestCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findLowestCoroutine);
            }
            IEnumerator healCoroutine = base.GameController.GainHP(base.HeroTurnTakerController, (Card c) => lowestHeroTargets.Contains(c), 1, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            yield break;
        }
    }
}
