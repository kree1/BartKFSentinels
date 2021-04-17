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
    public class RecoveryNodeCardController : ClusterCardController
    {
        public RecoveryNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, 1 target regains X HP, where X = the number of targets destroyed this turn."
            AddWhenDestroyedTrigger(HealResponse, TriggerType.GainHP);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            if (Journal.GetCardPropertiesBoolean(base.Card, IgnoreEntersPlay) != true)
            {
                // "When this card enters play, you may draw a card or play a card."
                IEnumerator drawPlayCoroutine = DrawACardOrPlayACard(base.HeroTurnTakerController, true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawPlayCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawPlayCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator HealResponse(DestroyCardAction dca)
        {
            // "... 1 target regains X HP, where X = the number of targets destroyed this turn."
            int x = base.NumTargetsDestroyedThisTurn();
            IEnumerator healCoroutine = base.GameController.SelectAndGainHP(base.HeroTurnTakerController, x, requiredDecisions: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            yield break;
        }
    }
}
