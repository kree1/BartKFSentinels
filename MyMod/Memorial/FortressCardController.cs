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
    public class FortressCardController : RenownCardController
    {
        public FortressCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of this play area's turn, this hero may regain 2 HP."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.HighestRecursiveLocation.OwnerTurnTaker && GetCardThisCardIsNextTo() != null && GetCardThisCardIsNextTo().IsTarget && GetCardThisCardIsNextTo().IsHeroCharacterCard, OptionalHealResponse, TriggerType.GainHP);
        }

        private IEnumerator OptionalHealResponse(PhaseChangeAction pca)
        {
            // "... this hero may regain 2 HP."
            if (GetCardThisCardIsNextTo().HitPoints < GetCardThisCardIsNextTo().MaximumHitPoints)
            {
                YesNoAmountDecision yesno = new YesNoAmountDecision(GameController, GameController.FindTurnTakerController(GetCardThisCardIsNextTo().Owner).ToHero(), SelectionType.GainHP, 2, cardSource: GetCardSource());
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
                    IEnumerator healCoroutine = GameController.GainHP(GetCardThisCardIsNextTo(), 2, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(healCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(healCoroutine);
                    }
                }
            }
            else
            {
                IEnumerator messageCoroutine = GameController.SendMessageAction(GetCardThisCardIsNextTo().Title + " is already at maximum HP.", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }
    }
}
