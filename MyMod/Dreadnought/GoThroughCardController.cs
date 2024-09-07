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
    public class GoThroughCardController : CardController
    {
        public GoThroughCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{Dreadnought} deals 1 target 4 melee damage."
            IEnumerator meleeCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 4, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "{Dreadnought} deals each non-hero target 1 projectile damage."
            IEnumerator projectileCoroutine = DealDamage(CharacterCard, (Card c) => !IsHeroTarget(c), (Card c) => 1, DamageType.Projectile);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(projectileCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(projectileCoroutine);
            }
            // "Discard the top 2 cards of your deck."
            IEnumerator discardCoroutine = GameController.DiscardTopCards(DecisionMaker, TurnTaker.Deck, 2, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
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
