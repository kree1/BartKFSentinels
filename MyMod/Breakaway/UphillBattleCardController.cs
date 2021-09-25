using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    public class UphillBattleCardController : CardController
    {
        public UphillBattleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroCharacterCardWithLowestHP(1, 1);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a hero uses a power, their player discards a card."
            AddTrigger((UsePowerAction upa) => upa.HeroUsingPower != null, DiscardResponse, TriggerType.DiscardCard, TriggerTiming.After);
            // "At the end of the villain turn, {Momentum} deals the hero character with the lowest HP and itself {H - 1} energy damage each."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DealDamageResponse, TriggerType.DealDamage);
            // "When {Momentum} flips to its "Under Pressure" side, destroy this card and play the top card of the villain deck."
            AddTrigger((FlipCardAction fca) => fca.CardToFlip.Card == base.TurnTaker.FindCard("MomentumCharacter") && !fca.ToFaceDown, SelfDestructResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard }, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        private IEnumerator DiscardResponse(UsePowerAction upa)
        {
            // "Whenever a hero uses a power, their player discards a card."
            HeroTurnTakerController discarding = upa.HeroUsingPower;
            IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(discarding, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(discardCoroutine);
            }
            yield break;
        }

        private IEnumerator DealDamageResponse(PhaseChangeAction pca)
        {
            // "At the end of the villain turn, {Momentum} deals the hero character with the lowest HP and itself {H - 1} energy damage each."
            List<SelectCardDecision> storedResultsHero = new List<SelectCardDecision>();

            // Find the hero character card with the lowest HP; save it to tiredHero...
            LinqCardCriteria criteria = new LinqCardCriteria((Card card) => base.CanCardBeConsideredLowestHitPoints(card, (Card c) => c.IsHeroCharacterCard && c.IsInPlayAndHasGameText && !c.IsFlipped));
            IEnumerator findCoroutine = base.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.LowestHP, criteria, storedResultsHero, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card tiredHero = storedResultsHero.FirstOrDefault().SelectedCard;

            // Momentum deals that hero and itself H-1 energy damage each
            Card momentumCard = base.TurnTaker.FindCard("MomentumCharacter");
            List<Card> targets = new List<Card>() { tiredHero, momentumCard };
            IEnumerator damageCoroutine = base.DealDamage(momentumCard, (Card c) => targets.Contains(c), base.H - 1, DamageType.Energy);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }

        private IEnumerator SelfDestructResponse(FlipCardAction fca)
        {
            // "When {Momentum} flips to its "Under Pressure" side, destroy this card and play the top card of the villain deck."
            Log.Debug(base.TurnTaker.FindCard("MomentumCharacter").Title + " flipped to Under Pressure! " + base.Card.Title + " will play the top card of the villain deck and then destroy itself.");
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(this.DecisionMaker, this.Card, optional: false, postDestroyAction: () => base.PlayTheTopCardOfTheVillainDeckResponse(fca), actionSource: fca, responsibleCard: this.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(destroyCoroutine);
            }

            yield break;
        }
    }
}
