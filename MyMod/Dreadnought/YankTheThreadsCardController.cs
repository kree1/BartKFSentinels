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
    public class YankTheThreadsCardController : CardController
    {
        public YankTheThreadsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{Dreadnought} deals 1 target 7 irreducible projectile damage."
            IEnumerator yeetCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 7, DamageType.Projectile, 1, false, 1, isIrreducible: true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(yeetCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(yeetCoroutine);
            }
            // "Either shuffle your trash into your deck or {Dreadnought} deals herself 3 irreducible toxic damage."
            List<Function> options = new List<Function>();
            options.Add(new Function(DecisionMaker, "Shuffle your trash into your deck", SelectionType.ShuffleTrashIntoDeck, () => GameController.ShuffleTrashIntoDeck(TurnTakerController, cardSource: GetCardSource()), repeatDecisionText: "shuffle trash into deck"));
            options.Add(new Function(DecisionMaker, "{Dreadnought} deals herself 3 irreducible toxic damage", SelectionType.DealDamage, () => DealDamage(CharacterCard, CharacterCard, 3, DamageType.Toxic, isIrreducible: true, cardSource: GetCardSource()), repeatDecisionText: "Dreadnought deals herself 3 irreducible toxic damage"));
            SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, false, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = GameController.SelectAndPerformFunction(choice);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chooseCoroutine);
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
