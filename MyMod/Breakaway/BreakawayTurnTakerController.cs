using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace BartKFSentinels.Breakaway
{
    public class BreakawayTurnTakerController : TurnTakerController
    {
        public BreakawayTurnTakerController(TurnTaker turnTaker, GameController gameController): base(turnTaker, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // "Put {Breakaway} into play, "Criminal Courier" side up, with 30 HP."
            IEnumerator startingHPCoroutine = base.GameController.SetHP(base.TurnTaker.FindCard("Breakaway"), 30);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(startingHPCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(startingHPCoroutine);
            }
            // "Put {Momentum} into play, "Under Pressure" side up."
            // ???
            // "Shuffle the villain deck."
            // ???
            yield break;
        }
    }
}
