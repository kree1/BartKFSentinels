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
    public class HalfRestCardController : BenefitCardController
    {
        public HalfRestCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 1;
        }

        public override IEnumerator OneShotEffect()
        {
            // "Each other player may discard a card. If they do, they draw 2 cards or their hero regains 2 HP."
            IEnumerator selectCoroutine = GameController.SelectTurnTakersAndDoAction(new SelectTurnTakersDecision(GameController, DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && tt != TurnTaker), SelectionType.DiscardCard, allowAutoDecide: true, cardSource: GetCardSource()), MayDiscardToDrawOrHeal, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        public IEnumerator MayDiscardToDrawOrHeal(TurnTaker tt)
        {
            // "... [tt] may discard a card."
            HeroTurnTakerController httc = FindTurnTakerController(tt).ToHero();
            List<DiscardCardAction> discardResults = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(httc, optional: true, storedResults: discardResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If they do, they draw 2 cards or their hero regains 2 HP."
            if (DidDiscardCards(discardResults))
            {
                List<Function> options = new List<Function>();
                options.Add(new Function(httc, "Draw 2 cards", SelectionType.DrawCard, () => DrawCards(httc, 2), onlyDisplayIfTrue: CanDrawCards(httc), repeatDecisionText: "draw 2 cards"));
                options.Add(new Function(httc, "Your hero regains 2 HP", SelectionType.GainHP, () => GameController.SelectAndGainHP(httc, 2, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Owner == tt), repeatDecisionText: "your hero regains 2 HP"));
                SelectFunctionDecision choice = new SelectFunctionDecision(GameController, httc, options, false, cardSource: GetCardSource());
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
