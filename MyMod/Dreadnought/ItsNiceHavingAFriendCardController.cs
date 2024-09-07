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
            // "At the start of your turn, you may destroy one of your Ongoing cards. If you do, draw 2 cards."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DestroyDrawResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DrawCard, TriggerType.GainHP });
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numCards = GetPowerNumeral(0, 2);
            int numRequired = GetPowerNumeral(1, 2);
            int numHP = GetPowerNumeral(2, 2);
            // "Discard 2 cards. If you discarded 2 cards this way, {Dreadnought} and another hero target each regain 2 HP."
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

        IEnumerator DestroyDrawResponse(PhaseChangeAction pca)
        {
            // "... you may destroy one of your Ongoing cards."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsOngoing(c) && c.Owner == TurnTaker && c.IsInPlayAndHasGameText, "belonging to " + TurnTaker.Name + " in play", useCardsPrefix: true, useCardsSuffix: false, singular: "Ongoing card", plural: "Ongoing cards"), true, results, responsibleCard: Card, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If you do, draw 2 cards."
            if (DidDestroyCard(results))
            {
                IEnumerator drawCoroutine = DrawCards(DecisionMaker, 2);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
        }
    }
}
