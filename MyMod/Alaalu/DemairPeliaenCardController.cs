﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class DemairPeliaenCardController : CardController
    {
        public DemairPeliaenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.DoKeywordsContain("alaalid"), "Alaalid"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, the X players with the fewest cards in hand each draw a card, where X is the number of Alaalids in play. Then, each player with 4 or more non-character cards in play returns 1 of those cards to their hand."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DrawRecallResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.MoveCard });
        }

        public IEnumerator DrawRecallResponse(GameAction ga)
        {
            // "... the X players with the fewest cards in hand each draw a card, where X is the number of Alaalids in play."
            List<TurnTaker> fewestResults = new List<TurnTaker>();
            IEnumerator findCoroutine = base.GameController.DetermineTurnTakersWithMostOrFewest(false, 1, base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("alaalid"), "Alaalid"), visibleToCard: GetCardSource()).Count(), (TurnTaker tt) => tt.IsHero, (TurnTaker tt) => tt.ToHero().Hand.NumberOfCards, SelectionType.DrawCard, fewestResults, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            IEnumerator drawCoroutine = EachPlayerDrawsACard((HeroTurnTaker htt) => fewestResults.Contains(htt as TurnTaker));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Then, each player with 4 or more non-character cards in play returns 1 of those cards to their hand."
            IEnumerable<TurnTaker> overloadedPlayers = base.GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && base.GameController.FindCardsWhere((Card c) => c.IsInPlay && c.Owner == tt && !c.IsCharacter).Count() >= 4);
            IEnumerator massReturnCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => overloadedPlayers.Contains(tt)), SelectionType.ReturnToHand, ReturnToHandResponse, allowAutoDecide: true, numberOfCards: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(massReturnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(massReturnCoroutine);
            }
            yield break;
        }

        public IEnumerator ReturnToHandResponse(TurnTaker tt)
        {
            // "... returns 1 of [their non-character cards in play] to their hand."
            IEnumerator returnCoroutine = base.GameController.SelectAndMoveCard(base.GameController.FindHeroTurnTakerController(tt.ToHero()), (Card c) => c.IsInPlay && !c.IsCharacter && c.Owner == tt, tt.ToHero().Hand, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            yield break;
        }
    }
}
