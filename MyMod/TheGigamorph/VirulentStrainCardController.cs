using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGigamorph
{
    public class VirulentStrainCardController : TheGigamorphUtilityCardController
    {
        public VirulentStrainCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "Increase damage dealt to and by Pathogens by 1."
            base.AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource.Card != null && dda.DamageSource.Card.DoKeywordsContain("pathogen"), 1);
            base.AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target.DoKeywordsContain("pathogen"), 1);
            // "At the end of the environment turn, this card deals the non-Pathogen target with the lowest HP 2 toxic damage."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DealDamageToLowestHP(base.Card, 1, (Card c) => !c.DoKeywordsContain("pathogen"), (Card c) => 2, DamageType.Toxic), TriggerType.DealDamage);
            // "Whenever damage dealt by this card reduces a target to 0 or fewer HP, this card regains {H - 1} HP and deals the {H - 1} non-Pathogen targets with the highest HP 1 toxic damage each."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DidDealDamage && dda.DamageSource.Card == base.Card && dda.TargetHitPointsAfterBeingDealtDamage <= 0, HealAttackResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DealDamage }, TriggerTiming.After);
            base.AddTriggers();
        }

        public IEnumerator HealAttackResponse(DealDamageAction dda)
        {
            // "... this card regains {H - 1} HP..."
            IEnumerator healCoroutine = base.GameController.GainHP(base.Card, base.H - 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "... and deals the {H - 1} non-Pathogen targets with the highest HP 1 toxic damage each."
            IEnumerator attackCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => !c.DoKeywordsContain("pathogen"), (Card c) => 1, DamageType.Toxic, numberOfTargets: () => base.H - 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(attackCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(attackCoroutine);
            }
            yield break;
        }
    }
}
