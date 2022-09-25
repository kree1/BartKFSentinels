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
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsRelay(c) && c.Location.HighestRecursiveLocation.IsHero);
        }

        public int NumRelaysInHeroPlayAreas()
        {
            return RelaysInHeroPlayAreas().Count();
        }

        public IEnumerable<Card> RelaysInNonHeroPlayAreas()
        {
            return base.TurnTaker.GetCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsRelay(c) && !c.Location.HighestRecursiveLocation.IsHero);
        }

        public int NumRelaysInNonHeroPlayAreas()
        {
            return RelaysInNonHeroPlayAreas().Count();
        }

        public SpecialString ShowListOfCardsAtLocationOfCardRecursive(Card cardToCheck, LinqCardCriteria cardCriteria, Func<bool> showInEffectsList = null)
        {
            Func<string> output = () => StringForListOfCards(new LinqCardCriteria((Card c) => c.Location.HighestRecursiveLocation == cardToCheck.Location.HighestRecursiveLocation && cardCriteria.Criteria(c), cardCriteria.Description), GetLocationOutput(cardToCheck.Location, specifyPlayAreas: true));
            return SpecialStringMaker.ShowSpecialString(output, showInEffectsList);
        }

        public SpecialString ShowListOfCardsAtLocationRecursive(Location location, LinqCardCriteria cardCriteria, Func<bool> showInEffectsList = null)
        {
            Func<string> output = () => StringForListOfCards(new LinqCardCriteria((Card c) => c.Location.HighestRecursiveLocation == location.HighestRecursiveLocation && cardCriteria.Criteria(c), cardCriteria.Description), GetLocationOutput(location, specifyPlayAreas: true));
            return SpecialStringMaker.ShowSpecialString(output, showInEffectsList);
        }

        public string StringForListOfCards(LinqCardCriteria cardCriteria, string where = null, bool consolodateDuplicates = true)
        {
            IEnumerable<Card> source = this.FindCardsWhere((Card c) => cardCriteria.Criteria(c));
            int num = source.Count();
            if (where != null)
            {
                where = " " + where;
            }
            if (num == 0)
            {
                int number = this.FindCardsWhere((Card c) => c.IsInPlay && cardCriteria.Criteria(c)).Count();
                return "There " + number.ToString_NumberOfCards(cardCriteria.GetDescription(plural: true, false), cardCriteria.UseCardsSuffix) + where + ".";
            }
            List<string> list = new List<string>();
            IEnumerable<string> source2 = source.Select((Card c) => c.AlternateTitleOrTitle);
            if (consolodateDuplicates)
            {
                foreach (string distinctTitle in source2.Distinct())
                {
                    int num2 = source2.Where((string s) => s == distinctTitle).Count();
                    string text = distinctTitle;
                    if (num2 > 1)
                    {
                        text = text + " (x" + num2 + ")";
                    }
                    list.Add(text);
                }
            }
            else
            {
                list = source2.ToList();
            }
            return cardCriteria.GetDescription().Capitalize() + where + ": " + list.ToCommaList(useWordAnd: true) + ".";
        }

        public string GetLocationOutput(Location location, bool specifyPlayAreas)
        {
            string text = location.GetFriendlyName();
            if (location.Name == LocationName.PlayArea)
            {
                if (specifyPlayAreas)
                {
                    string name = location.OwnerTurnTaker.Name;
                    text = ((!name.EndsWith("s")) ? (name + "'s play area") : (name + "' play area"));
                }
                else
                {
                    text = "play";
                }
            }
            if (location.Name != LocationName.NextToCard && location.Name != LocationName.UnderCard)
            {
                text = "in " + text;
            }
            return text;
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
