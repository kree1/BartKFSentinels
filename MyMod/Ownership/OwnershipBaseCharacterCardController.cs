using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class OwnershipBaseCharacterCardController : VillainCharacterCardController
    {
        public OwnershipBaseCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public string DisplayMarkerLocation(int heroIndex)
        {
            string output = "";
            HeroTurnTakerController httc = HTTCAtIndex(heroIndex);
            string markerName = httc.TurnTaker.Name + "'s marker";
            if (httc.TurnTaker.DeckDefinition.IsPlural)
            {
                markerName = httc.TurnTaker.Name + "' marker";
            }
            int[] location = HeroMarkerLocation(heroIndex);
            string markerLocation = "row " + location[0] + ", column " + location[1];
            string punctuation = ".";
            if (base.Card.IsFlipped)
            {
                // Add extra detail to markerLocation describing it relative to important landmarks?
                // ...
            }
            else
            {
                if (location[0] >= 2)
                {
                    markerLocation += ", above the yellow line";
                }
                else
                {
                    markerLocation += ", ";
                    if (location[0] == 1)
                    {
                        markerLocation += "just ";
                    }
                    markerLocation += "below the yellow line";
                }
            }
            output = markerName + " is at " + markerLocation + punctuation;
            return output;
        }

        public readonly string OwnershipIdentifier = "OwnershipCharacter";
        public readonly string MapCardIdentifier = "MapCharacter";
        public readonly string StatCardIdentifier = "StatCharacter";
        public readonly string WeightPoolIdentifier = "StatCardWeightPool";
        public readonly string SunSunIdentifier = "SunSun";
        public readonly string StatKeyword = "stat";

        public HeroTurnTakerController HTTCAtIndex(int heroIndex)
        {
            // This list will be 0-indexed but heroIndex assumes the first hero is hero #1
            if (heroIndex >= 1 && heroIndex <= H)
                return base.GameController.HeroTurnTakerControllers.ElementAt(heroIndex - 1);
            else
                return null;
        }

        public int IndexOfHero(HeroTurnTakerController hero)
        {
            int? index = base.GameController.HeroTurnTakerControllers.IndexOf(hero);
            if (index.HasValue)
                return index.Value + 1;
            return -1;
        }

        public string HeroNameAtIndex(int heroIndex)
        {
            string title = "n/a";
            List<string> names = (from ttc in base.GameController.HeroTurnTakerControllers select ttc.TurnTaker.Name).ToList();
            if (heroIndex >= 1 && heroIndex <= names.Count())
            {
                // This list will be 0-indexed but heroIndex assumes the first hero is hero #1
                title = names[heroIndex - 1];
            }
            return title;
        }

        public Card StatCardOf(HeroTurnTakerController hero)
        {
            return base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == StatCardIdentifier && c.Location.IsPlayAreaOf(hero.TurnTaker)), visibleToCard: GetCardSource()).FirstOrDefault();
        }

        public Card StatCardOf(TurnTaker tt)
        {
            return StatCardOf(base.GameController.FindHeroTurnTakerController(tt.ToHero()));
        }

        public int HeroIndexOfPool(TokenPool pool)
        {
            if (pool != null && pool.CardWithTokenPool.Identifier == MapCardIdentifier)
            {
                string indexAsText = pool.Identifier.Substring(4, 1);
                if (int.TryParse(indexAsText, out int index))
                {
                    return index;
                }
            }
            return -1;
        }

        public string LocationPoolIdentifier(int heroIndex, bool column = false)
        {
            string id = "Hero";
            if (heroIndex >= 1 && heroIndex <= 5)
            {
                id += heroIndex.ToString();
            }
            if (column)
            {
                id += "Column";
            }
            else
            {
                id += "Row";
            }
            return id;
        }

        public int[] HeroMarkerLocation(int heroIndex)
        {
            int row = FindCard(MapCardIdentifier).FindTokenPool(LocationPoolIdentifier(heroIndex, false)).CurrentValue;
            int col = FindCard(MapCardIdentifier).FindTokenPool(LocationPoolIdentifier(heroIndex, true)).CurrentValue;
            return new int[] { row, col };
        }

        public IEnumerator MoveHeroMarker(int heroIndex, int up, int right, TurnTaker responsibleTurnTaker = null, bool showMessage = false, CardSource cardSource = null)
        {
            if (heroIndex >= 1 && heroIndex <= 5 && !(up == 0 && right == 0))
            {
                TokenPool rowPool = FindCard(MapCardIdentifier).FindTokenPool(LocationPoolIdentifier(heroIndex, false));
                TokenPool columnPool = FindCard(MapCardIdentifier).FindTokenPool(LocationPoolIdentifier(heroIndex, true));
                // Move vertically first
                if (up > 0)
                {
                    IEnumerator upCoroutine = base.GameController.AddTokensToPool(rowPool, up, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(upCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(upCoroutine);
                    }
                }
                else if (up < 0)
                {
                    IEnumerator downCoroutine = base.GameController.RemoveTokensFromPool(rowPool, -1 * up, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(downCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(downCoroutine);
                    }
                }
                // Move horizontally second
                if (right > 0)
                {
                    IEnumerator rightCoroutine = base.GameController.AddTokensToPool(columnPool, right, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(rightCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(rightCoroutine);
                    }
                }
                else if (right < 0)
                {
                    IEnumerator leftCoroutine = base.GameController.AddTokensToPool(columnPool, -1 * right, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(leftCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(leftCoroutine);
                    }
                }
                // Finally, report the move
                if (!showMessage)
                {
                    yield break;
                }
                int[] location = HeroMarkerLocation(heroIndex);
                string hero = HTTCAtIndex(heroIndex).TurnTaker.Name;
                string marker = hero + "'s marker";
                if (HTTCAtIndex(heroIndex).TurnTaker.DeckDefinition.IsPlural)
                {
                    marker = hero + "' marker";
                }
                string dest = "row " + location[0].ToString() + ", column " + location[1].ToString();
                string message = marker + " was moved to " + dest;
                if (cardSource != null)
                {
                    if (cardSource.Card == FindCard(MapCardIdentifier))
                    {
                        if (responsibleTurnTaker != null)
                        {
                            message = responsibleTurnTaker.Name + " moved " + marker + " to " + dest;
                        }
                    }
                    else if (cardSource.Card.Identifier == StatCardIdentifier)
                    {
                        if (cardSource.Card.Location.OwnerTurnTaker.Name == hero)
                        {
                            message = hero + "'s Stat card moved their marker to " + dest;
                        }
                        else
                        {
                            message = cardSource.Card.Location.OwnerTurnTaker.Name + "'s Stat card moved " + marker + " to " + dest;
                        }
                    }
                    else
                    {
                        message = cardSource.Card.Title + " moved " + marker + " to " + dest;
                    }
                }
                message += ".";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, cardSource);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
        }
    }
}
