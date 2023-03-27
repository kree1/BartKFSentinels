using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class FabricatedDisasterCardController : EmpireUtilityCardController
    {
        public FabricatedDisasterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a hero uses a power, reduce damage dealt using that power by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.CardSource != null && dda.CardSource.PowerSource != null, (DealDamageAction dda) => 1);
            // "At the end of the environment turn, one player may discard their hand. If {H + 3} or more cards were discarded this way, each player with no cards in hand draws 2 cards, then move this card under the Timeline card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, MayDiscardResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DrawCard, TriggerType.MoveCard });
        }

        public IEnumerator MayDiscardResponse(PhaseChangeAction pca)
        {
            // "... one player may discard their hand. If {H + 3} or more cards were discarded this way, each player with no cards in hand draws 2 cards, then move this card under the Timeline card."
            List<SelectTurnTakerDecision> choice = new List<SelectTurnTakerDecision>();
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator selectCoroutine = base.GameController.SelectHeroToDiscardTheirHand(DecisionMaker, optionalSelectHero: true, optionalDiscardCards: false, storedResultsTurnTaker: choice, storedResultsDiscard: discards, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            if (DidDiscardCards(discards, H + 3, orMore: true))
            {
                IEnumerator eraseCoroutine = EraseFromHistoryResponse(discards.FirstOrDefault());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(eraseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(eraseCoroutine);
                }
            }
        }

        public IEnumerator EraseFromHistoryResponse(GameAction ga)
        {
            // "... each player with no cards in hand draws 2 cards, then move this card under the Timeline card."
            string announce = "The heroes ";
            if (ga is DiscardCardAction)
            {
                if ((ga as DiscardCardAction).HeroTurnTakerController != null && (ga as DiscardCardAction).HeroTurnTakerController.HeroTurnTaker != null)
                {
                    announce = (ga as DiscardCardAction).HeroTurnTakerController.HeroTurnTaker.NameRespectingVariant + " ";
                }
            }
            announce += "uncovered a way to prevent the crisis that sparked this world's fear and distrust of superhumans!";
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(announce, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            string effect = "The effects of " + base.Card.Title + " are removed from history!";
            IEnumerator effectCoroutine = base.GameController.SendMessageAction(effect, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(effectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(effectCoroutine);
            }
            IEnumerable<TurnTaker> emptyHanded = FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && tt.ToHero().NumberOfCardsInHand <= 0);
            IEnumerator drawCoroutine = base.GameController.DrawCards(new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().NumberOfCardsInHand <= 0), 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            Location underTimeline = base.TurnTaker.FindCard(TimelineIdentifier).UnderLocation;
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, underTimeline, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
        }
    }
}
