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
    public class SurpriseRaidCardController : EmpireUtilityCardController
    {
        public SurpriseRaidCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, this card deals each non-Imperial target 2 melee damage. Then, destroy this card."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDestroySequence, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        public IEnumerator DamageDestroySequence(GameAction ga)
        {
            // "... this card deals each non-Imperial target 2 melee damage."
            IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => !c.DoKeywordsContain(AuthorityKeyword), 2, DamageType.Melee, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // " Then, destroy this card."
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, each player may discard a card or destroy one of their cards in play. If they do, reduce damage dealt to their targets by this card by 2."
            List<SelectFunctionDecision> chosen = new List<SelectFunctionDecision>();
            Func<HeroTurnTakerController, IEnumerable<Function>> options = (HeroTurnTakerController httc) => new Function[]
            {
                new Function(httc, "Discard a card", SelectionType.DiscardCard, () => SelectAndDiscardCards(httc, 1, optional: false, responsibleTurnTaker: base.TurnTaker), onlyDisplayIfTrue: httc.HasCardsInHand),
                new Function(httc, "Destroy one of your cards", SelectionType.DestroyCard, () => DestroyOneOfYourCards(httc), onlyDisplayIfTrue: httc.HasCardsWhere((Card c) => c.IsInPlay && !c.IsCharacter && !base.GameController.IsCardIndestructible(c)))
            };
            IEnumerator selectCoroutine = EachPlayerSelectsFunction((HeroTurnTakerController httc) => httc.IsHero && !httc.IsIncapacitatedOrOutOfGame, options, requiredNumberOfHeroes: 0, storedResults: chosen, outputIfCannotChooseFunction: (HeroTurnTakerController httc) => httc.Name + " cannot discard or destroy a card for " + base.Card.Title + ".");
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            IEnumerable<HeroTurnTakerController> complied = (from d in chosen where d.SelectedFunction != null select d.HeroTurnTakerController);
            foreach(HeroTurnTakerController httc in complied)
            {
                ReduceDamageStatusEffect protection = new ReduceDamageStatusEffect(2);
                protection.TargetCriteria.OwnedBy = httc.TurnTaker;
                protection.SourceCriteria.IsSpecificCard = base.Card;
                protection.CardDestroyedExpiryCriteria.Card = base.Card;

                IEnumerator statusCoroutine = base.GameController.AddStatusEffect(protection, true, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DestroyOneOfYourCards(HeroTurnTakerController httc)
        {
            // "... destroy one of their cards in play."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(httc, new LinqCardCriteria((Card c) => c.Owner == httc.HeroTurnTaker && !c.IsCharacter, "owned by " + httc.Name, false, true), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
