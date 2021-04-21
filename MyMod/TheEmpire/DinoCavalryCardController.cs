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
    public class DinoCavalryCardController : EmpireUtilityCardController
    {
        public DinoCavalryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroWithMostCards(false);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to this card by 1."
            AddReduceDamageTrigger((Card c) => c == base.Card, 1);
            // "At the end of the environment turn, this card deals the hero with the most cards in play {H - 2} melee damage. Destroy an Equipment card belonging to a hero dealt damage this way."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DamageDestroySequence, new TriggerType[] { TriggerType.DealDamage, TriggerType.DestroyCard });
        }

        public IEnumerator DamageDestroySequence(GameAction ga)
        {
            // "... this card deals the hero with the most cards in play {H - 2} melee damage."
            List<DealDamageAction> damage = new List<DealDamageAction>();
            IEnumerator damageCoroutine = DealDamageToMostCardsInPlay(base.Card, 1, new LinqCardCriteria((Card c) => c.IsHeroCharacterCard), H - 2, DamageType.Melee, storedResults: damage, mostFewestSelectionType: SelectionType.MostCardsInPlay);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Destroy an Equipment card belonging to a hero dealt damage this way."
            List<Card> damagedHeroes = (from DealDamageAction dda in damage where dda.Target.IsHeroCharacterCard && dda.DidDealDamage select dda.Target).ToList();
            if (damagedHeroes.Count() > 0)
            {
                LinqCardCriteria equipmentOfDamaged = new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("equipment") && damagedHeroes.Any((Card target) => target.Owner == c.Owner), "Equipment cards owned by heroes who were dealt damage by " + base.Card.Title, false, false, "Equipment card owned by a hero who was dealt damage by " + base.Card.Title, "Equipment cards owned by a hero who was dealt damage by " + base.Card.Title);
                List<TurnTaker> damagedTurnTakers = (from Card target in damagedHeroes select target.Owner).ToList();
                HeroTurnTakerController destroyChooser = DecisionMaker;
                if (damagedTurnTakers.Count() == 1)
                {
                    destroyChooser = FindHeroTurnTakerController(damagedTurnTakers.FirstOrDefault().ToHero());
                }
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(destroyChooser, equipmentOfDamaged, false, responsibleCard: base.Card, cardSource: GetCardSource());
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
