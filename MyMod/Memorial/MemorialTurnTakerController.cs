using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class MemorialTurnTakerController : TurnTakerController
    {
        public MemorialTurnTakerController(TurnTaker turnTaker, GameController gameController)
        : base(turnTaker, gameController)
        {
        }

        public override IEnumerator StartGame()
        {
            // "Search the villain deck for all Incident cards..."
            List<Card> incidents = FindCardsWhere((Card c) => c.Location == TurnTaker.Deck && c.DoKeywordsContain("incident")).ToList();
            // "... and select one at random. Put that Incident into play..."
            Card incidentSelected = incidents.ElementAt(GameController.Game.RNG.Next(0, incidents.Count()));
            IEnumerator putCoroutine = GameController.PlayCard(this, incidentSelected, true, responsibleTurnTaker: TurnTaker, cardSource: GameController.FindCardController(TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "... and remove the others from the game."
            IEnumerator removeCoroutine = GameController.MoveCards(this, FindCardsWhere((Card c) => c.Location == TurnTaker.Deck && c.DoKeywordsContain("incident")), (Card c) => new MoveCardDestination(TurnTaker.OutOfGame), responsibleTurnTaker: TurnTaker, cardSource: GameController.FindCardController(TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine);
            }
        }
    }
}
