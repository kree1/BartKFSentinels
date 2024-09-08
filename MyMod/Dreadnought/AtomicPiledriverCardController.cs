using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class AtomicPiledriverCardController : CardController
    {
        public AtomicPiledriverCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "You may play an Ongoing card."
            IEnumerator playCoroutine = GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardCriteria: new LinqCardCriteria((Card c) => IsOngoing(c), "Ongoing"), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
            // "{Dreadnought} deals 1 target 3 melee damage."
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 3, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "Discard the top card of your deck."
            IEnumerator discardCoroutine = GameController.DiscardTopCard(TurnTaker.Deck, null, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
        }
    }
}
