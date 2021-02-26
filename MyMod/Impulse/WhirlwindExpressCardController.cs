using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class WhirlwindExpressCardController : ImpulseUtilityCardController
    {
        public WhirlwindExpressCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "Draw a card."
            IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            // "Select a target. Until the start of your next turn, reduce damage dealt by that target by 1 and increase damage dealt to that target by 1."
            List<SelectCardDecision> choice = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.SelectTargetNoDamage, new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && GameController.IsCardVisibleToCardSource(c, GetCardSource())), choice, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }

            if (choice != null && choice.Count > 0)
            {
                Card chosen = choice.FirstOrDefault().SelectedCard;
                ReduceDamageStatusEffect reduction = new ReduceDamageStatusEffect(1);
                reduction.SourceCriteria.IsSpecificCard = chosen;
                reduction.UntilStartOfNextTurn(base.TurnTaker);
                reduction.UntilTargetLeavesPlay(chosen);

                IEnumerator reduceCoroutine = base.GameController.AddStatusEffect(reduction, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(reduceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(reduceCoroutine);
                }

                IncreaseDamageStatusEffect enhancement = new IncreaseDamageStatusEffect(1);
                enhancement.TargetCriteria.IsSpecificCard = chosen;
                enhancement.UntilStartOfNextTurn(base.TurnTaker);
                enhancement.UntilTargetLeavesPlay(chosen);

                IEnumerator increaseCoroutine = base.GameController.AddStatusEffect(enhancement, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(increaseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(increaseCoroutine);
                }
            }
            yield break;
        }
    }
}
