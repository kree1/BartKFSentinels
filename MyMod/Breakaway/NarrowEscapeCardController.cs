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
    public class NarrowEscapeCardController : CardController
    {
        public NarrowEscapeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has already reduced damage this turn.", () => base.Card.Title + " has not yet reduced damage this turn.");
            SpecialStringMaker.ShowSpecialString(BuildBlockedSpecialString);
            SpecialStringMaker.ShowSpecialString(BuildNotBlockedSpecialString);
            SpecialStringMaker.ShowLowestHP(1, () => 2, new LinqCardCriteria((Card c) => IsHeroCharacterCard(c), "", singular: "hero", plural: "heroes"));
        }

        public List<Card> blockedHeroes = new List<Card>();

        public bool IsBlocked(Card c)
        {
            if (IsHeroCharacterCard(c))
            {
                var blockedEffects = GameController.StatusEffectManager.StatusEffectControllers.Where((StatusEffectController sec) => sec.StatusEffect is OnDealDamageStatusEffect odds && odds.CardSource.Title == this.Card.Title && odds.MethodToExecute == "RedirectDamage");
                List<Card> hasBlockEffect = new List<Card>();
                foreach (StatusEffectController sec in blockedEffects)
                {
                    var odds = sec.StatusEffect as OnDealDamageStatusEffect;
                    Card heroAffected = odds.TargetCriteria.IsSpecificCard;
                    if (!hasBlockEffect.Contains(heroAffected))
                    {
                        hasBlockEffect.Add(heroAffected);
                    }
                }
                return hasBlockEffect.Contains(c);
            }
            else
            {
                return false;
            }
        }

        public string BuildBlockedSpecialString()
        {
            string blockedSpecial = "BLOCKED heroes: ";
            var blockedHeroes = FindCardsWhere(new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && IsBlocked(c)));
            if (blockedHeroes.Any())
            {
                blockedSpecial += blockedHeroes.FirstOrDefault().Title;
                for (int i = 1; i < blockedHeroes.Count(); i++)
                {
                    blockedSpecial += ", " + blockedHeroes.ElementAt(i).Title;
                }
            }
            else
            {
                blockedSpecial += "None";
            }
            return blockedSpecial;
        }

        public string BuildNotBlockedSpecialString()
        {
            string unblockedSpecial = "Non-BLOCKED heroes: ";
            var unblockedHeroes = FindCardsWhere(new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && !IsBlocked(c)));
            if (unblockedHeroes.Any())
            {
                unblockedSpecial += unblockedHeroes.FirstOrDefault().Title;
                for (int i = 1; i < unblockedHeroes.Count(); i++)
                {
                    unblockedSpecial += ", " + unblockedHeroes.ElementAt(i).Title;
                }
            }
            else
            {
                unblockedSpecial += "None";
            }
            return unblockedSpecial;
        }

        protected const string OncePerTurn = "ReduceDamageOncePerTurn";
        private ITrigger ReduceDamageTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce the first damage dealt to this card each turn by 1."
            this.ReduceDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Target == this.Card && dda.Amount > 0, ReduceDamage, TriggerType.ReduceDamage, TriggerTiming.Before);
            // "At the end of the villain turn, each hero except the 2 heroes with the lowest HP become [b]BLOCKED[/b] until the start of the villain turn."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, AssignBlocked, TriggerType.Other);
        }

        private IEnumerator AssignBlocked(PhaseChangeAction pca)
        {
            // "At the end of the villain turn, each hero except the 2 heroes with the lowest HP become [b]BLOCKED[/b] until the start of the villain turn."
            // Find exactly 2 hero character cards with lowest HP
            List<Card> currentLowestHeroes = new List<Card>();
            IEnumerator findLowestCoroutine = GameController.FindTargetsWithLowestHitPoints(1, 2, (Card c) => IsHeroCharacterCard(c), currentLowestHeroes, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(findLowestCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(findLowestCoroutine);
            }
            string lowestNames = "";
            if (currentLowestHeroes.Any())
            {
                lowestNames += currentLowestHeroes.First().Title;
                if (currentLowestHeroes.Count() > 1)
                {
                    lowestNames += ", " + currentLowestHeroes.ElementAt(1).Title;
                }
            }
            Log.Debug("Found 2 heroes with lowest HP: " + lowestNames);

            // All OTHER active hero characters become BLOCKED
            LinqCardCriteria criteria = new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsActive && !currentLowestHeroes.Contains(c));
            List<Card> toBlock = base.GameController.FindCardsWhere(criteria).ToList();

            foreach (Card hero in toBlock)
            {
                //Log.Debug("Creating status effects for " + hero.Title);
                // BLOCKED comprises 2 status effects, both expiring at the start of the villain turn, when the hero leaves play, or when this card leaves play:
                // The hero cannot deal damage to non-Terrain villain targets
                OnDealDamageStatusEffect cantHitNonTerrain = new OnDealDamageStatusEffect(cardWithMethod: this.Card, methodToExecute: nameof(this.PreventDamage), description: hero.Title + " cannot deal damage to non-Terrain villain targets.", triggerTypes: new TriggerType[] { TriggerType.CancelAction }, decisionMaker: base.TurnTaker, cardSource: this.Card, powerNumerals: null);
                //Log.Debug("Initialized cantHitNonTerrain");
                cantHitNonTerrain.UntilStartOfNextTurn(this.TurnTaker);
                cantHitNonTerrain.UntilTargetLeavesPlay(hero);
                cantHitNonTerrain.UntilCardLeavesPlay(this.Card);
                cantHitNonTerrain.SourceCriteria.IsSpecificCard = hero;
                cantHitNonTerrain.TargetCriteria.IsVillain = true;
                cantHitNonTerrain.BeforeOrAfter = BeforeOrAfter.Before;
                IEnumerator addCantHitCoroutine = AddStatusEffect(cantHitNonTerrain);
                //Log.Debug("Built cantHitNonTerrain for " + hero.Title);
                // When the hero would be dealt damage by a villain target, redirect it to a non-BLOCKED hero
                OnDealDamageStatusEffect redirectWhenHit = new OnDealDamageStatusEffect(cardWithMethod: this.Card, methodToExecute: nameof(this.RedirectDamage), description: "When " + hero.Title + " would be dealt damage by a villain target, redirect it to a non-BLOCKED hero.", triggerTypes: new TriggerType[] { TriggerType.RedirectDamage }, decisionMaker: base.TurnTaker, cardSource: this.Card, powerNumerals: null);
                redirectWhenHit.UntilStartOfNextTurn(this.TurnTaker);
                redirectWhenHit.UntilTargetLeavesPlay(hero);
                redirectWhenHit.UntilCardLeavesPlay(this.Card);
                redirectWhenHit.SourceCriteria.IsVillain = true;
                redirectWhenHit.TargetCriteria.IsSpecificCard = hero;
                redirectWhenHit.BeforeOrAfter = BeforeOrAfter.Before;
                IEnumerator addRedirectCoroutine = AddStatusEffect(redirectWhenHit);
                //Log.Debug("Built redirectWhenHit for " + hero.Title);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(addCantHitCoroutine);
                    yield return this.GameController.StartCoroutine(addRedirectCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(addCantHitCoroutine);
                    this.GameController.ExhaustCoroutine(addRedirectCoroutine);
                }
            }

            string escapedNames = "";
            if (currentLowestHeroes.Any())
            {
                escapedNames = currentLowestHeroes.First().Title;
                if (currentLowestHeroes.Count > 1)
                {
                    escapedNames += " and " + currentLowestHeroes.ElementAt(1).Title;
                }
            }
            string blockedAnnouncement = escapedNames + " slip through Narrow Escape!";
            if (blockedHeroes.Any())
            {
                blockedAnnouncement += " All other heroes are BLOCKED!";
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(blockedAnnouncement, Priority.High, cardSource: GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(messageCoroutine);
            }
        }

        private IEnumerator ClearBlocked(PhaseChangeAction pca)
        {
            blockedHeroes = new List<Card>();
            yield break;
        }

        private IEnumerator ReduceDamage(DealDamageAction dda)
        {
            // "Reduce the first damage dealt to this card each turn by 1."
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            IEnumerator reduceCoroutine = base.GameController.ReduceDamage(dda, 1, ReduceDamageTrigger, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(reduceCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(reduceCoroutine);
            }
        }

        public IEnumerator PreventDamage(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // "[b]BLOCKED[/b] heroes can't deal damage to non-Terrain villain targets."
            if (dda != null && dda.Target != null && IsVillainTarget(dda.Target) && !dda.Target.DoKeywordsContain("terrain"))
            {
                IEnumerator preventCoroutine = CancelAction(dda);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(preventCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(preventCoroutine);
                }
            }
        }

        public IEnumerator RedirectDamage(DealDamageAction dda, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            // "Whenever a villain target would deal damage to a [b]BLOCKED[/b] hero, redirect it to a non-[b]BLOCKED[/b] hero."
            IEnumerator redirectCoroutine = base.GameController.SelectTargetAndRedirectDamage(base.DecisionMaker, (Card c) => IsHeroCharacterCard(c) && !IsBlocked(c), dda, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(redirectCoroutine);
            }
        }
    }
}
