﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class MoveTheGoalpostsCardController : TheGoalieUtilityCardController
    {
        public MoveTheGoalpostsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsGoalposts(c), "goalposts"));
        }

        public override IEnumerator Play()
        {
            // "Search your deck for a Goalposts card and put it into your hand. Shuffle your deck."
            LinqCardCriteria match = new LinqCardCriteria((Card c) => IsGoalposts(c));
            IEnumerator searchCoroutine = base.SearchForCards(base.HeroTurnTakerController, true, false, new int?(1), 1, match, false, true, false, optional: false, shuffleAfterwards: new bool?(true));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(searchCoroutine);
            }
            // "You may destroy one of your Ongoing cards."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsOngoing && c.Owner == base.TurnTaker, "ongoing"), true, storedResultsAction: destroyed, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            if (destroyed != null && destroyed.Count() > 0 && DidDestroyCard(destroyed.First()))
            {
                // "If you do, you may play an Ongoing card."
                IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, optional: true, cardCriteria: new LinqCardCriteria((Card c) => c.IsOngoing));
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }
    }
}