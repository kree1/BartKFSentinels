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
    public class TechnoInfiltrationCardController : CardController
    {
        public TechnoInfiltrationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "{Planetfall} deals each hero target X lightning damage, where X = the number of Equipment and/or Device cards from that target's deck in play plus 2."
            IEnumerator lightningCoroutine = DealDamage(CharacterCard, (Card c) => IsHeroTarget(c), (Card c) => GameController.FindCardsWhere((Card e) => e.NativeDeck == c.NativeDeck && (IsEquipment(e) || e.IsDevice)).Count(), DamageType.Lightning);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(lightningCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(lightningCoroutine);
            }
            // "Destroy {H - 1} Equipment cards."
            LinqCardCriteria equipmentInPlay = new LinqCardCriteria((Card c) => IsEquipment(c) && c.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), "Equipment");
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, equipmentInPlay, H - 1, requiredDecisions: H - 1, allowAutoDecide: base.GameController.FindCardsWhere(equipmentInPlay, visibleToCard: GetCardSource()).Count() <= H - 1, responsibleCard: base.Card, cardSource: GetCardSource());
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
