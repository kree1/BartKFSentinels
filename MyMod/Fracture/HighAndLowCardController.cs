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
    public class HighAndLowCardController : FractureUtilityCardController
    {
        public HighAndLowCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            int numTargets = GetPowerNumeral(0, 1);
            int damageAmt = GetPowerNumeral(1, 2);
            // "1 target deals itself 2 irreducible projectile damage."
            List<SelectCardsDecision> selection = new List<SelectCardsDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardsAndStoreResults(base.HeroTurnTakerController, SelectionType.DealDamageSelf, (Card c) => c.IsInPlayAndHasGameText && c.IsTarget, numTargets, selection, false, requiredDecisions: numTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Log.Debug("selection.Count(): " + selection.Count().ToString());
            Log.Debug("selection.Where((SelectCardsDecision dec) => dec != null).Count(): " + selection.Where((SelectCardsDecision dec) => dec != null).Count().ToString());
            Log.Debug("selection.Where((SelectCardsDecision dec) => dec != null && dec.SelectedCard != null).Count(): " + selection.Where((SelectCardsDecision dec) => dec != null && dec.SelectedCard != null).Count().ToString());
            List<Card> selectedTargets = GetSelectedCards(selection).ToList();
            Log.Debug("selectedTargets.Count(): " + selectedTargets.Count().ToString());
            IEnumerator selfDamageCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => selectedTargets.Contains(c), 2, DamageType.Projectile, isIrreducible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDamageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDamageCoroutine);
            }
            // "You may destroy this card. If you do, cards from that target's deck cannot be played until the start of your turn."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, optional: true, responsibleCard: base.Card, postDestroyAction: () => PreventPlayResponse(selectedTargets), associatedCards: selectedTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            yield break;
        }

        public IEnumerator PreventPlayResponse(List<Card> targets)
        {
            // "... cards from that target's deck cannot be played until the start of your turn."
            foreach (Card c in targets)
            {
                CannotPlayCardsStatusEffect hinder = new CannotPlayCardsStatusEffect();
                hinder.CardCriteria.NativeDeck = c.NativeDeck;
                hinder.UntilStartOfNextTurn(base.TurnTaker);
                IEnumerator statusCoroutine = AddStatusEffect(hinder);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            yield break;
        }
    }
}
