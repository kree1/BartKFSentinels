using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class EsemeliCardController : CardController
    {
        public EsemeliCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals the hero target with the highest HP 4 psychic damage. The player whose target takes damage this way may draw a card and their hero may use a power."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDrawPowerResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DrawCard, TriggerType.UsePower });
            // "At the start of the environment turn, reveal the top card of the environment deck and play or discard it. Then, this card deals each Wizard 2 psychic damage."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, RevealAttackResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.PlayCard, TriggerType.DiscardCard, TriggerType.DealDamage });
        }

        public IEnumerator DamageDrawPowerResponse(GameAction ga)
        {
            // "... this card deals the hero target with the highest HP 4 psychic damage. The player whose target takes damage this way may draw a card and their hero may use a power."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => IsHeroTarget(c), (Card c) => 4, DamageType.Psychic, storedResults: damageResults);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            DealDamageAction success = damageResults.FirstOrDefault((DealDamageAction dda) => dda.DidDealDamage && IsHeroTarget(dda.Target));
            if (success != null)
            {
                Card damaged = success.Target;
                if (damaged.Owner.IsHero)
                {
                    HeroTurnTakerController owner = base.GameController.FindHeroTurnTakerController(damaged.Owner.ToHero());
                    IEnumerator drawCoroutine = base.GameController.DrawCard(owner.HeroTurnTaker, optional: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(drawCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(drawCoroutine);
                    }
                    IEnumerator powerCoroutine = base.GameController.SelectAndUsePower(owner, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(powerCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(powerCoroutine);
                    }
                }
                else
                {
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction(damaged.Owner.Name + " is not a hero, so they can't draw a card or use a power.", Priority.Medium, GetCardSource(), damaged.ToEnumerable(), showCardSource: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                }
            }
            yield break;
        }

        public IEnumerator RevealAttackResponse(GameAction ga)
        {
            // "... reveal the top card of the environment deck and play or discard it."
            IEnumerator revealCoroutine = RevealCard_PlayItOrDiscardIt(base.TurnTakerController, base.TurnTaker.Deck, showRevealedCards: true, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "... Then, this card deals each Wizard 2 psychic damage."
            IEnumerator damageCoroutine = base.GameController.DealDamage(base.DecisionMaker, base.Card, (Card c) => c.DoKeywordsContain("wizard"), 2, DamageType.Psychic, cardSource: GetCardSource());
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
    }
}
