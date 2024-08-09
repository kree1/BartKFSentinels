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
    public class TeamModCardController : OwnershipUtilityCardController
    {
        public TeamModCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero play area with fewest Modification cards
            ShowTurnTakerWithMostOrFewestCards("Hero play area", "Hero play areas", false, false, true, (TurnTaker tt) => IsHero(tt), ModificationCriteria());
        }

        public readonly string ModificationKeyword = "modification";

        public SpecialString ShowTurnTakerWithMostOrFewestCards(string singular, string plural, bool most, bool inHand, bool includeUnownedCards, Func<TurnTaker, bool> ttCriteria = null, LinqCardCriteria additionalCriteria = null, Func<bool> showInEffectsList = null)
        {
            if (ttCriteria == null)
            {
                ttCriteria = (TurnTaker tt) => true;
            }
            Func<string> output = delegate
            {
                IEnumerable<TurnTaker> turnTakers = base.GameController.FindTurnTakersWhere(ttCriteria, base.BattleZone);
                List<string> mostFewestNames = new List<string>();
                int? currentExtreme = null;
                foreach (TurnTaker tt in turnTakers)
                {
                    //Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: checking " + tt.Name);
                    IEnumerable<Card> ttCardsAtLocations = from c in tt.GetAllCards(realCardsOnly: true, includeUnownedCards).Distinct()
                                                           where (!inHand || c.IsInHand) && (inHand || c.IsInPlay) && c.Location.OwnerTurnTaker == tt
                                                           select c;
                    List<Card> ttRelevantCardsAtLocations = ((additionalCriteria == null) ? ttCardsAtLocations.ToList() : ttCardsAtLocations.Where(additionalCriteria.Criteria).ToList());
                    int ttValue = ttRelevantCardsAtLocations.Count();
                    //Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: ttRelevantCardsAtLocations.Count(): " + ttRelevantCardsAtLocations.Count());
                    //Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: listing ttRelevantCardsAtLocations for " + tt.Name);
                    /*foreach(Card c in ttRelevantCardsAtLocations)
                    {
                        Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: " + c.Title + " is in " + c.Location.GetFriendlyName() + " (IsPlayArea: " + c.Location.IsPlayArea + ", IsPlayAreaOf(tt): " + c.Location.IsPlayAreaOf(tt) + ")");
                    }*/
                    //Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: currentExtreme: " + currentExtreme);
                    if (ttValue == currentExtreme)
                    {
                        //Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: count matches currentExtreme, adding " + tt.Name + " to list");
                        mostFewestNames.Add(tt.Name);
                    }
                    else if ((most && ttValue > currentExtreme.GetValueOrDefault(0)) || (!most && ttValue < currentExtreme.GetValueOrDefault(int.MaxValue)))
                    {
                        //Log.Debug("TeamModCardController.ShowTurnTakerWithMostOrFewestCards: count defeats currentExtreme, replacing list with " + tt.Name + " and updating currentExtreme");
                        mostFewestNames.RemoveAll((string htt) => true);
                        mostFewestNames.Add(tt.Name);
                        currentExtreme = ttValue;
                    }
                }
                string turnTakerDesc = mostFewestNames.Count().ToString_SingularOrPlural(singular, plural);
                string locationDesc = ((!inHand) ? " in play" : " in hand");
                string cardDesc = " cards";
                if (additionalCriteria != null)
                {
                    cardDesc = " " + additionalCriteria.GetDescription();
                }
                return mostFewestNames.Any() ? string.Format("{0} with the {4}{3}{2}: {1}.", turnTakerDesc, mostFewestNames.ToRecursiveString(), locationDesc, cardDesc, most ? "most" : "fewest") : $"No {plural} have any {cardDesc}{locationDesc}.";
            };
            return SpecialStringMaker.ShowSpecialString(output, showInEffectsList);
        }

        public LinqCardCriteria ModificationCriteria()
        {
            return new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(ModificationKeyword), "Modification");
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in the hero play area with the fewest Modification cards."
            List<TurnTaker> results = new List<TurnTaker>();
            IEnumerator findCoroutine = FindHeroWithFewestCardsInPlayArea(results, cardCriteria: ModificationCriteria(), evenIfCannotDealDamage: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            TurnTaker hero = results.FirstOrDefault();
            if (hero != null)
            {
                storedResults?.Add(new MoveCardDestination(hero.PlayArea, showMessage: true));
            }
        }
    }
}
