using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public abstract class TheGoalieUtilityCardController : CardController
    {
        public TheGoalieUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public static readonly string GoalpostsKeyword = "goalposts";
        public static readonly string GoalpostsIdentifier = "PlaceOfPower";

        public static bool IsGoalposts(Card c)
        {
            return c.DoKeywordsContain(GoalpostsKeyword);
        }

        public static readonly LinqCardCriteria GoalpostsCards = new LinqCardCriteria((Card c) => IsGoalposts(c), "Goalposts");
        public static readonly LinqCardCriteria GoalpostsInPlay = new LinqCardCriteria((Card c) => IsGoalposts(c) && c.IsInPlayAndHasGameText, "Goalposts cards in play", false, false, "Goalposts card in play", "Goalposts cards in play");

        public IEnumerator FetchGoalpostsResponse()
        {
            // "Search your deck and trash for a Goalposts card and put it into play. Shuffle your deck."
            Card place = FindCard(GoalpostsIdentifier);
            if (place.IsInDeck || place.IsInTrash)
            {
                IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, place, isPutIntoPlay: true, optional: false, responsibleTurnTaker: base.TurnTaker, associateCardSource: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            IEnumerator shuffleCoroutine = ShuffleDeck(base.HeroTurnTakerController, base.TurnTaker.Deck);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            yield break;
        }
    }
}
