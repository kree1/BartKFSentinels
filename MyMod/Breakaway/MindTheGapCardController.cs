using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace BartKFSentinels.Breakaway
{
    public class MindTheGapCardController : CardController
    {
        public MindTheGapCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsHero && !c.IsCharacter, "hero non-character"));
        }

        public override IEnumerator Play()
        {
            if (GameController.FindCardsWhere((Card c) => c.IsInPlay && c.IsHero && !c.IsCharacter).Any())
            {
                // "Each player may destroy any number of their non-character cards."
                List<DestroyCardAction> destroyAttempts = new List<DestroyCardAction>();
                //SelectTurnTakersDecision destroyOrder = new SelectTurnTakersDecision(base.GameController, this.DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero), SelectionType.DestroyCard, isOptional: true, allowAutoDecide: true, cardSource: GetCardSource());
                //IEnumerator playersDestroyCoroutine = base.GameController.SelectTurnTakersAndDoAction(destroyOrder, (TurnTaker tt) => base.GameController.SelectAndDestroyCard(base.FindHeroTurnTakerController(tt.ToHero()), new LinqCardCriteria((Card c) => !c.IsCharacter && c.Owner == tt, "non-character"), true, storedResultsAction: destroyAttempts, cardSource: GetCardSource()));
                IEnumerator playersDestroyCoroutine = EachPlayerDestroysTheirCards(new LinqTurnTakerCriteria((TurnTaker tt) => true, "players with non-character cards in play"), new LinqCardCriteria((Card c) => !c.IsCharacter, "non-character"), null, requiredNumberOfCards: 0, requiredNumberOfHeroes: 0, storedResults: destroyAttempts);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(playersDestroyCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(playersDestroyCoroutine);
                }

                // "For each player who destroyed at least 1 card this way,..."
                List<TurnTaker> heroesWithDestroyed = new List<TurnTaker>();
                if (destroyAttempts.Count() > 0)
                {
                    using (List<DestroyCardAction>.Enumerator enumerator = destroyAttempts.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            DestroyCardAction destroyed = enumerator.Current;
                            if (destroyed.WasCardDestroyed && !heroesWithDestroyed.Contains(destroyed.CardToDestroy.TurnTaker))
                            {
                                Log.Debug(destroyed.CardToDestroy.TurnTaker.Identifier + " has destroyed a card");
                                heroesWithDestroyed.Add(destroyed.CardToDestroy.TurnTaker);
                            }
                        }
                    }
                }
                int numHeroesDestroyed = heroesWithDestroyed.Count();

                for (int i = 0; i < numHeroesDestroyed; i++)
                {
                    // "... increase the next damage dealt to {Momentum} by 1..."
                    IncreaseDamageStatusEffect plusX = new IncreaseDamageStatusEffect(1);
                    plusX.TargetCriteria.IsSpecificCard = base.TurnTaker.FindCard("MomentumCharacter");
                    plusX.NumberOfUses = 1;
                    plusX.UntilCardLeavesPlay(base.TurnTaker.FindCard("MomentumCharacter"));
                    IEnumerator increaseCoroutine = base.AddStatusEffect(plusX);
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(increaseCoroutine);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(increaseCoroutine);
                    }

                    // "... and 1 hero target regains 1 HP."
                    IEnumerator heroHealCoroutine = base.GameController.SelectAndGainHP(this.DecisionMaker, 1, additionalCriteria: (Card c) => c.IsHero && c.IsTarget && c.IsInPlayAndNotUnderCard, numberOfTargets: 1, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(heroHealCoroutine);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(heroHealCoroutine);
                    }
                }
            }

            // "{Breakaway} regains X HP, where X = 3 plus the number of non-character hero cards in play."
            int hpAmount = FindCardsWhere((Card c) => c.IsInPlay && c.IsHero && !c.IsCharacter).Count() + 3;
            IEnumerator hpGainCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("BreakawayCharacter"), hpAmount, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(hpGainCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(hpGainCoroutine);
            }

            yield break;
        }
    }
}
