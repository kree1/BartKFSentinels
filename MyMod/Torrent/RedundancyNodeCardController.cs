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
    public class RedundancyNodeCardController : ClusterCardController
    {
        public RedundancyNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value <= 2));
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, you may destroy a target with 2 or fewer HP."
            AddWhenDestroyedTrigger(DestroyTargetResponse, TriggerType.DestroyCard);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            if (Journal.GetCardPropertiesBoolean(base.Card, IgnoreEntersPlay) != true)
            {
                // "When this card enters play, you may draw a card and you may play a card."
                IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
                IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, associateCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DestroyTargetResponse(DestroyCardAction dca)
        {
            // "... you may destroy a target with 2 or fewer HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value <= 2), true, responsibleCard: base.Card, cardSource: GetCardSource());
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
