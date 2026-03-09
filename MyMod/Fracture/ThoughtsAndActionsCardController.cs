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
            IEnumerator discardCoroutine = GameController.SelectAndDiscardCard(base.HeroTurnTakerController, optional: true, storedResults: discards, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            if (DidDiscardCards(discards, 1))
            {
                // "If you do, up to 3 players each draw a card."
                IEnumerator drawCoroutine = GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && IsHero(tt)), SelectionType.DrawCard, (TurnTaker tt) => DrawCard(tt.ToHero(), optional: true), 3, optional: false, 0, null, allowAutoDecide: false, null, null, null, ignoreBattleZone: false, null, GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            // "{Fracture} deals herself 1 psychic damage."
            int psychicAmt = GetPowerNumeral(0, 1);
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator psychicCoroutine = DealDamage(CharacterCard, CharacterCard, psychicAmt, DamageType.Psychic, storedResults: damageResults, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(psychicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(psychicCoroutine);
            }
            // "If she was dealt damage this way, another hero may use a power now."
            if (DidDealDamage(damageResults, toSpecificTarget: CharacterCard))
            {
                IEnumerator powerCoroutine = GameController.SelectHeroToUsePower(base.HeroTurnTakerController, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker), cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
        }
    }
}
