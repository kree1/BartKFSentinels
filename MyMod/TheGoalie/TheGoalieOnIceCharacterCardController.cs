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
    public class TheGoalieOnIceCharacterCardController : HeroCharacterCardController
    {
        public TheGoalieOnIceCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public static readonly string GoalpostsKeyword = "goalposts";
        public static readonly string GoalpostsIdentifier = "PlaceOfPower";

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int meleeAmt = GetPowerNumeral(1, 1);
            int coldAmt = GetPowerNumeral(2, 1);
            // "Play a Goalposts card from your trash."
            IEnumerator playCoroutine = base.GameController.SelectAndMoveCard(base.HeroTurnTakerController, (Card c) => c.DoKeywordsContain("goalposts") && c.Location == base.TurnTaker.Trash, base.TurnTaker.PlayArea, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "{TheGoalieCharacter} deals 1 target 1 melee damage and 1 cold damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, meleeAmt, DamageType.Melee));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, coldAmt, DamageType.Cold));
            IEnumerator damageCoroutine = SelectTargetsAndDealMultipleInstancesOfDamage(instances, minNumberOfTargets: numTargets, maxNumberOfTargets: numTargets);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
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
            IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, cardSource: GetCardSource());
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
            // "Destroy an environment card."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: base.Card, cardSource: GetCardSource());
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

        private IEnumerator UseIncapOption3()
        {
            // "Until the start of your turn, increase melee damage dealt by hero targets by 1."
            IncreaseDamageStatusEffect gongshow = new IncreaseDamageStatusEffect(1);
            gongshow.SourceCriteria.IsHero = true;
            gongshow.SourceCriteria.IsTarget = true;
            gongshow.DamageTypeCriteria.AddType(DamageType.Melee);
            gongshow.UntilStartOfNextTurn(base.TurnTaker);
            IEnumerator statusCoroutine = AddStatusEffect(gongshow);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }
    }
}
