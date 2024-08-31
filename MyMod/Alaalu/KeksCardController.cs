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
    public class KeksCardController : AlaaluUtilityCardController
    {
        public KeksCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(AlaalidCriteria());
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, destroy all non-Myth Landmarks."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyRealLandmarksResponse, TriggerType.DestroyCard);
            // "At the start of the environment turn, play the top card of the villain deck with the fewest cards in its trash. Then, play the top card of the hero deck with the fewest cards in its trash. Then, if an Alaalid is in play, shuffle this card and the environment trash into the environment deck."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, BuildThenLeaveResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.ShuffleTrashIntoDeck, TriggerType.ShuffleCardIntoDeck });
        }

        public IEnumerator DestroyRealLandmarksResponse(GameAction ga)
        {
            // "... destroy all non-Myth Landmarks."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, NonMythLandmarkCriteria(), cardSource: GetCardSource());
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

        public IEnumerator BuildThenLeaveResponse(GameAction ga)
        {
            // "... play the top card of the villain deck with the fewest cards in its trash."
            List<TurnTaker> villainResult = new List<TurnTaker>();
            IEnumerator selectVillainCoroutine = base.GameController.DetermineTurnTakersWithMostOrFewest(false, 1, 1, (TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsVillain(tt), (TurnTaker tt) => tt.Trash.NumberOfCards, SelectionType.PlayTopCard, villainResult, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectVillainCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectVillainCoroutine);
            }
            IEnumerator villainPlayCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.GameController.FindTurnTakerController(villainResult.First()), responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(villainPlayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(villainPlayCoroutine);
            }
            // "Then, play the top card of the hero deck with the fewest cards in its trash."
            List<TurnTaker> heroResult = new List<TurnTaker>();
            IEnumerator selectHeroCoroutine = base.GameController.DetermineTurnTakersWithMostOrFewest(false, 1, 1, (TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt), (TurnTaker tt) => tt.Trash.NumberOfCards, SelectionType.PlayTopCard, heroResult, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectHeroCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectHeroCoroutine);
            }
            IEnumerator heroPlayCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.GameController.FindTurnTakerController(heroResult.First()), responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(heroPlayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(heroPlayCoroutine);
            }
            // "Then, if an Alaalid is in play, shuffle this card and the environment trash into the environment deck."
            IEnumerable<Card> alaalids = base.GameController.FindCardsWhere(AlaalidInPlayCriteria(), visibleToCard: GetCardSource());
            if (alaalids.Count() > 0)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("It looks like the " + base.Card.Title + " are done building for today.", Priority.Medium, GetCardSource(), associatedCards: alaalids);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.Trash, responsibleTurnTaker: base.TurnTaker, actionSource: ga, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
                IEnumerator shuffleCoroutine = base.GameController.ShuffleTrashIntoDeck(base.TurnTakerController, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            yield break;
        }
    }
}
