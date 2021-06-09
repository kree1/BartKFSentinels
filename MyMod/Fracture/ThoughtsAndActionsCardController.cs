using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class ThoughtsAndActionsCardController : FractureUtilityCardController
    {
        public ThoughtsAndActionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may discard a card. If you do, up to 3 players each draw a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardDrawResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DrawCard });
        }

        public IEnumerator DiscardDrawResponse(GameAction ga)
        {
            // "... you may discard a card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, optional: true, storedResults: discards, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, 1))
            {
                // "If you do, up to 3 players each draw a card."
                IEnumerator drawCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt.IsHero), SelectionType.DrawCard, (TurnTaker tt) => DrawCard(tt.ToHero(), optional: true), 3, optional: false, 0, null, allowAutoDecide: false, null, null, null, ignoreBattleZone: false, null, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawCoroutine);
                }
            }
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Another hero may use a power now."
            IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(base.HeroTurnTakerController, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(powerCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(powerCoroutine);
            }
            yield break;
        }
    }
}
