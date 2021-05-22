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
    public class TheGrandUnslamCardController : CardController
    {
        public TheGrandUnslamCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.IncreasePhaseActionCount);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
			// "Players may play an additional card, use an additional power, and draw an additional card during their turns."
			foreach (HeroTurnTaker hero in base.GameController.AllHeroes)
			{
				AddAdditionalPhaseActionTrigger((TurnTaker tt) => tt == hero, Phase.PlayCard, 1);
				AddAdditionalPhaseActionTrigger((TurnTaker tt) => tt == hero, Phase.UsePower, 1);
				AddAdditionalPhaseActionTrigger((TurnTaker tt) => tt == hero, Phase.DrawCard, 1);
			}
			// "At the start of the villain turn, each non-hero target regains {H} HP and the environment deals each hero target 4 infernal damage. Then, play the top card of the villain deck and remove this card from the game."
			AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealDamagePlayRemoveResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DealDamage, TriggerType.PlayCard, TriggerType.RemoveFromGame });
		}
		private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
		{
			if (tt.IsHero)
			{
				return tt.BattleZone == base.BattleZone;
			}
			return false;
		}

		public override bool AskIfIncreasingCurrentPhaseActionCount()
		{
			if (base.GameController.ActiveTurnPhase.IsPlayCard || base.GameController.ActiveTurnPhase.IsUsePower || base.GameController.ActiveTurnPhase.IsDrawCard)
			{
				return ShouldIncreasePhaseActionCount(base.GameController.ActiveTurnTaker);
			}
			return false;
		}

		public IEnumerator HealDamagePlayRemoveResponse(GameAction ga)
        {
			// "... each non-hero target regains {H} HP..."
			IEnumerator healCoroutine = base.GameController.GainHP(DecisionMaker, (Card c) => !c.IsHero, H, cardSource: GetCardSource());
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(healCoroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(healCoroutine);
			}
			// "... and the non-hero target with the highest HP deals each hero target 4 infernal damage."
			List<Card> highest = new List<Card>();
			IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => !c.IsHero, highest, evenIfCannotDealDamage: true, cardSource: GetCardSource());
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(findCoroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(findCoroutine);
			}
			Card highestNonHero = highest.FirstOrDefault();
			if (highestNonHero != null)
			{
				IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, highestNonHero, (Card c) => c.IsHero, 4, DamageType.Infernal, cardSource: GetCardSource());
				if (base.UseUnityCoroutines)
				{
					yield return base.GameController.StartCoroutine(damageCoroutine);
				}
				else
				{
					base.GameController.ExhaustCoroutine(damageCoroutine);
				}
			}
			// "Then, play the top card of the villain deck..."
			IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, base.TurnTakerController, responsibleTurnTaker: base.TurnTaker, showMessage: true, cardSource: GetCardSource());
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(playCoroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(playCoroutine);
			}
			// "... and remove this card from the game."
			IEnumerator removeCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
			if (base.UseUnityCoroutines)
			{
				yield return base.GameController.StartCoroutine(removeCoroutine);
			}
			else
			{
				base.GameController.ExhaustCoroutine(removeCoroutine);
			}
			yield break;
        }
	}
}
