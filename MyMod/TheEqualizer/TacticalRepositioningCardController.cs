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
    public class TacticalRepositioningCardController : CardController
    {
        public TacticalRepositioningCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to {TheEqualizer} by 1."
            AddReduceDamageTrigger((Card c) => c == CharacterCard, 1);
            // "Damage dealt by {TheEqualizer} is irreducible."
            AddMakeDamageIrreducibleTrigger((DealDamageAction dda) => dda.DamageSource.IsSameCard(CharacterCard));
            // "At the end of the villain turn, destroy all environment cards, then play the top card of the environment deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, RearrangeResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.PlayCard });
            // "At the start of the villain turn, destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyThisCardResponse, TriggerType.DestroySelf);
        }

        public IEnumerator RearrangeResponse(PhaseChangeAction pca)
        {
            // "... destroy all environment cards, ..."
            IEnumerator destroyCoroutine = GameController.DestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "... then play the top card of the environment deck."
            IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(pca);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
