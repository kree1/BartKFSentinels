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
            // "Search your deck and/or trash for up to 2 Mantle cards and put them into your hand. If you searched your deck, shuffle your deck."
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
                // Choose a Mantle card from that location and put it into your hand
                Location chosen = GetSelectedLocation(decisions);
                if (chosen == TurnTaker.Deck)
                    searchedDeck = true;
                List<SelectCardDecision> cardDecisions = new List<SelectCardDecision>();
                IEnumerator searchCoroutine = GameController.SelectAndMoveCard(DecisionMaker, (Card c) => GameController.DoesCardContainKeyword(c, MantleKeyword) && c.Location == chosen, HeroTurnTaker.Hand, optional: true, storedResults: cardDecisions, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(searchCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(searchCoroutine);
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
            // "You may play a Mantle card."
            IEnumerator playCoroutine = GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardCriteria: new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MantleKeyword), "Mantle"), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
            // "{Dreadnought} may deal 1 target 2 irreducible melee damage."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 2, DamageType.Melee, 1, false, 0, isIrreducible: true, storedResultsDamage: damageResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            if (!DidDealDamage(damageResults))
            {
                // "If she dealt no damage this way, return this card to your hand."
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
