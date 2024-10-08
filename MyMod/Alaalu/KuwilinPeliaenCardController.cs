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
    public class KuwilinPeliaenCardController : AlaaluUtilityCardController
    {
        public KuwilinPeliaenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(LivestockCriteria());
            SpecialStringMaker.ShowLowestHP(1, () => FindCardsWhere(LivestockInPlayCriteria(), visibleToCard: GetCardSource()).Count() + 1, new LinqCardCriteria((Card c) => IsHeroTarget(c), "hero", singular: "target", plural: "targets"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, the X hero targets with the lowest HP regain 2 HP each, where X is 1 plus the number of Livestock in play. Then, X players each put a card from their trash on the bottom of their deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealMoveResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.MoveCard });
        }

        public IEnumerator HealMoveResponse(GameAction ga)
        {
            // "... the X hero targets with the lowest HP regain 2 HP each, where X is 1 plus the number of Livestock in play."
            List<Card> lowestHeroes = new List<Card>();
            GainHPAction gameAction = new GainHPAction(GetCardSource(), null, 2, null);
            IEnumerator findCoroutine = base.GameController.FindTargetsWithLowestHitPoints(1, base.GameController.FindCardsWhere(LivestockInPlayCriteria(), visibleToCard: GetCardSource()).Count() + 1, (Card c) => IsHeroTarget(c), lowestHeroes, gameAction: gameAction, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            IEnumerator healCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => lowestHeroes.Contains(c), 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "Then, X players each put a card from their trash on the bottom of their deck."
            IEnumerator massMoveCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt)), SelectionType.MoveCard, MoveFromTrashToDeckResponse, numberOfTurnTakers: base.GameController.FindCardsWhere(LivestockInPlayCriteria(), visibleToCard: GetCardSource()).Count() + 1, requiredDecisions: base.GameController.FindCardsWhere(LivestockInPlayCriteria(), visibleToCard: GetCardSource()).Count() + 1, allowAutoDecide: base.GameController.FindCardsWhere(LivestockInPlayCriteria(), visibleToCard: GetCardSource()).Count() + 1 >= base.GameController.AllHeroes.Count(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(massMoveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(massMoveCoroutine);
            }
        }

        public IEnumerator MoveFromTrashToDeckResponse(TurnTaker tt)
        {
            // "... put a card from their trash on the bottom of their deck."
            IEnumerator moveCoroutine = base.GameController.SelectAndMoveCard(base.GameController.FindHeroTurnTakerController(tt.ToHero()), (Card c) => c.IsInLocation(tt.Trash), tt.Deck, toBottom: true, cardSource: GetCardSource());
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
