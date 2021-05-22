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
    public class TheNecromancyCardController : CardController
    {
        public TheNecromancyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsAtLocations(() => from httc in base.GameController.FindHeroTurnTakerControllers()
                                                                       where !httc.IsIncapacitatedOrOutOfGame
                                                                       select httc.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain("one-shot"), "one-shot"));
        }

        public override IEnumerator Play()
        {
            // "Villain cards are indestructible this turn."
            MakeIndestructibleStatusEffect untouchable = new MakeIndestructibleStatusEffect();
            untouchable.CardsToMakeIndestructible.IsVillain = true;
            untouchable.UntilThisTurnIsOver(base.Game);
            IEnumerator statusCoroutine = base.GameController.AddStatusEffect(untouchable, true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            // "Play the topmost One-Shot card from each hero trash. Redirect damage on cards played this way to the hero target with the second lowest HP."
            IEnumerator playAllCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame), SelectionType.PlayCard, BringOutYourDeadResponse, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playAllCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playAllCoroutine);
            }
            yield break;
        }

        public IEnumerator BringOutYourDeadResponse(TurnTaker tt)
        {
            // "Play the topmost One-Shot card from [this hero's] trash. Redirect damage on cards played this way to the hero target with the second lowest HP."
            IEnumerable<Card> trashOneShots = tt.Trash.Cards.Where((Card c) => c.DoKeywordsContain("one-shot"));
            Card toPlay = trashOneShots.LastOrDefault();
            if (toPlay != null)
            {
                AddToTemporaryTriggerList(AddTrigger((DealDamageAction dda) => dda.CardSource != null && dda.CardSource.Card == toPlay, RedirectDamageResponse, TriggerType.RedirectDamage, TriggerTiming.Before));
                IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, toPlay, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                RemoveTemporaryTriggers();
            }
            yield break;
        }

        public IEnumerator RedirectDamageResponse(DealDamageAction dda)
        {
            // "Redirect damage on cards played this way to the hero target with the second lowest HP."
            List<Card> lowest = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(2, (Card c) => c.IsHero && c.IsTarget, lowest, dda, dda.ToEnumerable(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card secondLowest = lowest.FirstOrDefault();
            if (secondLowest != null)
            {
                IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, secondLowest, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(redirectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(redirectCoroutine);
                }
            }
            yield break;
        }
    }
}
