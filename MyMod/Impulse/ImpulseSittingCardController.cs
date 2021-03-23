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
    public class ImpulseSittingCardController : ImpulseUtilityCardController
    {
        public ImpulseSittingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "One player, other than you, may play a card or use a power."
            LinqTurnTakerCriteria otherPlayer = new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker);
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, "Another player may play a card", SelectionType.PlayCard, () => SelectHeroToPlayCard(base.HeroTurnTakerController, false, true, heroCriteria: otherPlayer)));
            options.Add(new Function(base.HeroTurnTakerController, "Another hero may use a power", SelectionType.UsePower, () => base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, additionalCriteria: otherPlayer, cardSource: GetCardSource())));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, false, noSelectableFunctionMessage: "No other players may currently play cards or use powers.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            // "If another player played a card this turn, prevent the next damage dealt to a hero target."
            List<PlayCardJournalEntry> thisTurnPlays = base.GameController.Game.Journal.PlayCardEntriesThisTurn().Where((PlayCardJournalEntry pcje) => pcje.CardPlayed.Owner.IsHero && pcje.CardPlayed.Owner != base.TurnTaker).ToList();
            bool otherHasPlayed = thisTurnPlays.Count > 0;
            if (otherHasPlayed)
            {
                CannotDealDamageStatusEffect preventNextDamage = new CannotDealDamageStatusEffect();
                preventNextDamage.TargetCriteria.IsHero = true;
                preventNextDamage.NumberOfUses = 1;
                preventNextDamage.IsPreventEffect = true;

                IEnumerator statusCoroutine = base.AddStatusEffect(preventNextDamage);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            // "If another hero used a power this turn, you may play an Ongoing card."
            List<UsePowerJournalEntry> thisTurnPowers = base.GameController.Game.Journal.UsePowerEntriesThisTurn().Where((UsePowerJournalEntry upje) => upje.PowerUser != base.HeroTurnTaker).ToList();
            bool otherHasUsedPower = thisTurnPowers.Count > 0;
            if (otherHasUsedPower)
            {
                IEnumerator ongoingCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, cardCriteria: new LinqCardCriteria((Card c) => c.DoKeywordsContain("ongoing")));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(ongoingCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(ongoingCoroutine);
                }
            }
            // "If neither happened, {ImpulseCharacter} deals up to 2 targets 2 melee damage each."
            if (!otherHasPlayed && !otherHasUsedPower)
            {
                IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Melee, 2, false, 0, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }
    }
}
