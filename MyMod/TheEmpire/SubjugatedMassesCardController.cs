using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class SubjugatedMassesCardController : EmpireUtilityCardController
    {
        public SubjugatedMassesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(ImperialTargetInPlay());
        }

        public LinqCardCriteria ImperialTargetInPlay()
        {
            return new LinqCardCriteria((Card c) => c.IsTarget && c.IsInPlayAndHasGameText && c.DoKeywordsContain(AuthorityKeyword), singular: "Imperial target", plural: "Imperial targets");
        }

        public int NumberOfImperialTargetsInPlay()
        {
            // "X on this card = the number of Imperial targets in play."
            return base.GameController.FindCardsWhere(ImperialTargetInPlay(), visibleToCard: GetCardSource()).Count();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals itself X + 2 psychic damage unless one player discards a card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageOrDiscardResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DealDamage });
            // "At the start of the environment turn, if X is less than this card's HP, one player may play an Ongoing or Equipment card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayOngEqpResponse, TriggerType.PlayCard, additionalCriteria: (PhaseChangeAction pca) => NumberOfImperialTargetsInPlay() < base.Card.HitPoints);
        }

        public IEnumerator DamageOrDiscardResponse(GameAction ga)
        {
            // "... this card deals itself X + 2 psychic damage unless one player discards a card."
            DealDamageAction preview = new DealDamageAction(GetCardSource(), new DamageSource(base.GameController, base.Card), base.Card, NumberOfImperialTargetsInPlay() + 2, DamageType.Psychic);
            List<DiscardCardAction> discard = new List<DiscardCardAction>();
            IEnumerator selectCoroutine = base.GameController.SelectHeroToDiscardCard(DecisionMaker, optionalSelectHero: true, optionalDiscardCard: true, additionalHeroCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand), storedResultsDiscard: discard, gameAction: preview, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }

            if (!DidDiscardCards(discard, 1, orMore: true))
            {
                IEnumerator damageCoroutine = DealDamage(base.Card, base.Card, NumberOfImperialTargetsInPlay() + 2, DamageType.Psychic, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
        }

        public IEnumerator PlayOngEqpResponse(GameAction ga)
        {
            // "... one player may play an Ongoing or Equipment card."
            IEnumerable<TurnTaker> options = base.GameController.FindTurnTakersWhere((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().Hand.Cards.Any((Card c) => IsOngoing(c) || IsEquipment(c)));
            SelectTurnTakerDecision playerChoice = new SelectTurnTakerDecision(base.GameController, DecisionMaker, options, SelectionType.PlayCard, cardSource: GetCardSource());
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakerAndDoAction(playerChoice, PlayOngEqp);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }

        public IEnumerator PlayOngEqp(TurnTaker tt)
        {
            if (IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame)
            {
                HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
                IEnumerator playCoroutine = SelectAndPlayCardFromHand(httc, cardCriteria: new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "Ongoing or Equipment"), associateCardSource: true);
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
    }
}
