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
    public class CounterfrequencyCardController : CostCardController
    {
        public CounterfrequencyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Select a target. Prevent the next damage dealt by that target. Draw 3 cards."
            int numDraws = GetPowerNumeral(0, 3);
            List<SelectCardDecision> targetChoices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.SelectTargetNoDamage, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget), targetChoices, false, cardSource: GetCardSource());
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
                Card targetChosen = targetChoices.FirstOrDefault().SelectedCard;
                CannotDealDamageStatusEffect cancel = new CannotDealDamageStatusEffect();
                cancel.SourceCriteria.IsSpecificCard = targetChosen;
                cancel.IsPreventEffect = true;
                cancel.NumberOfUses = 1;
                cancel.UntilTargetLeavesPlay(targetChosen);
                IEnumerator statusCoroutine = AddStatusEffect(cancel);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            IEnumerator drawCoroutine = DrawCards(DecisionMaker, numDraws);
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
