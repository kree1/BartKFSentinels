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
    public class FairgroundsCardController : OwnershipUtilityCardController
    {
        public FairgroundsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show total damage dealt to non-hero targets by hero targets this turn
            SpecialStringMaker.ShowSpecialString(() => DamageDealtToNonHeroByHeroThisTurn() + " damage has been dealt to non-hero targets by hero targets this turn.");
            // If in play: show whether a card has been discarded for this effect this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(GameWonThisTurn), () => base.Card.Title + " has already discarded a card from a deck this turn.", () => base.Card.Title + " has not discarded a card from a deck this turn.").Condition = () => base.Card.IsInPlayAndHasGameText;
            // Add display options for "it's not currently a player's turn, so this card doesn't care"?
            // ...
        }

        public readonly string GameWonThisTurn = "GameWonThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever 8 or more damage is dealt to non-hero targets by hero targets during a player's turn, discard the top card of that player's deck. If it's an Equipment card or a target, play it."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(GameWonThisTurn) && base.GameController.Game.ActiveTurnTaker.IsPlayer && DamageDealtToNonHeroByHeroThisTurn() >= 8 && !IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DidDealDamage, AwardCrateResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard }, TriggerTiming.After);
        }

        public IEnumerator AwardCrateResponse(DealDamageAction dda)
        {
            SetCardProperty(GameWonThisTurn, true);
            // "... discard the top card of that player's deck. If it's an Equipment card or a target, play it."
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
            if (important != null)
            {
                Card discarded = important.CardToMove;
                if (IsEquipment(discarded) || discarded.IsTarget)
                {
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Game.ActiveTurnTaker.Name + " received an item crate containing " + discarded.Title, Priority.Medium, GetCardSource(), discarded.ToEnumerable(), showCardSource: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                    IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, discarded, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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
