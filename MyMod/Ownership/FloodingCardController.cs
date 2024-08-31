using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class FloodingCardController : ExpansionWeatherCardController
    {
        public FloodingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, either destroy a hero Ongoing or Equipment card in the current turn's play area or {OwnershipCharacter} deals a hero character in that play area 2 cold damage."
            List<Function> options = new List<Function>();
            TurnTaker current = GameController.ActiveTurnTaker;
            string playAreaName = current.Name + "'s play area";
            if (current.DeckDefinition.IsPlural)
                playAreaName = current.Name + "' play area";
            LinqCardCriteria destroyCriteria = new LinqCardCriteria((Card c) => ((IsOngoing(c) && IsHero(c)) || IsEquipment(c)) && c.IsInPlayAndHasGameText && c.Location.IsPlayAreaOf(current), "hero Ongoing or Equipment", singular: "card in " + playAreaName, plural: "cards in " + playAreaName);
            LinqCardCriteria damageCriteria = new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsTarget && c.Location.IsPlayAreaOf(current), "active hero character", singular: "card in " + playAreaName, plural: "cards in " + playAreaName);

            // Decide whether to give control to the group, or just one player who controls all the relevant cards
            HeroTurnTakerController driving = DecisionMaker;
            List<Card> allOptions = GameController.FindCardsWhere((Card c) => destroyCriteria.Criteria(c) || damageCriteria.Criteria(c), visibleToCard: GetCardSource()).ToList();
            TurnTaker firstOwner = null;
            if (allOptions.Count > 0)
            {
                firstOwner = allOptions.First().Owner;
                List<Card> ownedOptions = allOptions.Where((Card c) => c.Owner == firstOwner).ToList();
                if (ownedOptions.Count == allOptions.Count)
                {
                    driving = GameController.FindHeroTurnTakerController(firstOwner.ToHero());
                }
            }

            Card ownCharacter = FindCard(OwnershipIdentifier);
            options.Add(new Function(DecisionMaker, "Destroy a hero Ongoing or Equipment card in " + playAreaName, SelectionType.DestroyCard, () => GameController.SelectAndDestroyCard(DecisionMaker, destroyCriteria, false, responsibleCard: base.Card, cardSource: GetCardSource()), GameController.FindCardsWhere(destroyCriteria, visibleToCard: GetCardSource()).Any(), ownCharacter.Title + " cannot deal damage to any hero characters in " + playAreaName + ", so " + base.Card.Title + " must destroy a hero Ongoing or Equipment card."));
            options.Add(new Function(DecisionMaker, "{Ownership} deals a hero character in " + playAreaName + " 2 cold damage", SelectionType.DealDamage, () => GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, ownCharacter), 2, DamageType.Cold, 1, false, 1, additionalCriteria: (Card c) => damageCriteria.Criteria(c), cardSource: GetCardSource()), GameController.FindCardsWhere(damageCriteria, visibleToCard: GetCardSource()).Any() && GameController.CanDealDamage(ownCharacter, cardSource: GetCardSource()) == null, "There are no hero Ongoing or Equipment cards in " + playAreaName + " that can be destroyed, so " + ownCharacter.Title + " must deal damage."));
            SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, false, noSelectableFunctionMessage: ownCharacter.Title + " cannot deal damage to any hero characters in " + playAreaName + ", nor can any hero Ongoing or Equipment cards there be destroyed.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
        }
    }
}
