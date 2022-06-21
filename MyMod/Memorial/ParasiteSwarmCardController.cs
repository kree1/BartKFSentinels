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
    public class ParasiteSwarmCardController : CardController
    {
        public ParasiteSwarmCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, this card deals each hero target 2 melee damage."
            AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => c.IsHero, TargetType.All, 2, DamageType.Melee);
            // "At the start of the villain turn, this card regains {H - 1} HP."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealResponse, TriggerType.GainHP);
        }

        private IEnumerator HealResponse(PhaseChangeAction pca)
        {
            // "... this card regains {H - 1} HP."
            IEnumerator healCoroutine = base.GameController.GainHP(base.Card, H - 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            yield break;
        }
    }
}
