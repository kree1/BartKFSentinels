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
    public class DueRespectCardController : MemorialUtilityCardController
    {
        public DueRespectCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Renowns in the villain deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsRenown(c), "Renown"));
            // Identify hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override IEnumerator Play()
        {
            // "Reveal cards from the top of the villain deck until a Renown is revealed. Play it. Shuffle the other revealed cards back into the villain deck."
            IEnumerator playCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(base.TurnTakerController, base.TurnTaker.Deck, true, false, false, new LinqCardCriteria((Card c) => IsRenown(c), "Renown"), 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "{Memorial} deals the hero target with the highest HP {H} melee damage."
            IEnumerator damageCoroutine = base.DealDamageToHighestHP(base.CharacterCard, 1, (Card c) => c.IsHero, (Card c) => H, DamageType.Melee, numberOfTargets: () => 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
