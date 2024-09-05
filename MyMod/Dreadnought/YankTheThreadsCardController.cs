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
    public class YankTheThreadsCardController : StressCardController
    {
        public YankTheThreadsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{Dreadnought} deals 1 target X irreducible projectile damage, where X = the number of cards in your trash plus 2."
            int x = TurnTaker.Trash.NumberOfCards + 2;
            IEnumerator yeetCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), x, DamageType.Projectile, 1, false, 1, isIrreducible: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(yeetCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(yeetCoroutine);
            }
            // "You may shuffle your trash into your deck. If you do, discard the top card of your deck."
            YesNoDecision choice = new YesNoDecision(GameController, DecisionMaker, SelectionType.ShuffleTrashIntoDeck, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = GameController.MakeDecisionAction(choice);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(choice))
            {
                IEnumerator shuffleCoroutine = GameController.ShuffleTrashIntoDeck(TurnTakerController, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(shuffleCoroutine);
                }
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
            else
            {
                // "Otherwise, {Dreadnought} deals herself X / 3 irreducible toxic damage, rounded up."
                double quotient = (double)x / (double)3;
                int rounded = (int)Math.Ceiling(quotient);
                IEnumerator toxicCoroutine = DealDamage(CharacterCard, CharacterCard, rounded, DamageType.Toxic, isIrreducible: true, cardSource: GetCardSource());
                if (UseUnityCoroutines)
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
}
