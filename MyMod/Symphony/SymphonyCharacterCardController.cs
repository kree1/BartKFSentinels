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
    public class SymphonyCharacterCardController : HeroCharacterCardController
    {
        public SymphonyCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string MeasureKeyword = "measure";
        public readonly string SilenceKeyword = "silence";

        public override IEnumerator UsePower(int index = 0)
        {
            // "You may play a measure card. You may draw a card."
            IEnumerator playCoroutine = GameController.SelectAndPlayCardFromHand(DecisionMaker, true, cardCriteria: new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MeasureKeyword), MeasureKeyword), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
            IEnumerator drawCoroutine = DrawCard(optional: true);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may draw 3 cards. If they do, they discard 2 cards."
            return GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(GameController, DecisionMaker, FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && CanDrawCards(FindTurnTakerController(tt).ToHero())), SelectionType.DrawAndDiscardCards, cardSource: GetCardSource()), DrawDiscardResponse);
        }

        public IEnumerator DrawDiscardResponse(TurnTaker tt)
        {
            // "[tt] may draw 3 cards. If they do, they discard 2 cards."
            HeroTurnTakerController httc = FindTurnTakerController(tt).ToHero();
            List<DrawCardAction> drawResults = new List<DrawCardAction>();
            IEnumerator drawCoroutine = DrawCards(httc, 3, storedResults: drawResults, optional: true);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(drawCoroutine);
            }
            if (DidDrawCards(drawResults, 3))
            {
                IEnumerator discardCoroutine = GameController.SelectAndDiscardCards(httc, 2, false, null, allowAutoDecide: 2 >= tt.ToHero().Hand.Cards.Count(), cardSource: GetCardSource());
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

        private IEnumerator UseIncapOption2()
        {
            // "One hero may use a power."
            return GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption3()
        {
            // "Select a target. Reduce the next damage dealt to that target by 2."
            List<SelectCardDecision> targetChoices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReduceNextDamageTaken, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget), targetChoices, false, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (targetChoices.Any())
            {
                Card selected = GetSelectedCard(targetChoices);
                ReduceDamageStatusEffect buff = new ReduceDamageStatusEffect(2);
                buff.TargetCriteria.IsSpecificCard = selected;
                buff.NumberOfUses = 1;
                buff.UntilTargetLeavesPlay(selected);
                IEnumerator statusCoroutine = AddStatusEffect(buff);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
        }
    }
}
