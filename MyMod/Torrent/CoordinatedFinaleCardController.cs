using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class CoordinatedFinaleCardController : TorrentUtilityCardController
    {
        public CoordinatedFinaleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"), "Cluster"));
        }

        public override void AddTriggers()
        {
            // "At the start of your turn, {TorrentCharacter} may deal 1 target X projectile damage, where X = the number of Clusters in play. If she deals damage this way, destroy any number of Clusters, then destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDestroySequence, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard, TriggerType.DestroySelf });
            // "After {TorrentCharacter} uses a power, you may destroy up to 2 Clusters."
            AddTrigger<UsePowerAction>((UsePowerAction upa) => upa.HeroUsingPower == base.HeroTurnTakerController && upa.IsSuccessful, DestroyClustersResponse, TriggerType.DestroyCard, TriggerTiming.After);
            base.AddTriggers();
        }

        public IEnumerator DamageDestroySequence(GameAction ga)
        {
            // "... {TorrentCharacter} may deal 1 target X projectile damage, where X = the number of Clusters in play."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), NumberOfClusters(), DamageType.Projectile, 1, false, 0, storedResultsDamage: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If she deals damage this way..."
            if (DidDealDamage(damageResults))
            {
                // "... destroy any number of Clusters..."
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("cluster"), "cluster"), null, requiredDecisions: 0, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
                // "... then destroy this card."
                IEnumerator selfDestructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DestroyClustersResponse(GameAction ga)
        {
            // "... you may destroy up to 2 Clusters."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("cluster"), "cluster"), 2, requiredDecisions: 0, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        private int NumberOfClusters()
        {
            return FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("cluster")).Count();
        }
    }
}
