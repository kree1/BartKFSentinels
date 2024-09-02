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
    public class DestructiveChipCardController : ChipCardController
    {
        public DestructiveChipCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        protected DamageType ToDeal {  get; set; }

        protected LinqCardCriteria Relevant {  get; set; }

        protected virtual LinqCardCriteria ToDestroy()
        {
            return Relevant;
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this next to the hero with the most [Relevant] cards in play."
            List<TurnTaker> players = new List<TurnTaker>();
            IEnumerator findCoroutine = FindHeroWithMostCardsInPlay(players, cardCriteria: Relevant, evenIfCannotDealDamage: true);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            TurnTaker selected = players.FirstOrDefault();
            if (selected != null)
            {
                List<Card> heroes = new List<Card>();
                IEnumerator characterCoroutine = FindCharacterCard(selected, SelectionType.MoveCardNextToCard, heroes);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(characterCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(characterCoroutine);
                }
                Card hero = heroes.FirstOrDefault();
                if (hero != null)
                {
                    storedResults?.Add(new MoveCardDestination(hero.NextToLocation));
                }
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of that hero's turn, this card deals itself or that hero 2 [ToDeal] damage."
            AddStartOfTurnTrigger((TurnTaker tt) => GetCardThisCardIsNextTo() != null && tt == GetCardThisCardIsNextTo().Owner, (PhaseChangeAction pca) => GameController.SelectTargetsAndDealDamage(GameController.FindHeroTurnTakerController(GetCardThisCardIsNextTo().Owner.ToHero()), new DamageSource(GameController, Card), 2, ToDeal, 1, false, 1, additionalCriteria: (Card c) => c == Card || c == GetCardThisCardIsNextTo(), cardSource: GetCardSource()), TriggerType.DealDamage);
            // "When this card is destroyed, destroy 2 [ToDestroy] cards."
            AddWhenDestroyedTrigger((DestroyCardAction dca) => GameController.SelectAndDestroyCards(DecisionMaker, ToDestroy(), 2, requiredDecisions: 2, responsibleCard: Card, cardSource: GetCardSource()), new TriggerType[] { TriggerType.DestroyCard });
        }
    }
}
