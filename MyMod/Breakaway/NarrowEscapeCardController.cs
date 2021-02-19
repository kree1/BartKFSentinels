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
        public List<Card> blockedHeroes = new List<Card>();

        public bool IsBlocked(Card c)
        {
            if (c.IsHero && c.IsCharacter)
            {
                return blockedHeroes.Contains(c);
            }
            else
            {
                return false;
            }
        }

        public string BuildBlockedSpecialString()
        {
            string blockedSpecial = "BLOCKED heroes: ";
            var blockedHeroes = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && IsBlocked(c)));
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
            var unblockedHeroes = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && !IsBlocked(c)));
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

        public NarrowEscapeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(BuildBlockedSpecialString);
            SpecialStringMaker.ShowSpecialString(BuildNotBlockedSpecialString);
            SpecialStringMaker.ShowLowestHP(1, () => 2, new LinqCardCriteria((Card c) => c.IsHeroCharacterCard));
        }

        protected const string OncePerTurn = "ReduceDamageOncePerTurn";
        private ITrigger ReduceDamageTrigger;

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce the first damage dealt to this card each turn by 1."
            this.ReduceDamageTrigger = base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && dda.Target == this.Card && dda.Amount > 0, ReduceDamage, TriggerType.ReduceDamage, TriggerTiming.After);
            // "At the end of the villain turn, each hero except the 2 heroes with the lowest HP become [b]BLOCKED[/b] until the start of the villain turn."
            base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, AssignBlocked, TriggerType.Other);
            // "[b]BLOCKED[/b] heroes can't deal damage to villain cards other than this card."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => IsBlocked(dda.DamageSource.Card) && dda.Target.IsVillainTarget && dda.Target != base.Card, PreventDamage, TriggerType.CancelAction, TriggerTiming.Before, requireActionSuccess: false);
            // "Whenever a villain target would deal damage to a [b]BLOCKED[/b] hero, redirect it to a non-[b]BLOCKED[/b] hero."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => IsBlocked(dda.Target) && dda.DamageSource.Card.IsVillainTarget, RedirectDamage, TriggerType.RedirectDamage, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        private IEnumerator AssignBlocked(PhaseChangeAction pca)
        {
            // "At the end of the villain turn, each hero except the 2 heroes with the lowest HP become [b]BLOCKED[/b] until the start of the villain turn."
            List<Card> currentLowestHeroes = GameController.FindAllTargetsWithLowestHitPoints(1, (Card c) => c.IsHeroCharacterCard, cardSource: GetCardSource(), numberOfTargets: 2).ToList();
            blockedHeroes = currentLowestHeroes;
            string blockedAnnouncement = BuildNotBlockedSpecialString() + " slip through Narrow Escape!";
            if (blockedHeroes.Any())
            {
                blockedAnnouncement += "All other heroes are BLOCKED!";
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
            yield break;
        }

        private IEnumerator PreventDamage(DealDamageAction dda)
        {
            // "[b]BLOCKED[/b] heroes can't deal damage to villain cards other than this card."
            IEnumerator preventCoroutine = CancelAction(dda);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(preventCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(preventCoroutine);
            }
            yield break;
        }

        private IEnumerator RedirectDamage(DealDamageAction dda)
        {
            // "Whenever a villain target would deal damage to a [b]BLOCKED[/b] hero, redirect it to a non-[b]BLOCKED[/b] hero."
            IEnumerator redirectCoroutine = base.GameController.SelectTargetAndRedirectDamage(base.DecisionMaker, (Card c) => c.IsHeroCharacterCard && !IsBlocked(c), dda, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(redirectCoroutine);
            }
            yield break;
        }
    }
}
