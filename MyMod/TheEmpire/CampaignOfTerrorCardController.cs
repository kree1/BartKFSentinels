using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class CampaignOfTerrorCardController : EmpireUtilityCardController
    {
        public CampaignOfTerrorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            LinqCardCriteria imperialTarget = new LinqCardCriteria((Card c) => c.IsTarget && c.DoKeywordsContain(AuthorityKeyword), "Imperial target", true);
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, imperialTarget);
            SpecialStringMaker.ShowNumberOfCardsInPlay(imperialTarget);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            LinqCardCriteria imperialTargetInTrash = new LinqCardCriteria((Card c) => c.IsTarget && c.DoKeywordsContain(AuthorityKeyword) && c.Location == base.TurnTaker.Trash, "Imperial target cards in the environment trash", false, false, "Imperial target card in the environment trash", "Imperial target cards in the environment trash");
            LinqCardCriteria imperialTargetInPlay = new LinqCardCriteria((Card c) => c.IsTarget && c.DoKeywordsContain(AuthorityKeyword) && c.IsInPlayAndHasGameText, "Imperial targets in play", false, false, "Imperial target in play", "Imperial targets in play");
            // "At the end of the environment turn, reveal cards from the top of the environment deck until an Imperial target is revealed. Put that target into play. Shuffle the other revealed cards back into the deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, SummonImperialResponse, TriggerType.MoveCard);
            // "At the start of the environment turn, if there are at least 3 Imperial targets in the environment trash and none in play, move this card under the Timeline card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && base.GameController.FindCardsWhere(imperialTargetInTrash).Count() >= 3 && !base.GameController.FindCardsWhere(imperialTargetInPlay).Any(), EraseFromHistoryResponse, TriggerType.MoveCard);
        }

        public IEnumerator SummonImperialResponse(PhaseChangeAction pca)
        {
            // "... reveal cards from the top of the environment deck until an Imperial target is revealed. Put that target into play. Shuffle the other revealed cards back into the deck."
            LinqCardCriteria imperialTarget = new LinqCardCriteria((Card c) => c.IsTarget && c.DoKeywordsContain(AuthorityKeyword), "Imperial target", true);
            IEnumerator revealCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, false, true, false, imperialTarget, 1, revealedCardDisplay: RevealedCardDisplay.Message);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            yield break;
        }

        public IEnumerator EraseFromHistoryResponse(GameAction ga)
        {
            // "... move this card under the Timeline card."
            IEnumerable<Card> targetsInTrash = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsTarget && c.DoKeywordsContain(AuthorityKeyword) && c.Location == base.TurnTaker.Trash));
            int numTargetsInTrash = targetsInTrash.Count();
            string announcement = "The heroes drove back " + numTargetsInTrash.ToString() + " Imperial enforcers, leaving none standing!";
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(announcement, Priority.Medium, GetCardSource(), associatedCards: targetsInTrash, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            string effect = "The effects of " + base.Card.Title + " are removed from history!";
            IEnumerator effectCoroutine = base.GameController.SendMessageAction(effect, Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(effectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(effectCoroutine);
            }
            Location underTimeline = base.TurnTaker.FindCard(TimelineIdentifier).UnderLocation;
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, underTimeline, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield break;
        }
    }
}
