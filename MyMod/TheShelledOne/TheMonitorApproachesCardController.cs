using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class TheMonitorApproachesCardController : CardController
    {
        public override bool DoNotMoveOneShotToTrash
        {
            get
            {
                if (OneShotStaysInDeck)
                {
                    return true;
                }
                return base.DoNotMoveOneShotToTrash;
            }
        }

        public TheMonitorApproachesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowTokenPool(cardIdentifier: "TheShelledOneCharacter", poolIdentifier: StrikePoolIdentifier).Condition = () => !base.CharacterCard.IsTarget;
        }

        public const string StrikePoolIdentifier = "TheShelledOneStrikePool";
        private bool OneShotStaysInDeck = false;

        public override IEnumerator Play()
        {
            // "Play the top card of the villain deck."
            IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            if (base.CharacterCard.IsTarget)
            {
                // "If {TheShelledOne} is a target, it deals itself 5 melee damage..."
                IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.CharacterCard, (Card c) => c == base.CharacterCard, 5, DamageType.Melee, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
                // "... and plays the top card of the villain deck."
                IEnumerator play2Coroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(play2Coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(play2Coroutine);
                }
            }
            else
            {
                // "Otherwise, put a token on {TheShelledOne}..."
                TokenPool strikePool = base.CharacterCard.FindTokenPool(StrikePoolIdentifier);
                IEnumerator addTokenCoroutine = base.GameController.AddTokensToPool(strikePool, 1, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(addTokenCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(addTokenCoroutine);
                }
                // "... and shuffle this card into the villain deck."
                /*IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.Deck, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
                IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }*/
                OneShotStaysInDeck = true;
                IEnumerator shuffleCoroutine = base.GameController.ShuffleCardIntoLocation(DecisionMaker, base.Card, base.TurnTaker.Deck, false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            yield break;
        }
    }
}
