using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class EbonyNeedleCardController : EvidenceStorageUtilityCardController
    {
        public EbonyNeedleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash);
        }

        public override void AddTriggers()
        {
            // "At the end of this play area's turn, this card may deal a character card in this play area {H - 1} psychic damage. If it deals no damage this way, destroy a non-character card from this play area's deck, then destroy this card."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, StabOrDestroyResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
            // "When this card is destroyed, if the environment trash has fewer than {H} cards, discard cards from the top of the environment deck until a Storage card is discarded. Put it into play."
            base.AddWhenDestroyedTrigger(DigIfShallowResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
            base.AddTriggers();
        }

        public IEnumerator StabOrDestroyResponse(PhaseChangeAction pca)
        {
            // "... this card may deal a character card in this play area {H - 1} psychic damage."
            List<DealDamageAction> storedResultsDamage = new List<DealDamageAction>();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.DecisionMaker, new DamageSource(base.GameController, base.Card), Game.H - 1, DamageType.Psychic, 1, false, 0, additionalCriteria: (Card c) => c.IsCharacter && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, storedResultsDamage: storedResultsDamage, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If it deals no damage this way, destroy a non-character card from this play area's deck, then destroy this card."
            if (storedResultsDamage == null || storedResultsDamage.Count((DealDamageAction dda) => dda.DidDealDamage) <= 0)
            {
                // Destroy a non-character card from this play area's deck
                TurnTaker host = base.Card.Location.OwnerTurnTaker;
                HeroTurnTakerController boss = DecisionMaker;
                if (host.IsHero)
                {
                    boss = base.GameController.FindHeroTurnTakerController(host.ToHero());
                }
                IEnumerator destroyNonCharacterCoroutine = base.GameController.SelectAndDestroyCard(boss, new LinqCardCriteria((Card c) => c.Owner == host && !c.IsCharacter, "from " + host.Name + "'s deck", useCardsSuffix: false, useCardsPrefix: true), false, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyNonCharacterCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyNonCharacterCoroutine);
                }
                // Destroy this card
                IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, optional: false, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selfDestructCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selfDestructCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DigIfShallowResponse(DestroyCardAction dca)
        {
            // "... if the environment trash has fewer than {H} cards, discard cards from the top of the environment deck until a Storage card is discarded. Put it into play."
            if (base.TurnTaker.Trash.Cards.Count() < Game.H)
            {
                IEnumerator digCoroutine = base.DigForStorage();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(digCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(digCoroutine);
                }
            }
            yield break;
        }
    }
}
