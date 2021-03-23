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
    public class RepelInvadersCardController : TheGigamorphUtilityCardController
    {
        public RepelInvadersCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "At the end of the environment turn, destroy this card."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => base.GameController.DestroyCard(base.DecisionMaker, base.Card, showOutput: true, actionSource: pca, responsibleCard: base.Card, cardSource: GetCardSource()), TriggerType.DestroySelf);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, deal each non-Antibody target 1 toxic damage, 1 melee damage, and 0 energy damage."
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, 1, DamageType.Toxic));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, 1, DamageType.Melee));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, 0, DamageType.Energy));
            IEnumerator massDamageCoroutine = DealMultipleInstancesOfDamage(instances, (Card c) => !c.DoKeywordsContain("antibody"));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(massDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(massDamageCoroutine);
            }
            yield break;
        }
    }
}
