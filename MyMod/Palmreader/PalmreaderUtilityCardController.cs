using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Palmreader
{
    public abstract class PalmreaderUtilityCardController : CardController
    {
        public PalmreaderUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public static readonly string RelayKeyword = "relay";

        public static bool IsRelay(Card c)
        {
            return c.DoKeywordsContain(RelayKeyword);
        }

        public static int NumRelaysAt(Location loc)
        {
            return loc.Cards.Where((Card c) => IsRelay(c)).Count();
        }

        public IEnumerable<Card> RelaysInPlay()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsRelay(c));
        }

        public int NumRelaysInPlay()
        {
            return RelaysInPlay().Count();
        }

        public IEnumerable<Card> RelaysInHeroPlayAreas()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsRelay(c) && c.Location.IsHero);
        }

        public int NumRelaysInHeroPlayAreas()
        {
            return RelaysInHeroPlayAreas().Count();
        }

        public IEnumerable<Card> RelaysInNonHeroPlayAreas()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsRelay(c) && !c.Location.IsHero);
        }

        public int NumRelaysInNonHeroPlayAreas()
        {
            return RelaysInNonHeroPlayAreas().Count();
        }

        public IEnumerator DestroyExcessRelaysResponse()
        {
            // "Then, destroy all but 2 Relay cards."
            int goalpostsCount = NumRelaysInPlay();
            if (goalpostsCount > 2)
            {
                LinqCardCriteria match = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsRelay(c), "relay");
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, match, goalpostsCount - 2, optional: false, dynamicNumberOfCards: () => NumRelaysInPlay() - 2, responsibleCard: base.Card, cardSource: GetCardSource());
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
