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
    public class RecoveryNodeCardController : TorrentUtilityCardController
    {
        public RecoveryNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => base.NumTargetsDestroyedThisTurn() == 1, () => "1 target has been destroyed this turn.", () => base.NumTargetsDestroyedThisTurn().ToString() + " targets have been destroyed this turn.", () => true);
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, 1 target regains X HP, where X = the number of targets destroyed this turn."
            AddWhenDestroyedTrigger(HealResponse, TriggerType.GainHP);
            base.AddTriggers();
        }

        public override IEnumerator Play()
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
