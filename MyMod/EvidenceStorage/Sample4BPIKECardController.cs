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
    public class Sample4BPIKECardController : EvidenceStorageUtilityCardController
    {
        public Sample4BPIKECardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            // "At the end of this play area's turn, one non-environment target in this play area regains 2 HP and this card deals itself 1 toxic damage. If no targets regained HP this way, play the top card of the environment deck."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker, EndOfTurnSequence, new TriggerType[] { TriggerType.GainHP, TriggerType.DealDamage, TriggerType.PlayCard });
            // "When this card is reduced to 0 or fewer HP, it deals the non-environment target in this play area with the highest HP 3 toxic damage."
            AddBeforeDestroyAction(ZeroHPResponse);
            base.AddTriggers();
        }

        public IEnumerator EndOfTurnSequence(PhaseChangeAction pca)
        {
            // "... one non-environment target in this play area regains 2 HP..."
            bool hpRecovered = false;
            if (base.Card.Location.HighestRecursiveLocation.Cards.Any((Card c) => c.IsTarget && !c.IsEnvironment))
            {
                List<GainHPAction> healing = new List<GainHPAction>();
                IEnumerator healCoroutine = base.GameController.SelectAndGainHP(base.DecisionMaker, 2, additionalCriteria: (Card c) => c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && !c.IsEnvironment, numberOfTargets: 1, storedResults: healing, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
                hpRecovered = !(healing.Count() <= 0 || healing.All((GainHPAction gha) => gha.AmountActuallyGained <= 0));
            }
            // "... and this card deals itself 1 toxic damage."
            IEnumerator selfDamageCoroutine = base.DealDamage(base.Card, base.Card, 1, DamageType.Toxic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDamageCoroutine);
            }
            if (!hpRecovered)
            {
                // "If no targets regained HP this way, play the top card of the environment deck."
                string message = "No targets regained HP, so " + base.Card.Title + " plays the top card of the environment deck.";
                IEnumerator showCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(showCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(showCoroutine);
                }
                IEnumerator playEnvironmentCoroutine = base.PlayTheTopCardOfTheEnvironmentDeckResponse(null);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playEnvironmentCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playEnvironmentCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator ZeroHPResponse(GameAction ga)
        {
            // "When this card is reduced to 0 or fewer HP, it deals the non-environment target in this play area with the highest HP 3 toxic damage."
            if (base.Card.HitPoints.Value <= 0 && base.Card.Location.HighestRecursiveLocation.Cards.Any((Card c) => c.IsTarget && !c.IsEnvironment))
            {
                IEnumerator toxicCoroutine = base.DealDamageToHighestHP(base.Card, 1, (Card c) => c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && !c.IsEnvironment, (Card c) => 3, DamageType.Toxic, numberOfTargets: () => 1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(toxicCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(toxicCoroutine);
                }
            }
            else if (base.Card.HitPoints.Value <= 0)
            {
                string message = base.Card.Title + " was reduced to " + base.Card.HitPoints.Value.ToString() + " HP, but there are no non-environment targets in " + base.Card.Location.HighestRecursiveLocation.OwnerName + "'s play area for it to deal damage to.";
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }
    }
}
