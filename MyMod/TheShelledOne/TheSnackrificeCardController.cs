using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class TheSnackrificeCardController : CardController
    {
        public TheSnackrificeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNonHeroTargetWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Hero cards cannot be played."
            CannotPlayCards((TurnTakerController turnTaker) => turnTaker?.IsHero ?? false, (Card c) => c.IsHero);
            // "At the start of the villain turn, put the top card of each hero deck into play in turn order. The non-hero target with the highest HP deals each hero target 2 sonic damage. Then, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayDamageDestroyResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        public IEnumerator PlayDamageDestroyResponse(GameAction ga)
        {
            // "... put the top card of each hero deck into play in turn order."
            IEnumerator putCoroutine = PlayTopCardOfEachDeckInTurnOrder((TurnTakerController ttc) => ttc.IsHero, (Location l) => l.IsHero, base.TurnTaker, isPutIntoPlay: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "The non-hero target with the highest HP deals each hero target 2 sonic damage."
            List<Card> highest = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => !c.IsHero, highest, evenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highestNonHero = highest.FirstOrDefault();
            if (highestNonHero != null)
            {
                IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, highestNonHero, (Card c) => c.IsHero, 2, DamageType.Sonic, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            // "Then, destroy this card."
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
