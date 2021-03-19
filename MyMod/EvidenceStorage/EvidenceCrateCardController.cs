using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class EvidenceCrateCardController : EvidenceStorageUtilityCardController
    {
        public EvidenceCrateCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "Reduce damage dealt to this card to 1."
            AddReduceDamageToSetAmountTrigger((DealDamageAction dda) => dda.Target == base.Card, 1);
            // "Whenever a non-environment target deals damage to this card, discard the top card of the environment deck. Then, select a Device card at random from the environment trash and put it into play in the play area of the target that dealt damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageSource.IsTarget && !dda.DamageSource.IsEnvironmentSource && dda.Target == base.Card && dda.Amount > 0 && !dda.IsPretend, OpenCrateResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay }, TriggerTiming.After, isActionOptional: false);
            base.AddTriggers();
        }

        public IEnumerator OpenCrateResponse(DealDamageAction dda)
        {
            // "... discard the top card of the environment deck."
            IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, null, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Then, select a Device card at random from the environment trash and put it into play in the play area of the target that dealt damage."
            Card opener = dda.DamageSource.Card;
            Location dest = opener.Owner.PlayArea;
            Log.Debug(base.Card.Title + " was dealt damage by " + opener.Title + ". Moving a random Device to " + dest.Name + "...");
            IEnumerable<Card> trashDevices = base.TurnTaker.Trash.Cards.Where((Card c) => c.DoKeywordsContain("device"));
            IEnumerable<Card> associated = null;
            bool anyDevices = trashDevices.Count() > 0;
            Card deviceRetrieved = null;
            string message = opener.Title + " opens an Evidence Crate and finds a Device!";
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
            if (!anyDevices)
            {
                message = opener.Title + " opens an Evidence Crate, but no Devices are inside.";
                messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
            }
            else
            {
                // Select a Device at random
                deviceRetrieved = trashDevices.ElementAt(Game.RNG.Next(0, trashDevices.Count()));
                message = opener.Title + " opens an Evidence Crate and finds " + deviceRetrieved.Title + "!";
                associated = deviceRetrieved.ToEnumerable();
                messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), associatedCards: associated, showCardSource: true);
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            if (deviceRetrieved != null)
            {
                // Put the selected Device into play in that target's play area
                IEnumerator retrieveCoroutine = base.GameController.MoveCard(base.TurnTakerController, deviceRetrieved, dest, isPutIntoPlay: true, playCardIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, actionSource: dda, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(retrieveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(retrieveCoroutine);
                }
            }
            yield break;
        }
    }
}
