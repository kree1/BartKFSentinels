using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.AkashBhuta
{
    public class UnsustainableAkashBhutaTurnTakerController : TurnTakerController
    {
        public UnsustainableAkashBhutaTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // Setup: "Put [i]Unsustainable Akash'bhuta[/i] into play, “Choking Overgrowth” side up, with 70 HP."
            IEnumerator setCoroutine = GameController.SetHP(TurnTaker.FindCard("AkashBhutaCharacter"), 70, cardSource: GameController.FindCardController(TurnTaker.CharacterCard).GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(setCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(setCoroutine);
            }
            // "Reveal cards from the villain deck until {H - 1} Primeval Limbs are revealed. Put them into play and shuffle the other revealed cards back into the villain deck."
            IEnumerator putCoroutine = PutCardsIntoPlay(new LinqCardCriteria((Card c) => c.IsPrimevalLimb), H - 1);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(putCoroutine);
            }
        }
    }
}
