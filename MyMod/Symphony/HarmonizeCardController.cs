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
    public class HarmonizeCardController : CardController
    {
        public HarmonizeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Draw a card, discard a card, or play a card."
            // "Draw a card, discard a card, or play a card."
            for (int i = 0; i < 2; i++)
            {
                List<Function> options = new List<Function>();
                options.Add(new Function(DecisionMaker, "Draw a card", SelectionType.DrawCard, () => DrawCard(), onlyDisplayIfTrue: CanDrawCards(DecisionMaker), forcedActionMessage: TurnTaker.Name + " cannot discard or play any cards, so they'll have to draw one.", repeatDecisionText: "draw a card"));
                options.Add(new Function(DecisionMaker, "Discard a card", SelectionType.DiscardCard, () => GameController.SelectAndDiscardCard(DecisionMaker, cardSource: GetCardSource()), onlyDisplayIfTrue: HeroTurnTaker.Hand.Cards.Any(), forcedActionMessage: TurnTaker.Name + " cannot draw or play any cards, so they'll have to discard one.", repeatDecisionText: "discard a card"));
                options.Add(new Function(DecisionMaker, "Play a card", SelectionType.PlayCard, () => GameController.SelectAndPlayCardFromHand(DecisionMaker, false, cardSource: GetCardSource()), onlyDisplayIfTrue: CanPlayCards(DecisionMaker), repeatDecisionText: "play a card"));
                SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, false, noSelectableFunctionMessage: TurnTaker.Name + " cannot draw, discard, or play any cards.", cardSource: GetCardSource());
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
