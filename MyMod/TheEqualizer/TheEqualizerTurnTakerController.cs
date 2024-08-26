using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BartKFSentinels.TheEqualizer
{
    public class TheEqualizerTurnTakerController : TurnTakerController
    {
        public TheEqualizerTurnTakerController(TurnTaker turnTaker, GameController gameController) : base(turnTaker, gameController)
        {

        }

        public const string ObjectiveIdentifier = "LucrativeContract";
        public const string MunitionKeyword = "munition";

        public bool IsMarked(Card c)
        {
            if (c.IsInPlayAndHasGameText)
            {
                return c.NextToLocation.Cards.Any((Card x) => x.Identifier == ObjectiveIdentifier);
            }
            return false;
        }

        public Card MarkedTarget(CardSource looking)
        {
            return GameController.FindCardsWhere((Card c) => IsMarked(c), visibleToCard: looking).FirstOrDefault();
        }

        public override IEnumerator StartGame()
        {
            if (GameController.Game.IsChallenge)
            {
                // "{TheEqualizer}'s max HP for this game is 60."
                IEnumerator setCoroutine = GameController.ChangeMaximumHP(CharacterCard, 60, alsoSetHP: true, CharacterCardController.GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(setCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(setCoroutine);
                }
            }
            // "Put “Lucrative Contract” into play next to the hero character target with the second highest HP."
            IEnumerator markCoroutine = PutCardIntoPlay(ObjectiveIdentifier);
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(markCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(markCoroutine);
            }
            // "Shuffle the villain deck and reveal cards until a Munition is revealed. Put that Munition into play. Shuffle the other revealed cards back into the villain deck."
            IEnumerator loadCoroutine = PutCardsIntoPlay(new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword), "Munition"), 1);
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(loadCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(loadCoroutine);
            }
        }
    }
}
