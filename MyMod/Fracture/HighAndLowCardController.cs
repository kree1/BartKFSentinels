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
            //Log.Debug("selection.Count(): " + selection.Count().ToString());
            //Log.Debug("selection.Where((SelectCardsDecision dec) => dec != null).Count(): " + selection.Where((SelectCardsDecision dec) => dec != null).Count().ToString());
            //Log.Debug("selection.Where((SelectCardsDecision dec) => dec != null && dec.SelectedCard != null).Count(): " + selection.Where((SelectCardsDecision dec) => dec != null && dec.SelectedCard != null).Count().ToString());
            List<Card> selectedTargets = GetSelectedCards(selection).ToList();
            //Log.Debug("selectedTargets.Count(): " + selectedTargets.Count().ToString());
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
            TurnTaker youDefinition = base.TurnTaker;
            Func<GameAction, IEnumerator> preventPlayAction = AddBeforeDestroyAction((GameAction ga) => PreventPlayResponse(selectedTargets, youDefinition));
            IEnumerator destructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, optional: true, responsibleCard: base.Card, associatedCards: selectedTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
            RemoveDestroyAction(BeforeOrAfter.Before, preventPlayAction);
            yield break;
        }

        public IEnumerator PreventPlayResponse(List<Card> targets, TurnTaker player)
        {
            // "... cards from that target's deck cannot be played until the start of your turn."
            foreach (Card c in targets)
            {
                //Log.Debug("Creating CannotPlayCardsStatusEffect for " + c.Title);
                //Log.Debug(c.Title + "'s associated deck is " + GetNativeDeck(c).Identifier);
                //Log.Debug("'You' in this power is " + player.Identifier);
                CannotPlayCardsStatusEffect hinder = new CannotPlayCardsStatusEffect();
                hinder.CardCriteria.NativeDeck = GetNativeDeck(c);
                hinder.UntilStartOfNextTurn(player);
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
        }
    }
}
