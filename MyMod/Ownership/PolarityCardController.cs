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
    public class PolarityCardController : ExpansionWeatherCardController
    {
        public PolarityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play: show whether this card is affecting token changes
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(NumbersGoDown), () => "This card will reverse the Map card's attempts to add 2 tokens to Stat cards this turn.", () => "This card is not affecting attempts to add 2 tokens to Stat cards this turn.", () => true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public readonly string NumbersGoDown = "NumbersGoDown";

        public override void AddTriggers()
        {
            base.AddTriggers();
            AddTrigger((AddTokensToPoolAction tpa) => HasBeenSetToTrueThisTurn(NumbersGoDown) && tpa.NumberOfTokensToAdd == 2 && tpa.TokenPool.CardWithTokenPool.Identifier == StatCardIdentifier && tpa.CardSource != null && tpa.CardSource.Card.Identifier == MapCardIdentifier, PreventAndRemoveResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.AddTokensToPool }, TriggerTiming.Before);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play during a player's turn, discard the top card of that player's deck."
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
                // "If it's a One-Shot card, whenever the Map card would add 2 tokens to a Stat card this turn, remove 2 tokens from that Stat card instead."
                if (important != null)
                {
                    Card discarded = important.CardToMove;
                    if (discarded.IsOneShot)
                    {
                        SetCardProperty(NumbersGoDown, true);
                        IEnumerator messageCoroutine = base.GameController.SendMessageAction("The " + base.Card.Title + " shifted!\nNumbers go down.", Priority.Medium, GetCardSource(), discarded.ToEnumerable(), true);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(messageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(messageCoroutine);
                        }
                    }
                }
            }
        }

        IEnumerator PreventAndRemoveResponse(AddTokensToPoolAction tpa)
        {
            // "... remove 2 tokens from that Stat card instead."
            IEnumerator cancelCoroutine = CancelAction(tpa, isPreventEffect: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator removeCoroutine = base.GameController.RemoveTokensFromPool(tpa.TokenPool, 2, cardSource: GetCardSource());
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
