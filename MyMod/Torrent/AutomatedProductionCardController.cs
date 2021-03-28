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
    public class AutomatedProductionCardController : TorrentUtilityCardController
    {
        public AutomatedProductionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"), "Cluster"));
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.HeroTurnTaker.Hand, new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"), "Cluster"));
        }

        public override IEnumerator Play()
        {
            LinqCardCriteria isCluster = new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"));
            // "Reveal cards from the top of your deck until 3 Clusters are revealed. Put the revealed Clusters into your hand and shuffle the other revealed cards into your deck."
            IEnumerator manipulateCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, false, false, true, isCluster, 3, revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(manipulateCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(manipulateCoroutine);
            }
            // "{TorrentCharacter} may deal herself 2 energy damage. If she takes damage this way, you may play a Cluster."
            List<DealDamageAction> damageResults = new List<DealDamageAction>();
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, base.CharacterCard, 2, DamageType.Energy, optional: true, storedResults: damageResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            if (damageResults != null && damageResults.FirstOrDefault() != null)
            {
                DealDamageAction selfDamage = damageResults.FirstOrDefault();
                if (selfDamage != null && selfDamage.DidDealDamage && selfDamage.Target == base.CharacterCard)
                {
                    IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, cardCriteria: isCluster);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
