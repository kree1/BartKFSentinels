﻿using Handelabra;
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
            Card momentumCard = base.TurnTaker.FindCard("MomentumCharacter");
            Log.Debug("Setting Momentum's max HP to " + (H * 4).ToString() + "...");
            momentumCard.SetMaximumHP(H * 4, false);
        }

        public override IEnumerator StartGame()
        {
            // "Put {Breakaway} into play, "Criminal Courier" side up, with 30 HP."
            IEnumerator startingHPCoroutine = base.GameController.SetHP(base.TurnTaker.FindCard("BreakawayCharacter"), 30, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(startingHPCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(startingHPCoroutine);
            }

            // Set Momentum's current HP to its max HP
            Card momentumCard = base.TurnTaker.FindCard("MomentumCharacter");
            IEnumerator momentumHPCoroutine = base.GameController.SetHP(momentumCard, momentumCard.MaximumHitPoints.Value, cardSource: base.GameController.FindCardController(momentumCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(momentumHPCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(momentumHPCoroutine);
            }

            yield break;
        }
    }
}
