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
    public class QueltPeliaenCardController : CardController
    {
        public QueltPeliaenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, each other target regains 1 HP. Then, each other target with 3 or fewer HP regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, MassHealResponse, TriggerType.GainHP);
            // "At the start of the environment turn, 1 player may discard a card. If they do, destroy all Myths."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardToDestroyResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DestroyCard });
            // "Whenever a player draws a card, reduce the next damage dealt to a character card by 1."
            AddTrigger<DrawCardAction>((DrawCardAction dca) => true, ProtectCharactersResponse, TriggerType.CreateStatusEffect, TriggerTiming.After);
        }

        public IEnumerator MassHealResponse(GameAction ga)
        {
            // "... each other target regains 1 HP."
            IEnumerator allHealCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => c != base.Card, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(allHealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(allHealCoroutine);
            }
            // "Then, each other target with 3 or fewer HP regains 1 HP."
            IEnumerator weakHealCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => c != base.Card && c.HitPoints.HasValue && c.HitPoints.Value <= 3, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(weakHealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(weakHealCoroutine);
            }
        }

        public IEnumerator DiscardToDestroyResponse(GameAction ga)
        {
            // "... 1 player may discard a card. If they do, destroy all Myths."
            List<DiscardCardAction> discarded = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalSelectHero: true, storedResultsDiscard: discarded, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discarded))
            {
                IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.DoKeywordsContain("myth"), "Myth", false, false, "Myth", "Myths"), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
        }

        public IEnumerator ProtectCharactersResponse(GameAction ga)
        {
            // "... reduce the next damage dealt to a character card by 1."
            ReduceDamageStatusEffect protection = new ReduceDamageStatusEffect(1);
            protection.TargetCriteria.IsCharacter = true;
            protection.NumberOfUses = 1;
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(protection, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }
    }
}
