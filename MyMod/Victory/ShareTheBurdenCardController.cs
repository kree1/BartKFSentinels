using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Victory
{
    public class ShareTheBurdenCardController : CardController
    {
        public ShareTheBurdenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, discard up to 2 cards. If you discard 2 cards this way, {Victory} regains 1 HP."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, DiscardHealResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.GainHP });
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int upTo = GetPowerNumeral(0, 3);
            // "Draw a card."
            IEnumerator drawCoroutine = DrawCard(HeroTurnTaker);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Discard up to 3 cards."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCards(DecisionMaker, upTo, false, 0, storedResults: discards, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "X heroes each draw a card, where X = the number of cards discarded this way."
            if (DidDiscardCards(discards))
            {
                int x = GetNumberOfCardsDiscarded(discards);
                IEnumerator massDrawCoroutine = GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame), SelectionType.DrawCard, (TurnTaker tt) => DrawCard(tt.ToHero()), x, requiredDecisions: 0, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(massDrawCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(massDrawCoroutine);
                }
            }
        }

        IEnumerator DiscardHealResponse(PhaseChangeAction pca)
        {
            // "... discard up to 2 cards."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCards(DecisionMaker, 2, false, 0, storedResults: discards, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If you discard 2 cards this way, {Victory} regains 1 HP."
            if (DidDiscardCards(discards, 2))
            {
                IEnumerator healCoroutine = GameController.GainHP(CharacterCard, 1, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(healCoroutine);
                }
            }
        }
    }
}
