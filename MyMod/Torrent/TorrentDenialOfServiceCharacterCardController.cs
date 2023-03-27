using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class TorrentDenialOfServiceCharacterCardController : HeroCharacterCardController
    {
        public TorrentDenialOfServiceCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int amount = GetPowerNumeral(0, 1);
            // "{TorrentCharacter} deals each of your targets and each non-hero target 1 lightning damage."
            IEnumerator damageCoroutine = base.GameController.DealDamage(base.HeroTurnTakerController, base.Card, (Card c) => (c.Owner == base.TurnTaker || !IsHeroTarget(c)), amount, DamageType.Lightning, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
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
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsEnvironment), false, responsibleCard: base.Card, cardSource: GetCardSource());
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
            // "Until the start of your next turn, this card counts as a target with 1 HP that is indestructible and immune to damage."
            MakeTargetStatusEffect animate = new MakeTargetStatusEffect(1, true);
            animate.CardsToMakeTargets.IsSpecificCard = base.Card;
            animate.UntilStartOfNextTurn(base.TurnTaker);
            IEnumerator animateCoroutine = AddStatusEffect(animate);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(animateCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(animateCoroutine);
            }
            ImmuneToDamageStatusEffect immunity = new ImmuneToDamageStatusEffect();
            immunity.TargetCriteria.IsSpecificCard = base.Card;
            immunity.UntilStartOfNextTurn(base.TurnTaker);
            IEnumerator immuneCoroutine = AddStatusEffect(immunity);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(immuneCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(immuneCoroutine);
            }
            yield break;
        }
    }
}
