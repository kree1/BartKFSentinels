using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class PityCardController : CardController
    {
        public PityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a hero character would be reduced to 0 or fewer HP, restore them to their maximum HP instead and destroy this card."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.IsHeroCharacterCard && dda.Target.HitPoints.HasValue && dda.Target.HitPoints.Value - dda.Amount <= 0 && dda.IsSuccessful, RestoreDestructResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.CardToDestroy.Card.IsHeroCharacterCard && dca.CardToDestroy.Card.HitPoints.HasValue && dca.CardToDestroy.Card.HitPoints.Value <= 0, RestoreDestructResponse, TriggerType.CancelAction, TriggerTiming.Before);
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.IsHeroCharacterCard && dda.Target.HitPoints.HasValue && dda.Target.HitPoints.Value <= 0, RestoreDestructResponse, TriggerType.GainHP, TriggerTiming.After);
            // "When this card is destroyed, increase damage dealt to hero targets during the next villain turn by 2."
            AddWhenDestroyedTrigger((DestroyCardAction dca) => InitiateIncreaseResponse(dca), TriggerType.CreateStatusEffect);
        }

        public IEnumerator RestoreDestructResponse(GameAction ga)
        {
            // "... restore them to their maximum HP instead and destroy this card."
            Card characterInPeril = null;
            if (ga is DealDamageAction)
            {
                characterInPeril = (ga as DealDamageAction).Target;
            }
            else if (ga is SetHPAction)
            {
                characterInPeril = (ga as SetHPAction).HpGainer;
            }
            if (characterInPeril != null)
            {
                IEnumerator cancelCoroutine = base.GameController.CancelAction(ga, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                IEnumerator restoreCoroutine = base.GameController.SetHP(characterInPeril, characterInPeril.MaximumHitPoints.Value, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(restoreCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(restoreCoroutine);
                }
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("The PODS send a gift to the heroes. Their Pity ends!", Priority.Medium, GetCardSource(), associatedCards: new Card[] { base.CharacterCard }, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, actionSource: ga, responsibleCard: base.Card, associatedCards: new List<Card>(new Card[] { characterInPeril }), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destructCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destructCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator InitiateIncreaseResponse(DestroyCardAction dca)
        {
            // "... increase damage dealt to hero targets during the next villain turn by 2."
            OnPhaseChangeStatusEffect pityEnds = new OnPhaseChangeStatusEffect(base.Card, nameof(IncreaseDamageResponse), "Increase damage dealt to hero targets during the next villain turn by 2.", new TriggerType[] { TriggerType.IncreaseDamage }, base.Card);
            pityEnds.TurnTakerCriteria.IsVillain = true;
            pityEnds.TurnIndexCriteria.GreaterThan = base.Game.TurnIndex;
            pityEnds.TurnPhaseCriteria.TurnTaker = base.TurnTaker;
            pityEnds.NumberOfUses = 1;
            pityEnds.CanEffectStack = true;
            IEnumerator initialStatusCoroutine = base.GameController.AddStatusEffect(pityEnds, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(initialStatusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(initialStatusCoroutine);
            }
            yield break;
        }

        public IEnumerator IncreaseDamageResponse(PhaseChangeAction pca, OnPhaseChangeStatusEffect sourceEffect)
        {
            // "... increase damage dealt to hero targets during [this turn] by 2."
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("[b]NOW WATCH{BR}WITNESS TRUE POWER[/b]", Priority.Medium, GetCardSource(), new Card[] { base.CharacterCard }, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IncreaseDamageStatusEffect pitiless = new IncreaseDamageStatusEffect(2);
            pitiless.UntilThisTurnIsOver(base.Game);
            pitiless.TargetCriteria.IsHero = true;
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(pitiless, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }
    }
}
