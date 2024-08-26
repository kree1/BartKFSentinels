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
    public class BombardmentCardController : CardController
    {
        public BombardmentCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero with most non-character cards in play
            SpecialStringMaker.ShowHeroWithMostCards(false, new LinqCardCriteria((Card c) => !c.IsCharacter, "non-character"));
        }

        public override IEnumerator Play()
        {
            // "{TheEqualizer} deals the hero with the most non-character cards in play {H - 1} sonic damage, ..."
            IEnumerator sonicCoroutine = DealDamageToMostCardsInPlay(CharacterCard, 1, new LinqCardCriteria((Card c) => IsHeroCharacterCard(c), "hero character"), H - 1, DamageType.Sonic, cardInPlayCriteria: (Card c) => !c.IsCharacter);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(sonicCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(sonicCoroutine);
            }
            // "... then deals each hero target 2 fire damage."
            IEnumerator fireCoroutine = GameController.DealDamage(DecisionMaker, CharacterCard, (Card c) => IsHeroTarget(c), 2, DamageType.Fire, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(fireCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(fireCoroutine);
            }
            // "Destroy {H} Equipment cards."
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => IsEquipment(c), "Equipment"), H, responsibleCard: Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
