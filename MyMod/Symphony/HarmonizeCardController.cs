using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class HarmonizeCardController : SymphonyUtilityCardController
    {
        public HarmonizeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Draw 3 cards."
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, 3);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Discard a silence card."
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(DecisionMaker, additionalCriteria: IsSilence, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "Discard a card or play a measure card."
            // "Discard a card or play a measure card."
            for (int i = 0; i < 2; i++)
            {
                List<Function> options = new List<Function>();
                options.Add(new Function(DecisionMaker, "Discard a card", SelectionType.DiscardCard, () => GameController.SelectAndDiscardCard(DecisionMaker, cardSource: GetCardSource()), onlyDisplayIfTrue: HeroTurnTaker.Hand.Cards.Any(), forcedActionMessage: TurnTaker.Name + " cannot play any " + MeasureKeyword + " cards, so they'll have to discard one.", repeatDecisionText: "discard a card"));
                options.Add(new Function(DecisionMaker, "Play a " + MeasureKeyword + " card", SelectionType.PlayCard, () => GameController.SelectAndPlayCardFromHand(DecisionMaker, false, cardCriteria: new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MeasureKeyword), MeasureKeyword), cardSource: GetCardSource()), onlyDisplayIfTrue: CanPlayCards(DecisionMaker) && HeroTurnTaker.Hand.Cards.Any((Card c) => GameController.DoesCardContainKeyword(c, MeasureKeyword) && GameController.CanPlayCard(FindCardController(c)) == CanPlayCardResult.CanPlay), repeatDecisionText: "play a " + MeasureKeyword + " card"));
                SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, false, noSelectableFunctionMessage: TurnTaker.Name + " cannot discard any cards or play any " + MeasureKeyword + " cards.", cardSource: GetCardSource());
                IEnumerator chooseCoroutine = GameController.SelectAndPerformFunction(choice);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(chooseCoroutine);
                }
            }
        }
    }
}
