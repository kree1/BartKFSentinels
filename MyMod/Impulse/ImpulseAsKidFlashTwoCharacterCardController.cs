using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class ImpulseAsKidFlashTwoCharacterCardController : HeroCharacterCardController
    {
        public ImpulseAsKidFlashTwoCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string DamageBeingRedirectedKey = "DamageBeingRedirected";
        public readonly string PowerNumeralForHPGainKey = "PowerNumeralForHPGain";

        public override void AddTriggers()
        {
            // When this card takes damage, if that damage was marked as redirected by this card's power, activate GainHPResponse
            AddTrigger((DealDamageAction dd) => dd.Target == Card && dd.DidDealDamage && GetCardPropertyJournalEntryBoolean(DamageBeingRedirectedKey).HasValueWhere((val) => val == true), GainHPResponse, TriggerType.GainHP, TriggerTiming.After);
        }

        private IEnumerator GainHPResponse(DealDamageAction dd)
        {
            // Get the amount of HP to regain from the Journal and regain that much HP
            int? hpGainAmount = GetCardPropertyJournalEntryInteger(PowerNumeralForHPGainKey);
            IEnumerator coroutine = GameController.GainHP(Card, hpGainAmount, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            Game.Journal.RecordCardProperties(Card, DamageBeingRedirectedKey, (bool?)null);
            Game.Journal.RecordCardProperties(Card, PowerNumeralForHPGainKey, (int?)null);

            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int hpGain = GetPowerNumeral(0, 2);
            int[] powerNumerals = { hpGain };

            // "The next time another hero would be dealt damage, redirect it to {ImpulseCharacter}. If {ImpulseCharacter} takes damage this way, he regains 2 HP."
            OnDealDamageStatusEffect redirect = new OnDealDamageStatusEffect(CardWithoutReplacements, nameof(RedirectDamageToMe), "The next time another hero would be dealt damage, redirect it to " + base.Card.Title + ". If " + base.Card.Title + " takes damage this way, he regains 2 HP.", new TriggerType[] { TriggerType.RedirectDamage, TriggerType.GainHP }, base.TurnTaker, base.Card, powerNumerals);
            redirect.CardFlippedExpiryCriteria.Card = base.Card;
            redirect.TargetCriteria.IsHeroCharacterCard = true;
            redirect.TargetCriteria.IsNotSpecificCard = base.Card;
            redirect.NumberOfUses = 1;
            IEnumerator redirectCoroutine = AddStatusEffect(redirect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            yield break;
        }

        public IEnumerator RedirectDamageToMe(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // "... redirect it to {ImpulseCharacter}. If {ImpulseCharacter} takes damage this way, he regains 2 HP."
            int? num = null;
            if (powerNumerals != null)
            {
                num = powerNumerals.ElementAtOrDefault(0);
            }
            if (!num.HasValue)
            {
                num = 2;
            }

            if (dd.IsRedirectable)
            {
                IEnumerator coroutine = GameController.RedirectDamage(dd, Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    // Use the Journal to record how much HP to regain
                    Game.Journal.RecordCardProperties(Card, PowerNumeralForHPGainKey, num);
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }

                // Use the CardProperty to activate the heal trigger from AddTriggers()
                SetCardPropertyToTrueIfRealAction(DamageBeingRedirectedKey);
            }
            yield break;
        }

        private IEnumerator HealResponse(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // Impulse regains 2 HP
            int hpGain = powerNumerals[0];
            if (hpGain > 0)
            {
                IEnumerator healCoroutine = base.GameController.GainHP(base.Card, hpGain, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
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
            // "One player may play a card now."
            IEnumerator playCoroutine = SelectHeroToPlayCard(base.HeroTurnTakerController, heroCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && base.GameController.CanPlayCards(FindTurnTakerController(tt), GetCardSource())));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "One player draws 2 cards, then discards a card."
            List<SelectTurnTakerDecision> playerChoice = new List<SelectTurnTakerDecision>();
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCards(base.HeroTurnTakerController, 2, optionalSelectHero: false, optionalDrawCards: false, 2, storedResults: playerChoice, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame, "active heroes"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            TurnTaker playerChosen = (from d in playerChoice where d.Completed select d.SelectedTurnTaker).FirstOrDefault();
            if (playerChosen != null)
            {
                IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.GameController.FindTurnTakerController(playerChosen).ToHero(), optional: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "Destroy a target with 1 HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1, "targets with 1 HP", useCardsSuffix: false), optional: false, cardSource: GetCardSource());
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
