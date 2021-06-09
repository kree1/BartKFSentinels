using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class BodyAndMindCardController : FractureUtilityCardController
    {
        public BodyAndMindCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsInPlay(BreachCard());
            base.SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, BreachCard());
        }

        public override IEnumerator Play()
        {
            // "If there are 3 or more Breach cards in play, increase that damage by 3."
            ITrigger breachesInPlay = AddIncreaseDamageTrigger((DealDamageAction dda) => base.GameController.FindCardsWhere((Card c) => IsBreach(c) && c.IsInPlayAndHasGameText, visibleToCard: GetCardSource()).Count() >= 3 && dda.CardSource.CardController == this, 3);

            // "If there are 4 or more Breach cards in your trash, increase that damage by 4."
            ITrigger breachesInTrash = AddIncreaseDamageTrigger((DealDamageAction dda) => base.TurnTaker.Trash.Cards.Where((Card c) => IsBreach(c)).Count() >= 4 && dda.CardSource.CardController == this, 4);

            // "{FractureCharacter} deals 1 target 3 melee damage."
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), (Card c) => 3, DamageType.Melee, () => 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }

            RemoveTrigger(breachesInPlay);
            RemoveTrigger(breachesInTrash);
            yield break;
        }
    }
}
