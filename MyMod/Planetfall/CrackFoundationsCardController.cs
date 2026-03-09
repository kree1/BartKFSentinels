using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class CrackFoundationsCardController : CardController
    {
        public CrackFoundationsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override IEnumerator Play()
        {
            // "{Planetfall} deals the hero target with the highest HP {H} melee damage, ..."
            IEnumerator meleeCoroutine = DealDamageToHighestHP(CharacterCard, 1, (Card c) => IsHeroTarget(c), (Card c) => H, DamageType.Melee);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "... then deals each target 1 projectile damage."
            IEnumerator projectileCoroutine = DealDamage(CharacterCard, (Card c) => c.IsTarget, 1, DamageType.Projectile);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(projectileCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(projectileCoroutine);
            }
            // "Destroy {H - 1} hero Ongoing cards."
            LinqCardCriteria heroOngoingInPlay = new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c) && c.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), "hero Ongoing");
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, heroOngoingInPlay, H - 1, requiredDecisions: H - 1, allowAutoDecide: base.GameController.FindCardsWhere(heroOngoingInPlay, visibleToCard: GetCardSource()).Count() <= H - 1, responsibleCard: base.Card, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
