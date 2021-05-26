using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class DanPeanutReplacementCardController : CardController
    {
        public DanPeanutReplacementCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroWithMostCards(true).Condition = () => !base.Card.IsInPlay;
            SpecialStringMaker.ShowNumberOfCardsAtLocations(() => new Location[] { base.TurnTaker.Deck, base.TurnTaker.Trash }, new LinqCardCriteria((Card c) => c.DoKeywordsContain("batter"), "Batters", false, false, "Batter", "Batters"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt by that hero to villain targets by 1."
            AddReduceDamageTrigger((DealDamageAction dda) => dda.Target.IsVillainTarget && dda.DamageSource != null && dda.DamageSource.Card == GetCardThisCardIsNextTo(), (DealDamageAction dda) => 1);
            // "At the end of that hero's turn, if {TheShelledOne} is a target, discard cards from the villain deck until a Pod is discarded."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == GetCardThisCardIsNextTo().Owner && base.CharacterCard.IsTarget, DiscardBatterResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.PlayCard });
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "When this card enters play, move it next to the hero with the most cards in hand."
            List<TurnTaker> biggestHands = new List<TurnTaker>();
            IEnumerator findCoroutine = FindHeroWithMostCardsInHand(biggestHands, ranking: 1, numberOfHeroes: 1, evenIfCannotDealDamage: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            TurnTaker player = biggestHands.FirstOrDefault();
            if (player != null && storedResults != null)
            {
                List<Card> characterChoice = new List<Card>();
                IEnumerator chooseCoroutine = FindCharacterCard(player, SelectionType.MoveCardNextToCard, characterChoice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
                Card chosen = characterChoice.FirstOrDefault();
                if (chosen != null)
                {
                    storedResults.Add(new MoveCardDestination(chosen.NextToLocation));
                }
            }
        }

        public IEnumerator DiscardBatterResponse(GameAction ga)
        {
            // "... discard cards from the villain deck until a Batter is discarded."
            if (base.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("batter", true, true) && c.Location.IsVillain && (c.Location.IsDeck || c.Location.IsTrash)).Any())
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " brings forth another player from the Pods...", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                List<MoveCardAction> moves = new List<MoveCardAction>();
                while (!moves.Where((MoveCardAction mca) => mca.IsDiscard && mca.CardToMove != null && mca.CardToMove.DoKeywordsContain("pod")).Any())
                {
                    IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, moves, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(discardCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(discardCoroutine);
                    }
                }
            }
            else
            {
                // If there are already no Pods in the deck or trash (e.g. because all of them are in play), don't loop infinitely- just discard the deck and send a message
                while(base.TurnTaker.Deck.HasCards)
                {
                    IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, null, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(discardCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(discardCoroutine);
                    }
                }
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are no more Pods for " + base.Card.Title + " to call on.", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }
    }
}
