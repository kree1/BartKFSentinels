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
    public class MomentaryFugueCardController : PalmreaderUtilityCardController
    {
        public MomentaryFugueCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ShowListOfCardsAtLocationRecursive(base.TurnTaker.PlayArea, new LinqCardCriteria((Card c) => IsRelay(c), "relay")).Condition = () => NumRelaysAt(base.TurnTaker.PlayArea) > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Relay cards in " + base.TurnTaker.NameRespectingVariant + "'s play area.").Condition = () => NumRelaysAt(base.TurnTaker.PlayArea) <= 0;
        }

        public override IEnumerator Play()
        {
            // "Return all Relay cards in your play area to your hand."
            IEnumerator returnCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsRelay(c) && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation.OwnerTurnTaker == base.TurnTaker, "Relay cards in " + base.TurnTaker.Name + "'s play area", false, false, "Relay card in " + base.TurnTaker.Name + "'s play area", "Relay cards in " + base.TurnTaker.Name + "'s play area"), visibleToCard: GetCardSource()), (Card c) => new MoveCardDestination(base.HeroTurnTaker.Hand), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            // "{PalmreaderCharacter} deals 1 target 3 melee damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 3, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "You may play a Relay card or draw 3 cards."
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, "Play a Relay card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.HeroTurnTakerController, cardCriteria: new LinqCardCriteria((Card c) => IsRelay(c), "Relay")), onlyDisplayIfTrue: base.HeroTurnTaker.Hand.Cards.Any((Card c) => IsRelay(c))));
            options.Add(new Function(base.HeroTurnTakerController, "Draw 3 cards", SelectionType.DrawCard, () => base.DrawCards(base.HeroTurnTakerController, 3, optional: true)));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, cardSource: GetCardSource());
            IEnumerator playDrawCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playDrawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playDrawCoroutine);
            }
            yield break;
        }
    }
}
