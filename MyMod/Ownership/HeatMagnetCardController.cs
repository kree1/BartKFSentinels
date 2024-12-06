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
    public class HeatMagnetCardController : BallparkModCardController
    {
        public HeatMagnetCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero character is dealt 4 or more fire damage, that hero character deals 1 target 3 lightning damage 3 times."
            AddTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && dda.DidDealDamage && dda.DamageType == DamageType.Fire && dda.FinalAmount >= 4 && dda.Target.IsTarget, ConverterResponse, TriggerType.DealDamage, TriggerTiming.After);
        }

        public IEnumerator ConverterResponse(DealDamageAction dda)
        {
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("The " + base.Card.Title + " catches. The Thermal Converter hums.", Priority.Medium, GetCardSource(), associatedCards: dda.Target.ToEnumerable(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            List<DealDamageAction> instances = new List<DealDamageAction>();
            for (int i = 0; i < 3; i++)
            {
                instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, dda.Target), null, 3, DamageType.Lightning));
            }
            IEnumerator lightningCoroutine = SelectTargetAndDealMultipleInstancesOfDamage(instances);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(lightningCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(lightningCoroutine);
            }
        }
    }
}
