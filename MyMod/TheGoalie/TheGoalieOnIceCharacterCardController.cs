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
            List<PlayCardAction> playResults = new List<PlayCardAction>();
            IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.HeroTurnTakerController, (Card c) => c.DoKeywordsContain("goalposts") && c.Location == base.TurnTaker.Trash, storedResults: playResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            Log.Debug("playResults.Count(): " + playResults.Count().ToString());
            foreach(PlayCardAction pca in playResults)
            {
                Log.Debug("    " + pca.ToString());
                Log.Debug("    WasCardPlayed: " + pca.WasCardPlayed.ToString());
            }
            Log.Debug("playResults.Where((PlayCardAction pca) => pca.WasCardPlayed).Count(): " + playResults.Where((PlayCardAction pca) => pca.WasCardPlayed).Count().ToString());
            Log.Debug("playResults.Any((PlayCardAction pca) => pca.WasCardPlayed): " + playResults.Any((PlayCardAction pca) => pca.WasCardPlayed).ToString());
            // "{TheGoalieCharacter} deals 1 target 1 melee damage."
            List <SelectCardDecision> selectedTargets = new List<SelectCardDecision>();
            IEnumerable<DealDamageAction> followUp = null;
            if (!playResults.Any((PlayCardAction pca) => pca.WasCardPlayed))
            {
                followUp = new DealDamageAction[] { new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, coldAmt, DamageType.Cold) };
            }
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.Card), meleeAmt, DamageType.Melee, numTargets, false, numTargets, storedResultsDecisions: selectedTargets, followUpDamageInformation: followUp, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "If no card entered play this way, {TheGoalieCharacter} deals that target 1 cold damage."
            IEnumerable<Card> targets = (from SelectCardDecision dec in selectedTargets where dec != null && dec.SelectedCard != null select dec.SelectedCard);
            if (!playResults.Any((PlayCardAction pca) => pca.WasCardPlayed))
            {
                IEnumerator coldCoroutine = base.GameController.DealDamage(base.HeroTurnTakerController, base.Card, (Card c) => targets.Contains(c), coldAmt, DamageType.Cold, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coldCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coldCoroutine);
                }
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
