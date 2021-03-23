using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class TheGigamorphUtilityCardController : CardController
    {
        public TheGigamorphUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public bool IsTagged(Card c)
        {
            //Log.Debug("Cards next to " + c.Title + ": " + (from cN in c.NextToLocation.Cards select cN.Title).ToCommaList());
            if (c.NextToLocation.Cards.Any((Card c2) => c2.DoKeywordsContain("antibody") && c2.IsEnvironment))
            {
                //Log.Debug(c.Title + " is Tagged!");
                return true;
            }
            //Log.Debug(c.Title + " is not Tagged.");
            return false;
        }

        public IEnumerator FetchAntibody(GameAction ga)
        {
            // "... search the environment deck and trash for an Antibody card and put it into play. Then, shuffle the environment deck."
            if (base.TurnTaker.Deck.Cards.Any((Card c) => c.DoKeywordsContain("antibody")) || base.TurnTaker.Trash.Cards.Any((Card c) => c.DoKeywordsContain("antibody")))
            {
                IEnumerable<Card> allCards = base.TurnTaker.Deck.Cards.Concat(base.TurnTaker.Trash.Cards);
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(base.DecisionMaker, allCards.Where((Card c) => c.DoKeywordsContain("antibody")), isPutIntoPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                IEnumerator shuffleCoroutine = ShuffleDeck(base.DecisionMaker, base.TurnTaker.Deck);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            else
            {
                IEnumerator showCoroutine = base.GameController.SendMessageAction("There are no Antibodies in the environment deck or trash to put into play.", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
            }
            yield break;
        }
    }
}
