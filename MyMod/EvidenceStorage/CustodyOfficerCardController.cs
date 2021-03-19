using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class CustodyOfficerCardController : OfficerCardController
    {
        public CustodyOfficerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.DoKeywordsContain("storage")));
        }

        public override void AddTriggers()
        {
            // "Whenever an environment card enters a villain play area, this card deals the character or Device with the highest HP in that play area 2 projectile damage."
            AddTrigger<MoveCardAction>((MoveCardAction mca) => mca.CardToMove.IsEnvironment && mca.Destination.IsInPlay && mca.Destination.IsVillain, DontTouchThatResponse, TriggerType.DealDamage, TriggerTiming.After, isActionOptional: false);
            // "At the end of the environment turn, if there are no Storage cards in play, put a Storage card from the environment trash into play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, OpenCrateFromTrash, TriggerType.PutIntoPlay);
            base.AddTriggers();
        }

        public IEnumerator DontTouchThatResponse(MoveCardAction mca)
        {
            // "Whenever an environment card enters a villain play area, this card deals the character or Device with the highest HP in that play area 2 projectile damage."
            // Identify the play area that the card moved to
            Location targetPlayArea = mca.Destination.HighestRecursiveLocation;
            // Deal damage to the character or Device target in that play area with the highest HP
            IEnumerator shootCoroutine = base.DealDamageToHighestHP(base.Card, 1, (Card c) => (c.IsCharacter || c.IsDevice) && c.Location.HighestRecursiveLocation == targetPlayArea, (Card c) => 2, DamageType.Projectile, isIrreducible: false, optional: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shootCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shootCoroutine);
            }
            yield break;
        }

        public IEnumerator OpenCrateFromTrash(PhaseChangeAction pca)
        {
            // "... if there are no Storage cards in play, put a Storage card from the environment trash into play."
            int cratesInPlay = FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("storage")).Count();
            int cratesInTrash = FindCardsWhere((Card c) => c.IsInTrash && c.DoKeywordsContain("storage")).Count();
            String message = "There are already " + cratesInPlay.ToString() + " Storage cards in play.";
            List<Card> associatedCards = null;
            if (cratesInPlay >= 1)
            {
                associatedCards = new List<Card>();
                associatedCards.Add(FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("storage")).FirstOrDefault());
            }

            if (cratesInPlay == 1)
            {
                message = "There is already a Storage card in play.";
            }
            else if (cratesInPlay <= 0 && cratesInTrash <= 0)
            {
                message = "There are no Storage cards in the trash for " + base.Card.Title + " to retrieve.";
            }
            else if (cratesInPlay <= 0)
            {
                message = "There are no Storage cards in play, so " + base.Card.Title + " retrieves one from the trash.";
            }
            IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), associatedCards: associatedCards, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(showCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(showCoroutine);
            }

            if (cratesInPlay <= 0 && cratesInTrash > 0)
            {
                MoveCardDestination dest = new MoveCardDestination(base.TurnTaker.PlayArea);
                IEnumerator openCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(DecisionMaker, base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain("storage")), dest.ToEnumerable(), isPutIntoPlay: true, optional: false, showOutput: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(openCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(openCoroutine);
                }
            }
            yield break;
        }
    }
}
