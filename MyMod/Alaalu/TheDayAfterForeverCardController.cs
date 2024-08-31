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
    public class TheDayAfterForeverCardController : AlaaluUtilityCardController
    {
        public TheDayAfterForeverCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c.Identifier == "TheHeartOfTheWorld"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, search the environment deck and trash for [i]The Heart of the World[/i] and put it into play. If you searched the environment deck, shuffle it."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, ArriveResponse, TriggerType.PutIntoPlay);
            // "All damage is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => true);
            // "When a Landmark is destroyed by an environment target, restore all character card targets to their maximum HP. Then, remove this card and that Landmark from the game."
            AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.CardToDestroy.Card.DoKeywordsContain(LandmarkKeyword) && dca.CardSource != null && dca.CardSource.Card != null && dca.CardSource.Card.IsEnvironmentTarget, MoveOnResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.RemoveFromGame }, TriggerTiming.After);
        }

        public IEnumerator ArriveResponse(GameAction ga)
        {
            // "... search the environment deck and trash for [i]The Heart of the World[/i] and put it into play. If you searched the environment deck, shuffle it."
            IEnumerator arriveCoroutine = PlayCardFromLocations(new Location[] { base.TurnTaker.Deck, base.TurnTaker.Trash }, "TheHeartOfTheWorld");
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(arriveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(arriveCoroutine);
            }
            yield break;
        }

        public IEnumerator MoveOnResponse(DestroyCardAction dca)
        {
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("With your help, " + dca.CardSource.Card.Title + " has made the decision to revise her people's Choice and open the way for the Alaalids to move on.", Priority.High, GetCardSource(), associatedCards: new Card[] { dca.CardSource.Card }, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            // "... restore all character card targets to their maximum HP."
            IEnumerator restoreCoroutine = base.GameController.SelectCardsAndDoAction(DecisionMaker, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && c.IsCharacter), SelectionType.SetHP, (Card c) => base.GameController.SetHP(c, c.MaximumHitPoints ?? 0, GetCardSource()), cardSource: GetCardSource(), allowAutoDecide: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(restoreCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(restoreCoroutine);
            }
            // "Then, remove this card and that Landmark from the game."
            IEnumerator removeCoroutine = base.GameController.MoveCards(base.TurnTakerController, new Card[] { dca.CardToDestroy.Card, base.Card }, (Card c) => new MoveCardDestination(c.Owner.OutOfGame), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }
            yield break;
        }
    }
}
