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
    public class SurgeProtectionCardController : TorrentUtilityCardController
    {
        public SurgeProtectionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"), "Cluster"));
        }

        public override void AddTriggers()
        {
            // "Reduce damage dealt to Clusters by 1."
            AddReduceDamageTrigger((Card c) => c.DoKeywordsContain("cluster"), 1);
            base.AddTriggers();
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Return all Clusters from your trash to your hand."
            IEnumerable<Card> recovering = base.TurnTaker.Trash.Cards.Where((Card c) => c.DoKeywordsContain("cluster"));
            IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, recovering, base.HeroTurnTaker.Hand, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            // "Destroy this card."
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
