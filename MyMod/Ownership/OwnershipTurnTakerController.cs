using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BartKFSentinels.Ownership
{
    public class OwnershipTurnTakerController : TurnTakerController
    {
        public OwnershipTurnTakerController(TurnTaker tt, GameController gameController)
            : base(tt, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // "Put this card and the Map card into play, “Stay Positive” side up."
            Card map = base.TurnTaker.GetCardByIdentifier("MapCharacter");
            CardSource baseCharacter = new CardSource(base.CharacterCardController);
            IEnumerator putCoroutine = base.GameController.PlayCard(this, map, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: baseCharacter);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "Put each Stat card under the Map card."
            List<Card> stats = base.TurnTaker.GetAllCards().Where((Card c) => c.Identifier == "StatCharacter").ToList();
            IEnumerator moveCoroutine = base.GameController.MoveCards(this, stats, (Card c) => new MoveCardDestination(map.UnderLocation), playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: baseCharacter);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            // "Shuffle the villain deck."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: baseCharacter);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
        }
    }
}
