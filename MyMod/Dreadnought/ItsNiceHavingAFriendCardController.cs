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
            int numTargets = GetPowerNumeral(0, 1);
            int numHP = GetPowerNumeral(1, 2);
            int numRequired = GetPowerNumeral(2, 2);
            int numCards = GetPowerNumeral(3, 2);
            // "{Dreadnought} and up to 1 other target each regain 2 HP. If 2 heroes regained HP this way, discard the top 2 cards of your deck."
            List<GainHPAction> healResults = new List<GainHPAction>();
            // Dreadnought regains HP
            IEnumerator healSelfCoroutine = GameController.GainHP(CharacterCard, numHP, storedResults: healResults);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healSelfCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healSelfCoroutine);
            }
            // Other target(s) regain HP
            IEnumerator healOthersCoroutine = GameController.SelectAndGainHP(DecisionMaker, numHP, additionalCriteria: (Card c) => c != CharacterCard, numberOfTargets: numTargets, requiredDecisions: 0, storedResults: healResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healOthersCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healOthersCoroutine);
            }
            List<Card> healed = (from GainHPAction gha in healResults where gha.AmountActuallyGained > 0 && IsHeroCharacterCard(gha.HpGainer) select gha.HpGainer).Distinct().ToList();
            if (healed.Count() == numRequired)
            {
                // Discard cards from deck
                IEnumerator discardCoroutine = GameController.DiscardTopCards(DecisionMaker, TurnTaker.Deck, numCards, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
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
}
