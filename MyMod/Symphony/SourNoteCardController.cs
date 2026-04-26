using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class SourNoteCardController : CostCardController
    {
        public SourNoteCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "{Symphony} deals 1 target 3 sonic damage. Draw 2 cards."
            int numTargets = GetPowerNumeral(0, 1);
            int sonicAmt = GetPowerNumeral(1, 3);
            int numDraws = GetPowerNumeral(2, 2);
            IEnumerator sonicCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), sonicAmt, DamageType.Sonic, numTargets, false, null, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(sonicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(sonicCoroutine);
            }
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, numDraws);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
        }
    }
}
