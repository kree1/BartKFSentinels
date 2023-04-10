using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class ThermiteLauncherCardController : CardController
    {
        public ThermiteLauncherCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, {Memorial} deals each non-villain target 1 irreducible fire damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.CharacterCard, (Card c) => c.IsTarget && !IsVillainTarget(c), TargetType.All, 1, DamageType.Fire, isIrreducible: true);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, destroy all non-target environment cards."
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment && !c.IsTarget, "non-target environment"), cardSource: GetCardSource());
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
