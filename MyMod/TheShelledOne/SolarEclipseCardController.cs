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
    public class SolarEclipseCardController : BlaseballWeatherCardController
    {
        public SolarEclipseCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "If there are any Umpires in play, this card is indestructible."
            if (FindCardsWhere(new LinqCardCriteria((Card c) => c.DoKeywordsContain("umpire") && c.IsInPlayAndHasGameText)).Count() > 0)
            {
                return card == base.Card;
            }
            return false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, each Umpire deals the hero target with the highest HP {H + 2} fire damage."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, IncinerateResponse, TriggerType.DealDamage);
        }

        public override IEnumerator Play()
        {
            IEnumerator inheritCoroutine = base.Play();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(inheritCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(inheritCoroutine);
            }
            // "Then, if there are no Umpires in play, put the Umpire with the lowest HP from the villain trash into play."
            if (!base.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("umpire") && c.IsInPlayAndHasGameText).Any())
            {
                IEnumerable<Card> trashUmpires = base.TurnTaker.Trash.Cards.Where((Card c) => c.DoKeywordsContain("umpire"));
                if (trashUmpires.Count() > 0)
                {
                    Card selected = null;
                    foreach(Card c in trashUmpires)
                    {
                        if (selected == null)
                        {
                            selected = c;
                        }
                        else if (c.MaximumHitPoints.Value < selected.MaximumHitPoints.Value)
                        {
                            selected = c;
                        }
                    }
                    IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, selected, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
                else
                {
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("There are no Umpires in the villain trash.", Priority.Medium, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                }
            }
            yield break;
        }

        public IEnumerator IncinerateResponse(GameAction ga)
        {
            // "... each Umpire deals the hero target with the highest HP {H + 2} fire damage."
            IEnumerator damageCoroutine = MultipleDamageSourcesDealDamage(new LinqCardCriteria((Card c) => c.DoKeywordsContain("umpire"), "Umpires", false, false, "Umpire", "Umpires"), TargetType.HighestHP, 1, new LinqCardCriteria((Card c) => c.IsHero), H + 2, DamageType.Fire);
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
