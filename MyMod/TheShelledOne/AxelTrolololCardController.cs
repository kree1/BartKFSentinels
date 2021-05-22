using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class AxelTrolololCardController : CardController
    {
        public AxelTrolololCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, discard the top card of each hero deck in turn order. Put non-One-Shot cards discarded this way under this card. For each One-Shot discarded this way, one hero may use a power."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardKeepOrPowerResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.MoveCard, TriggerType.UsePower });
            // "Whenever there are 3 or more cards under this card, destroy 3 of them and this card deals the hero target with the highest HP 4 projectile damage."
            AddTrigger((MoveCardAction mca) => mca.WasCardMoved && mca.Destination == base.Card.UnderLocation && base.Card.UnderLocation.NumberOfCards >= 3, OutResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.DealDamage }, TriggerTiming.After);
        }

        public IEnumerator DiscardKeepOrPowerResponse(GameAction ga)
        {
            // "... discard the top card of each hero deck in turn order. Put non-One-Shot cards discarded this way under this card."
            int discardedOneShots = 0;
            IEnumerable<HeroTurnTaker> heroDeckOwners = base.GameController.AllHeroes;
            foreach (HeroTurnTaker player in heroDeckOwners)
            {
                List<MoveCardAction> moveResults = new List<MoveCardAction>();
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(player.Deck, moveResults, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                MoveCardAction discard = moveResults.FirstOrDefault();
                if (discard != null && discard.CardToMove != null)
                {
                    Card discarded = discard.CardToMove;
                    if (discarded.DoKeywordsContain("one-shot"))
                    {
                        discardedOneShots += 1;
                    }
                    else
                    {
                        IEnumerator keepCoroutine = base.GameController.MoveCard(base.TurnTakerController, discarded, base.Card.UnderLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, actionSource: discard, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(keepCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(keepCoroutine);
                        }
                    }
                }
            }
            // "For each One-Shot discarded this way, one hero may use a power."
            for (int i = 0; i < discardedOneShots; i++)
            {
                IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(DecisionMaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator OutResponse(GameAction ga)
        {
            // "...  destroy 3 of them..."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.Location == base.Card.UnderLocation, "under " + base.Card.Title, false, true), 3, requiredDecisions: 3, responsibleCard: base.Card, allowAutoDecide: base.Card.UnderLocation.NumberOfCards <= 3, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "... and this card deals the hero target with the highest HP 4 projectile damage."
            IEnumerator damageCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => c.IsHero, (Card c) => 4, DamageType.Projectile);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
