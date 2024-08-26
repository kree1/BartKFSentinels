using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class WalkingArsenalCardController : EqualizerUtilityCardController
    {
        public WalkingArsenalCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show list of Munition cards in villain trash
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Trash, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword), "Munition"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Redirect damage dealt by {TheEqualizer} to the hero target with the highest HP."
            AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard), RedirectToHighestResponse, TriggerType.RedirectDamage, TriggerTiming.Before);
            // "At the end of the villain turn, shuffle the villain trash and reveal cards until 2 Munitions are revealed. Put the revealed Munitions into play and the rest back into the trash. If fewer than 2 cards entered play this way, {TheEqualizer} deals each hero target 2 projectile damage."
            // "At the end of the villain turn, destroy this card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, PlayMunitionsDestructResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        public IEnumerator RedirectToHighestResponse(DealDamageAction dda)
        {
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsHeroTarget(c), results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highest = results.FirstOrDefault();
            if (highest != null)
            {
                IEnumerator redirectCoroutine = GameController.RedirectDamage(dda, highest, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(redirectCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(redirectCoroutine);
                }
            }
        }

        public IEnumerator PlayMunitionsDestructResponse(PhaseChangeAction pca)
        {
            // "... shuffle the villain trash and reveal cards until 2 Munitions are revealed. Put the revealed Munitions into play and the rest back into the trash."
            List<Card> played = new List<Card>();
            IEnumerator revealCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(TurnTakerController, TurnTaker.Trash, false, true, false, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword), "Munition"), 2, shuffleBeforehand: true, storedPlayResults: played);
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "If fewer than 2 cards entered play this way, {TheEqualizer} deals each hero target 2 projectile damage."
            if (played.Count < 2)
            {
                IEnumerator projectileCoroutine = DealDamage(CharacterCard, (Card c) => IsHeroTarget(c), (Card c) => 2, DamageType.Projectile);
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(projectileCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(projectileCoroutine);
                }
            }
            // "... destroy this card."
            IEnumerator destructCoroutine = GameController.DestroyCard(DecisionMaker, Card, responsibleCard: Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
