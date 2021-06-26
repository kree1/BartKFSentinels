using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class TheWhisperingCardController : CardController
    {
        public TheWhisperingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, put a villain target from the villain trash into play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, RetrieveVillainTrashResponse, TriggerType.PutIntoPlay);
            // "At the start of the environment turn, put the top card of a hero trash into play. If that card is a One-Shot, deal the associated hero (H) minus 1 psychic damage and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, RetrieveHeroTrashResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        public IEnumerator RetrieveVillainTrashResponse(GameAction ga)
        {
            // "... put a villain target from the villain trash into play."
            IEnumerable<Card> options = FindCardsWhere((Card c) => c.IsVillainTarget && c.Location.IsVillain && c.Location.IsTrash);
            if (options.Count() == 1)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " returns " + options.First().Title + " from the villain trash to play!", Priority.Low, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            List<SelectLocationDecision> locationResults = new List<SelectLocationDecision>();
            IEnumerator chooseTrashCoroutine = FindVillainDeck(DecisionMaker, SelectionType.PutIntoPlay, locationResults, (Location deck) => FindTrashFromDeck(deck) != null && FindTrashFromDeck(deck).Cards.Any((Card c) => c.IsVillainTarget));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseTrashCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseTrashCoroutine);
            }
            Location selectedLocation = GetSelectedLocation(locationResults);
            if (selectedLocation != null)
            {
                Location trash = FindTrashFromDeck(selectedLocation);
                IEnumerator putCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, (Card c) => c.IsVillainTarget && c.Location == trash, isPutIntoPlay: true, cardSource: GetCardSource(), noValidCardsMessage: "There are no villain targets in " + trash.GetFriendlyName() + " to put into play.");
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(putCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(putCoroutine);
                }
            }
            else
            {
                IEnumerator failCoroutine = base.GameController.SendMessageAction("There are no villain targets in any villain trash to put into play.", Priority.Low, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(failCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(failCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator RetrieveHeroTrashResponse(GameAction ga)
        {
            // "... put the top card of a hero trash into play."
            List<PlayCardAction> played = new List<PlayCardAction>();
            IEnumerator putCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, (Card c) => c.Location.IsTrash && c.Location.IsHero && base.GameController.IsLocationVisibleToSource(c.Location, GetCardSource()) && c == c.Location.TopCard, isPutIntoPlay: true, cardSource: GetCardSource(), storedResults: played);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "If that card is a One-Shot..."
            if (DidPlayCards(played))
            {
                PlayCardAction successful = played.FirstOrDefault((PlayCardAction pca) => pca.WasCardPlayed);
                if (successful != null && successful.CardToPlay != null && successful.CardToPlay.DoKeywordsContain("one-shot"))
                {
                    // "... deal the associated hero (H) minus 1 psychic damage..."
                    if (successful.CardToPlay.IsHero)
                    {
                        List<Card> targets = new List<Card>();
                        if (base.GameController.FindHeroTurnTakerController(successful.CardToPlay.Owner.ToHero()).HasMultipleCharacterCards)
                        {
                            IEnumerator findCoroutine = FindCharacterCardToTakeDamage(successful.CardToPlay.Owner, targets, base.Card, H - 1, DamageType.Psychic);
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(findCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(findCoroutine);
                            }
                        }
                        else
                        {
                            targets.Add(successful.CardToPlay.Owner.CharacterCard);
                        }
                        IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => targets.Contains(c), H - 1, DamageType.Psychic, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(damageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(damageCoroutine);
                        }
                    }
                    // "... and destroy this card."
                    IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destructCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destructCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
