using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace BartKFSentinels.Breakaway
{
    public class LightenTheLoadCardController : CardController
    {
        public LightenTheLoadCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(this.TurnTaker.PlayArea, cardCriteria: new LinqCardCriteria((Card c) => c.IsHero, "hero"));
            // TODO: SpecialStringMaker for H-2 heroes with most cards in play?
        }

        public override IEnumerator Play()
        {
            // "The {H - 2} players with the most cards in play each destroy 1 of their cards."
            // Find those players, store them as loadedHeroes...
            List<TurnTaker> loadedHeroes = new List<TurnTaker>();
            IEnumerator findPlayersCoroutine = base.FindHeroWithMostCardsInPlay(loadedHeroes, numberOfHeroes: base.H - 2);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(findPlayersCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(findPlayersCoroutine);
            }
            LinqTurnTakerCriteria isListedHero = new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && loadedHeroes.Contains(tt));

            // Ask each of them to destroy a card if they have at least 1 in play
            List<DestroyCardAction> destroyAttempts = new List<DestroyCardAction>();
            List<DestroyCardAction> playersDestroyed = new List<DestroyCardAction>();
            if (GameController.FindCardsWhere(new LinqCardCriteria((Card c) => !c.IsCharacter && loadedHeroes.Contains(c.Owner))).Any())
            {
                SelectTurnTakersDecision destroyOrder = new SelectTurnTakersDecision(base.GameController, this.DecisionMaker, isListedHero, SelectionType.DestroyCard, Game.H - 2, cardSource: GetCardSource());
                IEnumerator playersDestroyCoroutine = base.GameController.SelectTurnTakersAndDoAction(destroyOrder, (TurnTaker tt) => base.GameController.SelectAndDestroyCard(base.FindHeroTurnTakerController(tt.ToHero()), cardCriteria: new LinqCardCriteria((Card c) => !c.IsCharacter && c.Owner == tt, "non-character"), false, storedResultsAction: playersDestroyed, cardSource: GetCardSource()));
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(playersDestroyCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(playersDestroyCoroutine);
                }
            }

            // "Destroy a hero card in the villain play area."
            List<DestroyCardAction> villainDestroyed = new List<DestroyCardAction>();
            IEnumerator villainDestroyCoroutine = base.GameController.SelectAndDestroyCards(this.DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && c.Location.HighestRecursiveLocation == base.TurnTaker.FindCard("BreakawayCharacter").Location.HighestRecursiveLocation), new int?(1), storedResultsAction: villainDestroyed, responsibleCard: this.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(villainDestroyCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(villainDestroyCoroutine);
            }

            // "If a card was destroyed this way, {Breakaway} regains 2 HP."
            DestroyCardAction villainAttempt = villainDestroyed.FirstOrDefault();
            if (villainAttempt != null && villainAttempt.WasCardDestroyed)
            {
                IEnumerator hpGainCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("BreakawayCharacter"), 2, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(hpGainCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(hpGainCoroutine);
                }
            }

            // "Reduce the next damage dealt by each non-villain target by X, where X = 2 plus the number of cards destroyed by this card."
            destroyAttempts = destroyAttempts.Concat(playersDestroyed).ToList();
            destroyAttempts = destroyAttempts.Concat(villainDestroyed).ToList();
            List<DestroyCardAction> destroySuccesses = destroyAttempts.FindAll((DestroyCardAction da) => da.WasCardDestroyed);
            int reduction = 2 + destroySuccesses.Count();

            // Do ReduceNextDamage for EACH non-villain target in play
            foreach(Card nvt in base.GameController.FindTargetsInPlay((Card c) => c.IsNonVillainTarget))
            {
                IEnumerator reduceCoroutine = this.ReduceNextDamage(nvt, reduction);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(reduceCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(reduceCoroutine);
                }

            }

            yield break;
        }

        private IEnumerator ReduceNextDamage(Card target, int reduction)
        {
            // Reduce the next damage dealt by [target] by [amount]
            ReduceDamageStatusEffect lightenStatusEffect = new ReduceDamageStatusEffect(reduction);
            lightenStatusEffect.SourceCriteria.IsSpecificCard = target;
            lightenStatusEffect.NumberOfUses = 1;
            lightenStatusEffect.UntilTargetLeavesPlay(target);

            IEnumerator statusCoroutine = base.AddStatusEffect(lightenStatusEffect);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(statusCoroutine);
            }

            yield break;
        }
    }
}
