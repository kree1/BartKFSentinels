using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class DepletedUraniumRoundsCardController : CartridgeCardController
    {
        public DepletedUraniumRoundsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Projectile damage dealt by {TheEqualizer} is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard) && dda.DamageType == DamageType.Projectile);
        }

        public override IEnumerator DealBonusDamageResponse(DealDamageAction dda)
        {
            // "... she also deals that target 1 toxic damage."
            IEnumerator toxicCoroutine = DealDamage(CharacterCard, dda.Target, 1, DamageType.Toxic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(toxicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(toxicCoroutine);
            }
        }
    }
}
