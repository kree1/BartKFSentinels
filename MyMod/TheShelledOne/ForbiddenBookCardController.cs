using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class ForbiddenBookCardController : StrikeCardController
    {
        public ForbiddenBookCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a player discards a card from their hand, this card regains 1 HP."
            AddTrigger((DiscardCardAction d) => d.WasCardDiscarded && d.Origin.IsHand, (DiscardCardAction dca) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            // "At the end of the villain turn, each player may discard a card. Then, if this card has {H * 2} or more HP, put a token on {TheShelledOne} and set this card's HP to 0."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardAndCheckResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.AddTokensToPool, TriggerType.GainHP });
        }

        public IEnumerator DiscardAndCheckResponse(GameAction ga)
        {
            // "... each player may discard a card."
            IEnumerator discardCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand), SelectionType.DiscardCard, (TurnTaker tt) => SelectAndDiscardCards(FindHeroTurnTakerController(tt.ToHero()), 1, optional: false, requiredDecisions: 0, responsibleTurnTaker: tt), requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Then, if this card has {H * 2} or more HP, put a token on {TheShelledOne} and set this card's HP to 0."
            if (base.Card.HitPoints.HasValue && base.Card.HitPoints.Value >= H * 2)
            {
                IEnumerator tokenCoroutine = base.AddTokenAndResetResponse(ga);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(tokenCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(tokenCoroutine);
                }
            }
            yield break;
        }
    }
}
