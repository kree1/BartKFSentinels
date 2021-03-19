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
    public class Sample01DONCardController : EvidenceStorageUtilityCardController
    {
        public Sample01DONCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has reacted to damage this turn", () => base.Card.Title + " has not reacted to damage this turn");
        }

        protected const string OncePerTurn = "SplashOncePerTurn";
        private ITrigger SplashDamageTrigger;

        public override void AddTriggers()
        {
            // "The first time another target in this play area deals damage each turn, this card deals 1 infernal damage to each non-environment target in the damaged target's play area."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.DamageSource != null && dda.DamageSource.Card != null && dda.DamageSource.Card.IsTarget && dda.DamageSource.Card != base.Card && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, SplashDamageResponse, TriggerType.DealDamage, TriggerTiming.After);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, it deals the character with the highest HP in this play area 2 irreducible toxic damage."
            Location playArea = base.Card.Location.HighestRecursiveLocation;
            bool damageSuccess = false;
            if (playArea.Cards.Any((Card c) => c.IsCharacter && c.IsTarget))
            {
                List<DealDamageAction> toxicAttack = new List<DealDamageAction>();
                IEnumerator damageCoroutine = base.DealDamageToHighestHP(base.Card, 1, (Card c) => c.IsCharacter && c.IsTarget && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, (Card c) => 2, DamageType.Toxic, isIrreducible: true, storedResults: toxicAttack, numberOfTargets: () => 1);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
                damageSuccess = DidDealDamage(toxicAttack);
            }
            if (!damageSuccess)
            {
                // "If no damage was dealt this way, play the top card of the environment deck."
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

        public IEnumerator SplashDamageResponse(DealDamageAction dda)
        {
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            // "... this card deals 1 infernal damage to each non-environment target in the damaged target's play area."
            Card originalTarget = dda.Target;
            Location splashZone = null;
            if (originalTarget.IsInPlay)
            {
                // If it's in play, its play area is its current location
                splashZone = originalTarget.Location.HighestRecursiveLocation;
            }
            else
            {
                // If not, its play area is the play area associated with its deck
                splashZone = originalTarget.Owner.PlayArea;
            }
            IEnumerator damageCoroutine = base.DealDamage(base.Card, (Card c) => !c.IsEnvironment && c.Location.HighestRecursiveLocation == splashZone, 1, DamageType.Infernal);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
