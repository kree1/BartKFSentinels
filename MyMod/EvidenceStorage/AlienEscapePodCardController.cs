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
    public class AlienEscapePodCardController : EvidenceStorageUtilityCardController
    {
        public AlienEscapePodCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            // "Targets in this play area are immune to melee damage."
            base.AddImmuneToDamageTrigger((DealDamageAction dda) => dda.DamageType == DamageType.Melee && dda.Target.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation);
            // "At the start of this play area’s turn, destroy this card."
            base.AddStartOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, (PhaseChangeAction pca) => base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, actionSource: pca, responsibleCard: base.Card, cardSource: GetCardSource()), TriggerType.DestroySelf);
            // "When this card is destroyed, it deals each other target in this play area 2 irreducible fire damage. Then, discard cards from the top of the environment deck until a Storage card is discarded. Put it into play."
            base.AddWhenDestroyedTrigger(CrashResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DiscardCard, TriggerType.PlayCard });
            base.AddTriggers();
        }

        public IEnumerator CrashResponse(DestroyCardAction dca)
        {
            // "When this card is destroyed, it deals each other target in this play area 2 irreducible fire damage."
            IEnumerator damageCoroutine = base.DealDamage(base.Card, (Card c) => c != base.Card && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, 2, DamageType.Fire, isIrreducible: true, optional: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Then, discard cards from the top of the environment deck until a Storage card is discarded. Put it into play."
            IEnumerator digCoroutine = base.DigForStorage();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(digCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(digCoroutine);
            }
            yield break;
        }
    }
}
