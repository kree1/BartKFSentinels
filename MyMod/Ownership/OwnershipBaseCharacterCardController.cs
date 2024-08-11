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
            string markerLocation = MapLocationDescription(location, FindCard(MapCardIdentifier).IsFlipped);
            output = markerName + " is at " + markerLocation + ".";
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

        public string MapLocationDescription(int[] markerLocation, bool endZones = false)
        {
            if (markerLocation.Length < 2 || !(markerLocation[0] >= 0 && markerLocation[0] <= 4 && markerLocation[1] >= 0 && markerLocation[1] <= 4))
            {
                return "Error: invalid location array";
            }
            string desc = "row " + markerLocation[0] + ", column " + markerLocation[1] + ", ";
            if (endZones)
            {
                // Describe using the End Zones map
                string horizon = "the Horizon (bottom left corner)";
                string desert = "the Desert (bottom right corner)";
                string coin = "Ownership's space (center)";
                string vault = "the Vault (top left corner)";
                string hall = "the Hall (top right corner)";
                string inSpace = "in ";
                string above = "just above ";
                string below = "just below ";
                string leftOf = "just left of ";
                string rightOf = "just right of ";
                string between = "diagonally between ";
                string and = " and ";
                switch (markerLocation[0])
                {
                    case 0:
                        switch (markerLocation[1])
                        {
                            case 0:
                                desc += inSpace + horizon;
                                break;
                            case 1:
                                desc += rightOf + horizon;
                                break;
                            case 2:
                                desc += "in the middle of the bottom row";
                                break;
                            case 3:
                                desc += leftOf + desert;
                                break;
                            default:
                                desc += inSpace + desert;
                                break;
                        }
                        break;
                    case 1:
                        switch (markerLocation[1])
                        {
                            case 0:
                                desc += above + horizon;
                                break;
                            case 1:
                                desc += between + horizon + and + coin;
                                break;
                            case 2:
                                desc += below + coin;
                                break;
                            case 3:
                                desc += between + desert + and + coin;
                                break;
                            default:
                                desc += above + desert;
                                break;
                        }
                        break;
                    case 2:
                        switch (markerLocation[1])
                        {
                            case 0:
                                desc += "in the middle of the leftmost column";
                                break;
                            case 1:
                                desc += leftOf + coin;
                                break;
                            case 2:
                                desc += inSpace + coin;
                                break;
                            case 3:
                                desc += rightOf + coin;
                                break;
                            default:
                                desc += "in the middle of the rightmost column";
                                break;
                        }
                        break;
                    case 3:
                        switch (markerLocation[1])
                        {
                            case 0:
                                desc += below + vault;
                                break;
                            case 1:
                                desc += between + vault + and + coin;
                                break;
                            case 2:
                                desc += above + coin;
                                break;
                            case 3:
                                desc += between + hall + and + coin;
                                break;
                            default:
                                desc += below + hall;
                                break;
                        }
                        break;
                    default:
                        switch (markerLocation[1])
                        {
                            case 0:
                                desc += inSpace + vault;
                                break;
                            case 1:
                                desc += rightOf + vault;
                                break;
                            case 2:
                                desc += "in the middle of the top row";
                                break;
                            case 3:
                                desc += leftOf + hall;
                                break;
                            default:
                                desc += inSpace + hall;
                                break;
                        }
                        break;
                }
            }
            else
            {
                // Describe using the Depth Chart
                switch (markerLocation[0])
                {
                    case 0:
                        desc += "at the bottom";
                        break;
                    case 1:
                        desc += "just below the yellow line";
                        break;
                    case 2:
                        desc += "just above the yellow line";
                        break;
                    default:
                        desc += "above the yellow line";
                        break;
                }
            }
            return desc;
        }

        public IEnumerator MoveHeroMarker(int heroIndex, int up, int right, TurnTaker responsibleTurnTaker = null, bool showMessage = false, CardSource cardSource = null)
        {
            //Log.Debug("OwnershipBaseCharacterCardController.MoveHeroMarker called with heroIndex " + heroIndex.ToString() + ", up " + up.ToString() + ", right " + right.ToString());
            if (heroIndex >= 1 && heroIndex <= 5 && !(up == 0 && right == 0))
            {
                TokenPool rowPool = FindCard(MapCardIdentifier).FindTokenPool(LocationPoolIdentifier(heroIndex, false));
                TokenPool columnPool = FindCard(MapCardIdentifier).FindTokenPool(LocationPoolIdentifier(heroIndex, true));
                // Move vertically first
                if (up > 0)
                {
                    //Log.Debug("OwnershipBaseCharacterCardController.MoveHeroMarker adding " + up.ToString() + " tokens to " + rowPool.Name);
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
                    //Log.Debug("OwnershipBaseCharacterCardController.MoveHeroMarker removing " + (-1 * up).ToString() + " tokens from " + rowPool.Name);
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
                    //Log.Debug("OwnershipBaseCharacterCardController.MoveHeroMarker adding " + right.ToString() + " tokens to " + columnPool.Name);
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
                    //Log.Debug("OwnershipBaseCharacterCardController.MoveHeroMarker removing " + (-1 * right).ToString() + " tokens from " + columnPool.Name);
                    IEnumerator leftCoroutine = base.GameController.RemoveTokensFromPool(columnPool, -1 * right, cardSource: GetCardSource());
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
