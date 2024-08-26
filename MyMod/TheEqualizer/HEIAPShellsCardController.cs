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
    public class HEIAPShellsCardController : CartridgeCardController
    {
        public HEIAPShellsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase projectile damage dealt by {TheEqualizer} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard) && dda.DamageType == DamageType.Projectile, 1);
        }

        public override IEnumerator DealBonusDamageResponse(DealDamageAction dda)
        {
            // "... that target deals itself 1 irreducible fire damage."
            IEnumerator fireCoroutine = DealDamage(dda.Target, dda.Target, 1, DamageType.Fire, isIrreducible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(fireCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(fireCoroutine);
            }
        }
    }
}
