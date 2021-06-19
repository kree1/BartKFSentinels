using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class HospitalBallCardController : TheGoalieUtilityCardController
    {
        public HospitalBallCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{TheGoalieCharacter} deals 1 target 4 projectile damage."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 4, DamageType.Projectile, new int?(1), false, new int?(1), storedResultsDamage: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "You may destroy a Goalposts card."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, GoalpostsCards, true, storedResultsAction: destroyed, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If a Goalposts card is in play, the damaged target deals {TheGoalieCharacter} 2 irreducible melee damage."
            if (base.GameController.FindCardsWhere(GoalpostsInPlay).Any())
            {
                IEnumerable<DealDamageAction> viableDamages = damageResults.Where((DealDamageAction dd) => dd.DidDealDamage);
                IEnumerable<Card> damagedTargetsInPlay = (from dd in damageResults
                                                          where viableDamages.Contains(dd) && dd.Target.IsInPlay && dd.Target.IsTarget
                                                          select dd.Target).Distinct();
                if (damagedTargetsInPlay.Any())
                {
                    List<SelectCardDecision> cardSelection = new List<SelectCardDecision>();
                    IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.CardToDealDamage, (from dd in damageResults
                                                                                                                                                where viableDamages.Contains(dd) && dd.Target.IsInPlay && dd.Target.IsTarget
                                                                                                                                                select dd.Target).Distinct(), cardSelection);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(selectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(selectCoroutine);
                    }
                    SelectCardDecision choice = cardSelection.FirstOrDefault();
                    if (choice != null && choice.SelectedCard != null)
                    {
                        IEnumerator meleeCoroutine = base.GameController.DealDamage(base.HeroTurnTakerController, choice.SelectedCard, (Card c) => c == base.CharacterCard, 2, DamageType.Melee, isIrreducible: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(meleeCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(meleeCoroutine);
                        }
                    }
                }
                else
                {
                    // No damaged targets are still in play
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are no damaged targets still active. " + base.CharacterCard.Title + "'s risky move paid off- she takes no damage!", Priority.Medium, GetCardSource(), showCardSource: true);
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
            yield break;
        }
    }
}
