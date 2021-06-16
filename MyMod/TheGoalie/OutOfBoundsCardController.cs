using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class OutOfBoundsCardController : TheGoalieUtilityCardController
    {
        public OutOfBoundsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.PlayArea, new LinqCardCriteria((Card c) => IsGoalposts(c), "goalposts")).Condition = () => NumGoalpostsAt(base.TurnTaker.PlayArea) > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Goalposts cards in " + base.TurnTaker.NameRespectingVariant + "'s play area.").Condition = () => NumGoalpostsAt(base.TurnTaker.PlayArea) <= 0;
        }

        public override IEnumerator Play()
        {
            // "Return all Goalposts cards in your play area to your hand."
            IEnumerator returnCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsGoalposts(c) && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation.OwnerTurnTaker == base.TurnTaker, "Goalposts cards in " + base.TurnTaker.Name + "'s play area", false, false, "Goalposts card in " + base.TurnTaker.Name + "'s play area", "Goalposts cards in " + base.TurnTaker.Name + "'s play area"), visibleToCard: GetCardSource()), (Card c) => new MoveCardDestination(base.HeroTurnTaker.Hand), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            // "{TheGoalieCharacter} deals 1 target 3 melee damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 3, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }

            // "You may play a Goalposts card or draw 3 cards."
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, "Play a Goalposts card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.HeroTurnTakerController, cardCriteria: new LinqCardCriteria((Card c) => IsGoalposts(c), "Goalposts")), onlyDisplayIfTrue: base.HeroTurnTaker.Hand.Cards.Any((Card c) => IsGoalposts(c))));
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
