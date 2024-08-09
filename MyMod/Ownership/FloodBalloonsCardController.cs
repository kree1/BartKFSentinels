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
    public class FloodBalloonsCardController : BallparkModCardController
    {
        public FloodBalloonsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero character is dealt 2 or more cold damage, remove 4 tokens from their Stat card."
            AddTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && StatCardOf(dda.Target.Owner) != null && !StatCardOf(dda.Target.Owner).IsFlipped && dda.DamageType == DamageType.Cold && dda.DidDealDamage && dda.Amount >= 2, InflateResponse, TriggerType.AddTokensToPool, TriggerTiming.After);
        }

        public IEnumerator InflateResponse(DealDamageAction dda)
        {
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("One of the " + base.Card.Title + " inflates!", Priority.Medium, GetCardSource(), dda.Target.ToEnumerable(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator removeCoroutine = base.GameController.RemoveTokensFromPool(StatCardOf(dda.Target.Owner).FindTokenPool(WeightPoolIdentifier), 4, cardSource: GetCardSource());
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
