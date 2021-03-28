using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class EphemeralDeploymentCardController : CardController
    {
        public EphemeralDeploymentCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OneClusterPerTurn), () => "A Cluster has already entered play this turn.", () => "No Clusters have entered play this turn.");
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster")), () => false);
        }

        protected const string OneClusterPerTurn = "OneClusterPerTurn";

        public override void AddTriggers()
        {
            // "The first time a Cluster enters play each turn, you may draw a card or play a card."
            AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => !HasBeenSetToTrueThisTurn(OneClusterPerTurn) && cepa.CardEnteringPlay.DoKeywordsContain("cluster"), DrawOrPlayResponse, new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard }, TriggerTiming.After);
            base.AddTriggers();
        }

        public IEnumerator DrawOrPlayResponse(CardEntersPlayAction cepa)
        {
            base.SetCardPropertyToTrueIfRealAction(OneClusterPerTurn);
            // "... you may draw a card or play a card."
            IEnumerator drawPlayCoroutine = DrawACardOrPlayACard(base.HeroTurnTakerController, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawPlayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawPlayCoroutine);
            }
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int maxTargets = GetPowerNumeral(0, 3);
            int hpValue = GetPowerNumeral(1, 1);
            // "Destroy up to 3 targets with 1 HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1), 3, false, 0, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Return a random Cluster from your trash to your hand."
            IEnumerable<Card> trashClusters = base.TurnTaker.Trash.Cards.Where((Card c) => c.DoKeywordsContain("cluster"));
            bool anyClusters = trashClusters.Count() > 0;
            IEnumerator retrieveCoroutine = null;
            if (!anyClusters)
            {
                retrieveCoroutine = base.GameController.SendMessageAction("No Clusters were found in " + base.TurnTaker.Name + "'s trash.", Priority.Medium, GetCardSource(), showCardSource: true);
            }
            else
            {
                Card clusterFound = trashClusters.ElementAt(Game.RNG.Next(0, trashClusters.Count()));
                retrieveCoroutine = base.GameController.MoveCard(base.TurnTakerController, clusterFound, base.HeroTurnTaker.Hand, showMessage: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(retrieveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(retrieveCoroutine);
            }
            yield break;
        }
    }
}
