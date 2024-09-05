using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class OrbitalGuidanceCardController : DreadnoughtUtilityCardController
    {
        public OrbitalGuidanceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Mantle cards in deck
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MantleKeyword), "Mantle"));
            // Show list of Mantle cards in trash
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Trash, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MantleKeyword), "Mantle"));
        }

        public override bool DoNotMoveOneShotToTrash
        {
            // If this card is in our hand when it finishes resolving, leave it there
            get
            {
                if (Card.IsInHand)
                {
                    return true;
                }
                return false;
            }
        }

        public override IEnumerator Play()
        {
            // "Search your deck and/or trash for up to 2 Mantle cards. Put 1 into play and the rest into your hand. If you searched your deck, shuffle your deck."
            List<Card> selected = new List<Card>();
            bool searchedDeck = false;
            for (int i = 0; i < 2; i++)
            {
                // Choose deck or trash (optional)
                List<SelectLocationDecision> decisions = new List<SelectLocationDecision>();
                IEnumerator chooseCoroutine = GameController.SelectLocation(DecisionMaker, new List<LocationChoice> { new LocationChoice(TurnTaker.Deck), new LocationChoice(TurnTaker.Trash) }, SelectionType.SearchLocation, decisions, true, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(chooseCoroutine);
                }
                if (!DidSelectLocation(decisions))
                {
                    break;
                }
                // Choose a Mantle card from that location, reveal it, and add it to selected
                Location chosen = GetSelectedLocation(decisions);
                if (chosen == TurnTaker.Deck)
                    searchedDeck = true;
                List<SelectCardDecision> cardDecisions = new List<SelectCardDecision>();
                IEnumerator searchCoroutine = GameController.SelectAndMoveCard(DecisionMaker, (Card c) => GameController.DoesCardContainKeyword(c, MantleKeyword) && c.Location == chosen, TurnTaker.Revealed, optional: true, storedResults: cardDecisions, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(searchCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(searchCoroutine);
                }
                if (DidSelectCard(cardDecisions))
                {
                    selected.Add(GetSelectedCard(cardDecisions));
                }
            }
            if (selected.Any())
            {
                // Choose a card from selected and put it into play
                List<SelectCardDecision> toMove = new List<SelectCardDecision>();
                IEnumerator moveCoroutine = GameController.SelectAndMoveCard(DecisionMaker, (Card c) => selected.Contains(c), TurnTaker.PlayArea, isPutIntoPlay: true, storedResults: toMove, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(moveCoroutine);
                }
                if (DidSelectCard(toMove))
                {
                    selected.Remove(GetSelectedCard(toMove));
                }
                // Put the rest of selected into hand
                IEnumerator restCoroutine = GameController.MoveCards(DecisionMaker, selected, HeroTurnTaker.Hand, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(restCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(restCoroutine);
                }
            }
            // If the deck was chosen at any point, shuffle it
            if (searchedDeck)
            {
                IEnumerator shuffleCoroutine = GameController.ShuffleLocation(TurnTaker.Deck, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            // "{Dreadnought} may deal 1 target 2 irreducible melee damage."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 2, DamageType.Melee, 1, false, 0, storedResultsDamage: damageResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            if (DidDealDamage(damageResults))
            {
                // "If she dealt damage this way, discard the top card of your deck."
                IEnumerator discardCoroutine = GameController.DiscardTopCard(TurnTaker.Deck, null, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            else
            {
                // "If not, return this card to your hand."
                IEnumerator returnCoroutine = GameController.MoveCard(DecisionMaker, Card, HeroTurnTaker.Hand, showMessage: true, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(returnCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(returnCoroutine);
                }
            }
        }
    }
}
