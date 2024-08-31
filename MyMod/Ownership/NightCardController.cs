using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class NightCardController : ExpansionWeatherCardController
    {
        public NightCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play during a player's turn, ..."
            if (base.Game.ActiveTurnTaker.IsPlayer)
            {
                // "... discard the top card of that player's deck."
                List<MoveCardAction> results = new List<MoveCardAction>();
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.Game.ActiveTurnTaker.Deck, results, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                MoveCardAction important = results.FirstOrDefault((MoveCardAction mca) => mca.WasCardMoved);
                // "If it's an Ongoing or Equipment card, they put 1 of their non-character cards in play on the bottom of their deck and play the discarded card."
                if (important != null)
                {
                    Card discarded = important.CardToMove;
                    if (IsOngoing(discarded) || IsEquipment(discarded))
                    {
                        IEnumerator announceCoroutine = base.GameController.SendMessageAction("Night Shift.", Priority.Medium, GetCardSource(), discarded.ToEnumerable(), true);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(announceCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(announceCoroutine);
                        }
                        IEnumerator moveCoroutine = base.GameController.SelectAndMoveCard(base.GameController.FindHeroTurnTakerController(base.Game.ActiveTurnTaker.ToHero()), (Card c) => c.Owner == base.Game.ActiveTurnTaker && c.IsInPlayAndHasGameText && !c.IsCharacter, base.Game.ActiveTurnTaker.Deck, toBottom: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(moveCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(moveCoroutine);
                        }
                        IEnumerator playCoroutine = base.GameController.PlayCard(base.GameController.FindTurnTakerController(base.Game.ActiveTurnTaker), discarded, responsibleTurnTaker: base.Game.ActiveTurnTaker, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(playCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(playCoroutine);
                        }
                    }
                }
            }
        }
    }
}
