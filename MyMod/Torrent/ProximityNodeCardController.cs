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
    public class ProximityNodeCardController : TorrentUtilityCardController
    {
        public ProximityNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => base.NumTargetsDestroyedThisTurn() == 1, () => "1 target has been destroyed this turn.", () => base.NumTargetsDestroyedThisTurn().ToString() + " targets have been destroyed this turn.", () => true);
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, you may destroy a target with X*2 or fewer HP, where X = the number of targets destroyed this turn."
            AddWhenDestroyedTrigger(DestroyTargetResponse, TriggerType.DestroyCard);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, you may play a card."
            IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, associateCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }

        public IEnumerator DestroyTargetResponse(DestroyCardAction dca)
        {
            // "... you may destroy a target with X*2 or fewer HP, where X = the number of targets destroyed this turn."
            int x = base.NumTargetsDestroyedThisTurn();
            int maxHP = x * 2;
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value <= maxHP), true, responsibleCard: base.Card, cardSource: GetCardSource());
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
