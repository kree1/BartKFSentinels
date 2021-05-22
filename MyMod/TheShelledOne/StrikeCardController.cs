using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class StrikeCardController : CardController
    {
        public StrikeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowTokenPool(cardIdentifier: "TheShelledOneCharacter", poolIdentifier: StrikePoolIdentifier);
        }

        public const string StrikePoolIdentifier = "TheShelledOneStrikePool";

        public IEnumerator AddTokenAndResetResponse(GameAction ga)
        {
            // "... put a token on {TheShelledOne}..."
            TokenPool strikePool = base.CharacterCard.FindTokenPool(StrikePoolIdentifier);
            IEnumerator addTokenCoroutine = base.GameController.AddTokensToPool(strikePool, 1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(addTokenCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(addTokenCoroutine);
            }
            // "... and set this card's HP to 0."
            IEnumerator setCoroutine = base.GameController.SetHP(base.Card, 0, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(setCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(setCoroutine);
            }
            yield break;
        }
    }
}
