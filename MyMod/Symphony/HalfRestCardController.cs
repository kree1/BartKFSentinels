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
            // "Another player may discard a card. If they do, they draw 3 cards."
            IEnumerator selectCoroutine = GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && tt != TurnTaker), SelectionType.DiscardCard, cardSource: GetCardSource()), MayDiscardToDraw);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectCoroutine);
            }
            // "Another player may discard a card. If they do, their hero regains 3 HP."
            selectCoroutine = GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && tt != TurnTaker), SelectionType.DiscardCard, cardSource: GetCardSource()), MayDiscardToHeal);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        public IEnumerator MayDiscardToDraw(TurnTaker tt)
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
            // "If they do, they draw 3 cards."
            if (DidDiscardCards(discardResults))
            {
                IEnumerator chooseCoroutine = DrawCards(httc, 3);
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

        public IEnumerator MayDiscardToHeal(TurnTaker tt)
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
            // "If they do, their hero regains 3 HP."
            if (DidDiscardCards(discardResults))
            {
                IEnumerator healCoroutine = GameController.SelectAndGainHP(httc, 3, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Owner == tt);
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
