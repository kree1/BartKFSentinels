using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Palmreader
{
    public class SignalFlareCardController : PalmreaderUtilityCardController
    {
        public SignalFlareCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsRelay(c) && c.IsInPlayAndHasGameText && !c.Location.IsHero), specifyPlayAreas: true).Condition = () => NumRelaysInNonHeroPlayAreas() > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Relay cards in non-hero play areas.").Condition = () => NumRelaysInNonHeroPlayAreas() <= 0;
        }

        public override IEnumerator Play()
        {
            // "{TheGoalieCharacter} deals each non-hero target 2 psychic damage."
            IEnumerator damageCoroutine = DealDamage(base.CharacterCard, (Card c) => !c.IsHero, 2, DamageType.Psychic, optional: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Destroy a Relay card in a non-hero play area."
            if (NumRelaysInNonHeroPlayAreas() > 0)
            {
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && IsRelay(c) && !c.Location.IsHero), false, responsibleCard: base.Card, cardSource: GetCardSource());
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
    }
}
