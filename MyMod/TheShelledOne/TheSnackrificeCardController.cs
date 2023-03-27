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
            CannotPlayCards((TurnTakerController turnTaker) => turnTaker != null && IsHero(turnTaker.TurnTaker), (Card c) => IsHero(c));
            // "At the start of the villain turn, the non-hero target with the highest HP deals each hero target 3 sonic damage. Then, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDestroyResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        public IEnumerator DamageDestroyResponse(GameAction ga)
        {
            // "... the non-hero target with the highest HP deals each hero target 3 sonic damage."
            List<Card> highest = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => !IsHeroTarget(c), highest, evenIfCannotDealDamage: true, cardSource: GetCardSource());
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
                IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, highestNonHero, (Card c) => IsHeroTarget(c), 3, DamageType.Sonic, cardSource: GetCardSource());
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
        }
    }
}
