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
    public class ImpulseCharacterCardController : HeroCharacterCardController
    {
        public ImpulseCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Draw a card, then discard a card."
            IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, optional: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Prevent the next damage that would be dealt to {ImpulseCharacter}."
            OnDealDamageStatusEffect preventNextDamage = new OnDealDamageStatusEffect(base.Card, "PreventDamage", "Prevent the next damage that would be dealt to " + base.CharacterCard.Title + ".", new TriggerType[] { TriggerType.CancelAction }, base.TurnTaker, base.Card);
            preventNextDamage.TargetCriteria.IsSpecificCard = base.Card;
            preventNextDamage.DamageAmountCriteria.GreaterThan = 0;
            preventNextDamage.NumberOfUses = 1;
            preventNextDamage.UntilTargetLeavesPlay(base.Card);

            IEnumerator statusCoroutine = base.AddStatusEffect(preventNextDamage);
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

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch(index)
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
            // "One player may draw a card now."
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCard(base.HeroTurnTakerController, numberOfCards: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "One hero may deal 1 target 2 melee damage."
            IEnumerator damageCoroutine = base.GameController.SelectHeroToSelectTargetAndDealDamage(base.HeroTurnTakerController, 2, DamageType.Melee, cardSource: GetCardSource());
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

        private IEnumerator UseIncapOption3()
        {
            // "Discard the top card of a deck."
            List<SelectLocationDecision> choice = new List<SelectLocationDecision>();
            IEnumerator chooseCoroutine = base.GameController.SelectADeck(base.HeroTurnTakerController, SelectionType.DiscardFromDeck, (Location l) => true, choice, noValidLocationsMessage: "There are no available decks.", cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            Location deck = GetSelectedLocation(choice);
            if (deck != null)
            {

                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(deck, null, showCard: (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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

        public IEnumerator PreventDamage(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            IEnumerator preventCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            yield break;
        }
    }
}
