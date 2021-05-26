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
    public class GiantPeanutShellCardController : CardController
    {
        public GiantPeanutShellCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowHeroCharacterCardWithHighestHP().Condition = () => !base.Card.IsInPlayAndHasGameText;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "That hero must skip either their play or power phase each turn."
            AddTrigger((PhaseChangeAction p) => p.ToPhase.IsUsePower && p.ToPhase.TurnTaker == GetCardThisCardIsNextTo().Owner && DidHeroPlayCardDuringPlayPhaseThisTurn(p.ToPhase.TurnTaker), SkipPhaseResponse, TriggerType.SkipPhase, TriggerTiming.After);
            AddTrigger((PhaseChangeAction p) => p.ToPhase.IsPlayCard && p.ToPhase.TurnTaker == GetCardThisCardIsNextTo().Owner && DidHeroUsePowerDuringPowerPhaseThisTurn(p.ToPhase.TurnTaker), SkipPhaseResponse, TriggerType.SkipPhase, TriggerTiming.After);
            // "At the end of that player's turn, if {TheShelledOne} is a target, that hero deals each other hero target 1 toxic damage. Then, if {Feedback} or {Reverb} is in play, you may move this card next to the previous hero in turn order."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == GetCardThisCardIsNextTo().Owner, PlayRedirectMoveResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.MoveCard });
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "When this card enters play, move it next to the hero with the highest HP."
            List<Card> highest = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHero && (overridePlayArea == null || c.IsAtLocationRecursive(overridePlayArea)), highest, null, null, evenIfCannotDealDamage: false, optional: false, null, ignoreBattleZone: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highestHero = highest.FirstOrDefault();
            if (highestHero != null)
            {
                storedResults?.Add(new MoveCardDestination(highestHero.NextToLocation));
            }
        }

        private IEnumerator SkipPhaseResponse(PhaseChangeAction p)
        {
            IEnumerator skipCoroutine = base.GameController.PreventPhaseAction(p.ToPhase, showMessage: false, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(skipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(skipCoroutine);
            }
        }

        private bool DidHeroPlayCardDuringPlayPhaseThisTurn(TurnTaker tt)
        {
            if (tt.IsHero)
            {
                return base.Journal.PlayCardEntriesThisTurn().Any((PlayCardJournalEntry p) => p.CardPlayed.Owner == tt && p.TurnPhase.TurnTaker == tt && p.TurnPhase.IsPlayCard);
            }
            return false;
        }

        private bool DidHeroUsePowerDuringPowerPhaseThisTurn(TurnTaker tt)
        {
            if (tt.IsHero)
            {
                return base.Journal.UsePowerEntriesThisTurn().Any((UsePowerJournalEntry p) => p.PowerUser == tt && p.TurnPhase.TurnTaker == tt && p.TurnPhase.IsUsePower);
            }
            return false;
        }

        public IEnumerator PlayRedirectMoveResponse(GameAction ga)
        {
            // "... if {TheShelledOne} is a target, that hero deals each other hero target 1 toxic damage."
            if (base.CharacterCard.IsTarget && GetCardThisCardIsNextTo().IsTarget)
            {
                IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, GetCardThisCardIsNextTo(), (Card c) => c.IsHero && c.IsTarget && c != GetCardThisCardIsNextTo(), 1, DamageType.Toxic, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            // "Then, if {Feedback} or {Reverb} is in play, you may move this card next to the previous hero in turn order."
            LinqCardCriteria relevantWeather = new LinqCardCriteria((Card c) => (c.Title == "Feedback" || c.Title == "Reverb") && c.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), "relevant Weather Effect");
            if (base.GameController.FindCardsWhere(relevantWeather, visibleToCard: GetCardSource()).Any())
            {
                IEnumerable<HeroTurnTakerController> heroTurnOrder = base.GameController.FindHeroTurnTakerControllers();
                HeroTurnTakerController previous;
                if (GetCardThisCardIsNextTo().Owner.IsHero)
                {
                    previous = heroTurnOrder.ElementAt((heroTurnOrder.IndexOf(base.GameController.FindHeroTurnTakerController(GetCardThisCardIsNextTo().Owner.ToHero())).Value + H - 1) % H);
                }
                else
                {
                    // The Shelled One doesn't summon characters from the box, but the environment might, so if this hero's owner isn't a hero TurnTaker, it must be the environment
                    // That would make "the previous hero in turn order" whoever is right before the environment
                    previous = heroTurnOrder.Last();
                }
                List<YesNoCardDecision> choice = new List<YesNoCardDecision>();
                IEnumerator checkCoroutine = base.GameController.MakeYesNoCardDecision(DecisionMaker, SelectionType.MoveCard, base.Card, storedResults: choice, associatedCards: base.GameController.FindCardsWhere((Card c) => c.IsHeroCharacterCard && c.IsInPlayAndHasGameText && c.Owner == previous.TurnTaker), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(checkCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(checkCoroutine);
                }
                if (DidPlayerAnswerYes(choice))
                {
                    List<Card> characterChoice = new List<Card>();
                    IEnumerator chooseCoroutine = FindCharacterCard(previous.TurnTaker, SelectionType.MoveCardNextToCard, characterChoice, activeOnly: false);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(chooseCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(chooseCoroutine);
                    }
                    Card chosen = characterChoice.FirstOrDefault();
                    if (chosen != null)
                    {
                        IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, chosen.NextToLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(moveCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(moveCoroutine);
                        }
                    }
                }
            }
            yield break;
        }

        public IEnumerator RedirectDamageResponse(DealDamageAction dda)
        {
            // "... redirect damage [on that card] to the hero target with the highest HP."
            List<Card> highest = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHero, highest, gameAction: dda, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highestHero = highest.FirstOrDefault();
            if (highestHero != null)
            {
                IEnumerator redirectCoroutine = base.GameController.RedirectDamage(dda, highestHero, cardSource: GetCardSource());
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
