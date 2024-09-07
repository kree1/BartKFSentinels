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
            // "You may shuffle your trash into your deck."
            int cardsMoved = 0;
            DealDamageAction preview = new DealDamageAction(GetCardSource(), new DamageSource(GameController, CharacterCard), CharacterCard, 3, DamageType.Toxic, isIrreducible: true);
            if (TurnTaker.Trash.HasCards)
            {
                // Ask whether to shuffle trash into deck
                SelectionType tag = SelectionType.ShuffleTrashIntoDeck;
                if (TurnTaker.Trash.NumberOfCards <= 2)
                {
                    CardsToMove = 2;
                    IsShuffle = true;
                    NoEffect = true;
                    tag = SelectionType.Custom;
                }
                YesNoDecision choice = new YesNoDecision(GameController, DecisionMaker, tag, gameAction: preview, cardSource: GetCardSource());
                IEnumerator decideCoroutine = GameController.MakeDecisionAction(choice);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(decideCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(decideCoroutine);
                }
                if (DidPlayerAnswerYes(choice))
                {
                    // Count cards in trash, then shuffle them into deck
                    cardsMoved = TurnTaker.Trash.NumberOfCards;
                    IEnumerator shuffleCoroutine = GameController.ShuffleTrashIntoDeck(TurnTakerController, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(shuffleCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(shuffleCoroutine);
                    }
                }
            }
            // "If you moved 2 or fewer cards to your deck this way, {Dreadnought} deals herself 3 irreducible toxic damage."
            if (cardsMoved <= 2)
            {
                IEnumerator toxicCoroutine = DealDamage(CharacterCard, CharacterCard, 3, DamageType.Toxic, isIrreducible: true, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(toxicCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(toxicCoroutine);
                }
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
