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
    public class ItsNiceHavingAFriendCardController : CardController
    {
        public ItsNiceHavingAFriendCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may discard a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => GameController.SelectAndDiscardCard(DecisionMaker, optional: true, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource()), TriggerType.DiscardCard);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numHP = GetPowerNumeral(0, 2);
            int numCards = GetPowerNumeral(1, 2);
            int numRequired = GetPowerNumeral(2, 2);
            int secondHP = GetPowerNumeral(3, 2);
            // "{Dreadnought} regains 2 HP. Discard 2 cards. If you discarded 2 cards this way, another hero target regains 2 HP."
            // Dreadnought regains HP
            IEnumerator healSelfCoroutine = GameController.GainHP(CharacterCard, numHP);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healSelfCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healSelfCoroutine);
            }
            // Dreadnought discards cards
            List<DiscardCardAction> results = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCards(DecisionMaker, numCards, false, numCards, storedResults: results, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(results, numRequired))
            {
                // Other target regains HP
                IEnumerator healOthersCoroutine = GameController.SelectAndGainHP(DecisionMaker, numHP, additionalCriteria: (Card c) => IsHeroTarget(c) && c != CharacterCard, requiredDecisions: 1, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(healOthersCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(healOthersCoroutine);
                }
            }
        }
    }
}
