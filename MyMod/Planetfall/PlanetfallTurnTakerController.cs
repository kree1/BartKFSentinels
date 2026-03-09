using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BartKFSentinels.Planetfall
{
    public class PlanetfallTurnTakerController : TurnTakerController
    {
        public PlanetfallTurnTakerController(TurnTaker tt, GameController gameController)
            : base(tt, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // "Put [i]Higgs Field Amplifier[/i] into play."
            Card hfa = TurnTaker.GetCardByIdentifier("HiggsFieldAmplifier");
            IEnumerator putCoroutine = GameController.PlayCard(this, hfa, isPutIntoPlay: true, cardSource: new CardSource(CharacterCardController));
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(putCoroutine);
            }
            // "Shuffle the villain deck."
            IEnumerator shuffleCoroutine = GameController.ShuffleLocation(TurnTaker.Deck, cardSource: new CardSource(CharacterCardController));
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(shuffleCoroutine);
            }
        }
    }
}
