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
            // "{Symphony} deals 1 target 4 sonic damage. Draw a card."
            int numTargets = GetPowerNumeral(0, 1);
            int sonicAmt = GetPowerNumeral(1, 4);
            IEnumerator sonicCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), sonicAmt, DamageType.Sonic, numTargets, false, null, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(sonicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(sonicCoroutine);
            }
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, 1);
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
