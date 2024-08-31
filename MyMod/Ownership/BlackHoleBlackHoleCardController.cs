using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class BlackHoleBlackHoleCardController : ExpansionWeatherCardController
    {
        public BlackHoleBlackHoleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show amount of damage dealt to non-hero targets by hero targets this turn
            SpecialStringMaker.ShowSpecialString(() => DamageDealtToNonHeroByHeroThisTurn() + " damage has been dealt to non-hero targets by hero targets this turn.");
        }

        public readonly string TenRunsThisTurn = "TenRunsThisTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When 12 or more damage is dealt to non-hero targets by hero targets in one turn, destroy a non-target villain Modification, chosen at random, then {OwnershipCharacter} regains 12 HP."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(TenRunsThisTurn) && DamageDealtToNonHeroByHeroThisTurn() >= 12 && !IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DidDealDamage, SwallowResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.GainHP }, TriggerTiming.After);
        }

        public IEnumerator SwallowResponse(DealDamageAction dda)
        {
            SetCardProperty(TenRunsThisTurn, true);
            // "... destroy a non-target villain Modification, chosen at random, ..."
            List<DestroyCardAction> nullified = new List<DestroyCardAction>();
            List<Card> available = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(ModificationKeyword) && !c.IsTarget && c.IsInPlayAndHasGameText, "non-target Modification", singular: "card in play", plural: "cards in play"), visibleToCard: GetCardSource()).ToList();
            int count = available.Count;
            if (count > 0)
            {
                IEnumerable<Card> toNullify = available.TakeRandom(count, base.GameController.Game.RNG);
                if (toNullify.Count() > 0)
                {
                    IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, toNullify.First(), storedResults: nullified, responsibleCard: base.Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                }
                if (nullified.Count() > 0)
                {
                    Card first = nullified.FirstOrDefault().CardToDestroy.Card;
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " nullified " + first.Title + "!", Priority.Medium, GetCardSource(), first.ToEnumerable(), true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                }
            }
            // "... then {OwnershipCharacter} regains 12 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(FindCard(OwnershipIdentifier), 12, cardSource: GetCardSource());
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
}
