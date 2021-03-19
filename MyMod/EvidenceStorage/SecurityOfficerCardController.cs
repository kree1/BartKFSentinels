using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class SecurityOfficerCardController : OfficerCardController
    {
        public SecurityOfficerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OneStoragePerTurn), () => base.Card.Title + " has prevented damage to a Storage card this turn", () => base.Card.Title + " has not prevented damage to a Storage card this turn").Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OneDevicePerTurn), () => base.Card.Title + " has prevented damage to a Device card this turn", () => base.Card.Title + " has not prevented damage to a Device card this turn").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        protected const string OneStoragePerTurn = "PreventStorageDamageOnce";
        protected const string OneDevicePerTurn = "PreventDeviceDamageOnce";

        public override void AddTriggers()
        {
            // "The first time a Storage card would be dealt damage each turn, prevent that damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OneStoragePerTurn) && dda.Target.DoKeywordsContain("storage"), PreventStorageDamage, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            // "The first time a Device card would be dealt damage each turn, prevent that damage."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OneDevicePerTurn) && dda.Target.DoKeywordsContain("device"), PreventDeviceDamage, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            base.AddTriggers();
        }

        public IEnumerator PreventStorageDamage(DealDamageAction dda)
        {
            // "The first time a Storage card would be dealt damage each turn, prevent that damage."
            base.SetCardPropertyToTrueIfRealAction(OneStoragePerTurn);
            IEnumerator preventCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            yield break;
        }

        public IEnumerator PreventDeviceDamage(DealDamageAction dda)
        {
            // "The first time a Device card would be dealt damage each turn, prevent that damage."
            base.SetCardPropertyToTrueIfRealAction(OneDevicePerTurn);
            IEnumerator preventCoroutine = GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(preventCoroutine);
            }
            yield break;
        }
    }
}
