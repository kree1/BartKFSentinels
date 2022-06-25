using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public class MarksmanCardController : RenownCardController
    {
        public MarksmanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of this play area's turn, this hero's player may increase the next damage dealt by this hero by 2."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == Card.Location.HighestRecursiveLocation.OwnerTurnTaker && GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsHeroCharacterCard && GetCardThisCardIsNextTo().IsTarget, OptionalIncreaseNextDamageResponse, TriggerType.CreateStatusEffect);
        }

        private IEnumerator OptionalIncreaseNextDamageResponse(PhaseChangeAction pca)
        {
            // "... this hero's player may increase the next damage dealt by this hero by 2."
            YesNoAmountDecision yesno = new YesNoAmountDecision(GameController, GameController.FindTurnTakerController(GetCardThisCardIsNextTo().Owner).ToHero(), SelectionType.Custom, 2, cardSource: GetCardSource());
            IEnumerator decideCoroutine = GameController.MakeDecisionAction(yesno);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(decideCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(decideCoroutine);
            }
            if (DidPlayerAnswerYes(yesno))
            {
                IncreaseDamageStatusEffect aim = new IncreaseDamageStatusEffect(2);
                aim.SourceCriteria.IsSpecificCard = GetCardThisCardIsNextTo();
                aim.NumberOfUses = 1;
                IEnumerator aimCoroutine = AddStatusEffect(aim);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(aimCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(aimCoroutine);
                }
            }
            yield break;
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            string turnTakerName = decision.DecisionMaker.Name;
            string heroName = decision.DecisionMaker.CharacterCard.Title;
            if (GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsHeroCharacterCard)
            {
                heroName = GetCardThisCardIsNextTo().Title;
            }
            return new CustomDecisionText("Do you want to increase the next damage dealt by " + heroName + " by 2?", "Should " + turnTakerName + " increase the next damage dealt by " + heroName + " by 2?", "Vote whether to increase the next damage dealt by " + heroName + " by 2", "increasing the next damage dealt by " + heroName + " by 2");
        }
    }
}
