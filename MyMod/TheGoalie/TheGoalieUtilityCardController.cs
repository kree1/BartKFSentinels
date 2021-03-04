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

        public static bool IsGoalposts(Card c)
        {
            return c.DoKeywordsContain(GoalpostsKeyword);
        }

        public static int NumGoalpostsAt(Location loc)
        {
            return loc.Cards.Where((Card c) => IsGoalposts(c)).Count();
        }

        public IEnumerable<Card> GoalpostsInPlay()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsGoalposts(c));
        }

        public int NumGoalpostsInPlay()
        {
            return GoalpostsInPlay().Count();
        }

        public IEnumerable<Card> GoalpostsInHeroPlayAreas()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsGoalposts(c) && c.Location.IsHero);
        }

        public int NumGoalpostsInHeroPlayAreas()
        {
            return GoalpostsInHeroPlayAreas().Count();
        }

        public IEnumerable<Card> GoalpostsInNonHeroPlayAreas()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsGoalposts(c) && !c.Location.IsHero);
        }

        public int NumGoalpostsInNonHeroPlayAreas()
        {
            return GoalpostsInNonHeroPlayAreas().Count();
        }

        public IEnumerator DestroyExcessGoalpostsResponse()
        {
            // "Then, destroy all but 2 Goalposts cards."
            int goalpostsCount = NumGoalpostsInPlay();
            if (goalpostsCount > 2)
            {
                LinqCardCriteria match = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsGoalposts(c), "goalposts");
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, match, goalpostsCount - 2, optional: false, dynamicNumberOfCards: () => NumGoalpostsInPlay() - 2, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }
    }
}
