using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    internal class TheEqualizerCharacterCardController : VillainCharacterCardController
    {
        public TheEqualizerCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            if (IsGameChallenge)
            {
                AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            }
            // Both sides: show list of villain Munitions in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsVillain(c) && GameController.DoesCardContainKeyword(c, MunitionKeyword), "villain Munition"));
            // Front side: show list of [b][i]Marked[/i][/b] hero targets in play
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => IsHeroTarget(c) && ettc.IsMarked(c), "[b][i]Marked[/i][/b]", singular: "hero target", plural: "hero targets")).Condition = () => !Card.IsFlipped;
            // Retreat and Resupply: show list of Munition cards in villain deck
            SpecialStringMaker.ShowListOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword), "Munition")).Condition = () => Card.IsFlipped;
        }

        public const string MunitionKeyword = "munition";
        public const string SalvoName = "salvo";

        public TheEqualizerTurnTakerController ettc => TurnTakerControllerWithoutReplacements as TheEqualizerTurnTakerController;

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "Munitions are indestructible during the villain turn."
            if (GameController.ActiveTurnTaker != null && IsVillain(GameController.ActiveTurnTaker) && GameController.DoesCardContainKeyword(card, MunitionKeyword))
            {
                return true;
            }
            return false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            if (IsGameChallenge)
            {
                // "Munitions are indestructible during the villain turn."
                AddTrigger((PhaseChangeAction pca) => pca.FromPhase != null && pca.FromPhase.TurnTaker != null && pca.ToPhase != null && pca.ToPhase.TurnTaker != null && IsVillain(pca.FromPhase.TurnTaker) && !IsVillain(pca.ToPhase.TurnTaker), (PhaseChangeAction pca) => GameController.DestroyAnyCardsThatShouldBeDestroyed(cardSource: GetCardSource()), TriggerType.DestroyCard, TriggerTiming.After);
            }
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Whenever a Munition leaves play, if there are no villain Munitions in play, flip this card."
                AddSideTrigger(AddTrigger((MoveCardAction mca) => GameController.DoesCardContainKeyword(mca.CardToMove, MunitionKeyword) && mca.Origin.IsInPlay && !mca.Destination.IsInPlay && mca.WasCardMoved && !GameController.FindCardsWhere((Card c) => IsVillain(c) && GameController.DoesCardContainKeyword(c, MunitionKeyword) && c.IsInPlayAndHasGameText).Any(), FlipThisCharacterCardResponse, TriggerType.FlipCard, TriggerTiming.After));
                // "At the start of the villain turn, if there are no [b][i]Marked[/i][/b] hero targets in play, {TheEqualizer} has fulfilled her contract. Game Over."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker && !GameController.FindCardsWhere((Card c) => IsHeroTarget(c) && ettc.IsMarked(c), visibleToCard: GetCardSource()).Any(), LoseTheGameResponse, TriggerType.GameOver));
                // "At the end of the villain turn, activate each [u]salvo[/u] text on each villain Munition in play."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, AllSalvoResponse, TriggerType.DealDamage));
                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // "Whenever {TheEqualizer} is dealt 4 or more damage at once, flip this card."
                    AddSideTrigger(AddTrigger((DealDamageAction dda) => dda.Target == base.Card && dda.DidDealDamage && dda.Amount >= 4, FlipThisCharacterCardResponse, TriggerType.FlipCard, TriggerTiming.After));
                }
            }
            else
            {
                // Back side:
                // "Reduce damage dealt to {TheEqualizer} by 2."
                AddSideTrigger(AddReduceDamageTrigger((Card c) => c == base.Card, 2));
                // "At the start of the villain turn, {TheEqualizer} regains {H} HP."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => GameController.GainHP(base.Card, H, cardSource: GetCardSource()), TriggerType.GainHP));
                // "At the end of the villain turn, reveal cards from the villain deck until a Munition is revealed. Put it into play. Discard the other revealed cards. Then, if there are any villain Munitions in play, flip this card."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, ResupplyResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.DiscardCard, TriggerType.PutIntoPlay, TriggerType.FlipCard }));
            }
            AddDefeatedIfDestroyedTriggers();
            AddDefeatedIfMovedOutOfGameTriggers();
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator flipCoroutine = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(flipCoroutine);
            }
            // Back side, Advanced:
            // "When {TheEqualizer} flips to this side, destroy an environment card. If a card was destroyed this way, {TheEqualizer} deals the {H - 2} hero targets with the highest HP 2 fire damage each."
            if (IsGameAdvanced && Card.IsFlipped)
            {
                List<DestroyCardAction> results = new List<DestroyCardAction>();
                IEnumerator destroyEnvCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, storedResultsAction: results, responsibleCard: Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(destroyEnvCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(destroyEnvCoroutine);
                }
                if (DidDestroyCard(results))
                {
                    IEnumerator fireCoroutine = DealDamageToHighestHP(Card, 1, (Card c) => IsHeroTarget(c), (Card c) => 2, DamageType.Fire, numberOfTargets: () => H - 2);
                    if (base.UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(fireCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(fireCoroutine);
                    }
                }
            }
        }

        public IEnumerator LoseTheGameResponse(PhaseChangeAction pca)
        {
            string defeat = base.Card.Title + " has fulfilled her contract.";
            IEnumerator messageCoroutine = GameController.SendMessageAction(defeat, Priority.Critical, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator endCoroutine = GameController.GameOver(EndingResult.AlternateDefeat, defeat, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(endCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(endCoroutine);
            }
        }

        public int NumberOfMunitionsInPlay()
        {
            return GameController.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && IsVillain(c) && GameController.DoesCardContainKeyword(c, MunitionKeyword)).Count();
        }

        public IEnumerator AllSalvoResponse(PhaseChangeAction pca)
        {
            // "... activate each [u]salvo[/u] text on each villain Munition in play."
            SelectCardsDecision choice = new SelectCardsDecision(GameController, DecisionMaker, (Card c) => c.IsInPlayAndHasGameText && IsVillain(c) && GameController.DoesCardContainKeyword(c, MunitionKeyword), SelectionType.Custom, numberOfCards: null, isOptional: false, eliminateOptions: true, allowAutoDecide: true, dynamicNumberOfCards: () => NumberOfMunitionsInPlay());
            IEnumerator activateCoroutine = GameController.SelectCardsAndDoAction(choice, (SelectCardDecision scd) => ActivateSalvo(scd.SelectedCard), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(activateCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(activateCoroutine);
            }
        }

        public IEnumerator ActivateSalvo(Card munition)
        {
            MunitionCardController mcc = FindCardController(munition) as MunitionCardController;
            if (mcc != null && munition.IsInPlay)
            {
                foreach (ActivatableAbility salvoText in mcc.GetActivatableAbilities(SalvoName))
                {
                    IEnumerator salvoCoroutine = GameController.ActivateAbility(salvoText, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(salvoCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(salvoCoroutine);
                    }
                }
            }
        }

        public IEnumerator ResupplyResponse(PhaseChangeAction pca)
        {
            // "... reveal cards from the villain deck until a Munition is revealed. Put it into play. Discard the other revealed cards."
            IEnumerator revealCoroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(TurnTakerController, TurnTaker.Deck, null, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword), "Munition"), isPutIntoPlay: true, revealUntilNumberOfMatchingCards: 1);
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "Then, if there are any villain Munitions in play, flip this card."
            if (GameController.FindCardsWhere((Card c) => GameController.DoesCardContainKeyword(c, MunitionKeyword) && c.IsInPlayAndHasGameText).Any())
            {
                IEnumerator flipCoroutine = GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Which card's [u]salvo[/u] text do you want to activate?", "deciding which card's [u]salvo[/u] text to activate", "Vote for which card's [u]salvo[/u] text to activate", "which card's [u]salvo[/u] text to activate");
        }
    }
}
