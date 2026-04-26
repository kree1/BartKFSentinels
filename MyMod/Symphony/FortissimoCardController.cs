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
    public class FortissimoCardController : BenefitCardController
    {
        public FortissimoCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 2;
        }

        public override IEnumerator OneShotEffect()
        {
            // "Select a target. Increase the next damage dealt by that target to another target by 2."
            List<SelectCardDecision> targetChoices = new List<SelectCardDecision>();
            IEnumerator chooseCoroutine = GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.IncreaseNextDamage, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget), targetChoices, false, cardSource: GetCardSource());
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
                IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(2);
                buff.SourceCriteria.IsSpecificCard = selected;
                buff.TargetCriteria.IsNotSpecificCard = selected;
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
