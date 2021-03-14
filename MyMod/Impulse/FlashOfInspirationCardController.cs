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
    public class FlashOfInspirationCardController : ImpulseUtilityCardController
    {
        public FlashOfInspirationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{ImpulseCharacter} deals 1 target 2 lightning damage."
            IEnumerator targetDamageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Lightning, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(targetDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(targetDamageCoroutine);
            }

            // "You may draw a card."
            IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }

            // "You may discard a card. If you don't, {ImpulseCharacter} deals himself 1 lightning damage."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, optional: true, storedResults: discards, selectionType: SelectionType.DiscardCard, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (!base.DidDiscardCards(discards))
            {
                IEnumerator selfDamageCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, 1, DamageType.Lightning, isOptional: false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selfDamageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selfDamageCoroutine);
                }
            }

            // "One player may play a card."
            IEnumerator playCoroutine = SelectHeroToPlayCard(base.HeroTurnTakerController, optionalSelectHero: false, optionalPlayCard: true);
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
    }
}
