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
    public class ChildrenOfNightCardController : CardController
    {
        public ChildrenOfNightCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to this card by 1."
            AddReduceDamageTrigger((Card c) => c == base.Card, 1);
            // "At the end of the villain turn, this card deals the hero target with the highest HP {H - 1} infernal damage and {H - 2} psychic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DealDamageResponse, TriggerType.DealDamage);
        }

        private IEnumerator DealDamageResponse(PhaseChangeAction pca)
        {
            List<DealDamageAction> list = new List<DealDamageAction> { 
                new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, H - 1, DamageType.Infernal),
                new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), null, H - 2, DamageType.Psychic)
            };
            List<Card> storedResults = new List<Card>();
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(list, (Card c) => IsHeroTarget(c), HighestLowestHP.HighestHP);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
        }
    }
}
