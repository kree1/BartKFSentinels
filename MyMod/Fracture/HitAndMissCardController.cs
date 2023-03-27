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
    public class HitAndMissCardController : FractureUtilityCardController
    {
        public HitAndMissCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        public Guid? DestroyToReduce { get; set; }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a non-hero target would deal damage, you may destroy this card. If you do, reduce that damage to 1."
            AddTrigger((DealDamageAction dda) => !base.IsBeingDestroyed && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card.IsTarget && !IsHeroTarget(dda.DamageSource.Card), DestructResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.ReduceDamage }, TriggerTiming.Before);
        }

        public IEnumerator DestructResponse(DealDamageAction dda)
        {
            // "... you may destroy this card. If you do, reduce that damage to 1."
            if (!DestroyToReduce.HasValue || DestroyToReduce.Value != dda.InstanceIdentifier)
            {
                List<YesNoCardDecision> choice = new List<YesNoCardDecision>();
                IEnumerator decideCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.DestroyCard, base.Card, action: dda, storedResults: choice, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(decideCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(decideCoroutine);
                }
                if (DidPlayerAnswerYes(choice))
                {
                    DestroyToReduce = dda.InstanceIdentifier;
                }
            }
            if (DestroyToReduce.HasValue && DestroyToReduce.Value == dda.InstanceIdentifier)
            {
                if (IsRealAction(dda))
                {
                    IEnumerator destructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, responsibleCard: base.Card, postDestroyAction: () => ReduceDamageResponse(dda), cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destructCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destructCoroutine);
                    }
                }
            }
            if (IsRealAction(dda))
            {
                DestroyToReduce = null;
            }
            yield break;
        }

        public IEnumerator ReduceDamageResponse(DealDamageAction dda)
        {
            // "... reduce that damage to 1."
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, dda.Amount - 1, null, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(reduceCoroutine);
            }
            yield break;
        }
    }
}
