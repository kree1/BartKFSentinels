using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class ReclusiveInformantCardController : EmpireUtilityCardController
    {
        public ReclusiveInformantCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }
        private ITrigger reduceDamageTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals itself 2 psychic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageWithReductionResponse, TriggerType.DealDamage);
            // "One player may destroy an Equipment card to reduce this damage by 2."
            reduceDamageTrigger = AddTrigger(new ReduceDamageTrigger(base.GameController, EndOfTurnDamageCriteria, null, DestroyedReduceResponse, false, false, GetCardSource()));
            // "At the start of the environment turn, one player may draw 3 cards."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, OnePlayerDrawsResponse, TriggerType.DrawCard);
        }

        private const string DidDestroyToReduce = "DidDestroyToReduce";

        private bool EndOfTurnDamageCriteria(DealDamageAction dda)
        {
            return dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.IsSameCard(base.Card) && dda.OriginalAmount == 2 && dda.OriginalDamageType == DamageType.Psychic && dda.CardSource.Card == base.Card && Journal.GetCardPropertiesBoolean(base.Card, DidDestroyToReduce) == true;
        }

        public IEnumerator DamageWithReductionResponse(PhaseChangeAction pca)
        {
            // "... this card deals itself 2 psychic damage. One player may destroy an Equipment card to reduce this damage by 2."
            DealDamageAction previewDamage = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), base.Card, 2, DamageType.Psychic);
            IEnumerable<Card> eqpInPlay = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.DoKeywordsContain("equipment") && c.IsInPlayAndHasGameText), visibleToCard: GetCardSource());
            //Log.Debug("eqpInPlay.Count(): " + eqpInPlay.Count().ToString());
            //Log.Debug("eqpInPlay.Any(): " + eqpInPlay.Any().ToString());
            if (eqpInPlay.Any())
            {
                List<YesNoCardDecision> chooseToDestroy = new List<YesNoCardDecision>();
                IEnumerator decideCoroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.Custom, base.Card, action: previewDamage, storedResults: chooseToDestroy, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(decideCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(decideCoroutine);
                }
                if (DidPlayerAnswerYes(chooseToDestroy))
                {
                    List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
                    IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.DoKeywordsContain("equipment"), "Equipment"), true, storedResultsAction: destroyed, responsibleCard: base.Card, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                    //Log.Debug("ReclusiveInformantCardController.DamageWithReductionResponse: DidDestroyCard(destroyed): " + DidDestroyCard(destroyed).ToString());
                    //Log.Debug("ReclusiveInformantCardController.DamageWithReductionResponse: IsRealAction(): " + IsRealAction().ToString());
                    if (DidDestroyCard(destroyed) && IsRealAction())
                    {
                        //Log.Debug("DidDestroyToReduce: " + Journal.GetCardPropertiesBoolean(base.Card, DidDestroyToReduce).ToString());
                        //Log.Debug("ReclusiveInformantCardController.DamageWithReductionResponse: setting DidDestroyToReduce to true");
                        Journal.RecordCardProperties(base.Card, DidDestroyToReduce, true);
                        //Log.Debug("DidDestroyToReduce: " + Journal.GetCardPropertiesBoolean(base.Card, DidDestroyToReduce).ToString());
                    }
                }
            }

            //Log.Debug("ReclusiveInformantCardController.DamageWithReductionResponse: initiating damage");
            IEnumerator damageCoroutine = DealDamage(base.Card, base.Card, 2, DamageType.Psychic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }

        public IEnumerator DestroyedReduceResponse(DealDamageAction dda)
        {
            // "... to reduce this damage by 2."
            //Log.Debug("ReclusiveInformantCardController.DestroyedReduceResponse called");
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 2, reduceDamageTrigger, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            if (IsRealAction())
            {
                //Log.Debug("DidDestroyToReduce: " + Journal.GetCardPropertiesBoolean(base.Card, DidDestroyToReduce).ToString());
                //Log.Debug("ReclusiveInformantCardController.DestroyedReduceResponse: setting DidDestroyToReduce to false");
                SetCardProperty(DidDestroyToReduce, false);
                //Log.Debug("DidDestroyToReduce: " + Journal.GetCardPropertiesBoolean(base.Card, DidDestroyToReduce).ToString());
                //Log.Debug("ReclusiveInformantCardController.DestroyedReduceResponse finished");
            }
        }

        public IEnumerator OnePlayerDrawsResponse(GameAction ga)
        {
            // "... one player may draw 3 cards."
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCards(DecisionMaker, 3, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            yield break;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Do you want to destroy an Equipment card to reduce the damage?", "Should they destroy an Equipment card and reduce the damage?", "Vote for whether to destroy an Equipment card and reduce damage", "destroying an Equipment card and reducing damage", true);
        }
    }
}
