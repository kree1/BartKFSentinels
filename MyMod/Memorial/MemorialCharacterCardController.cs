using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    internal class MemorialCharacterCardController : MemorialUtilityCharacterCardController
    {
        public MemorialCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Front side: who is the hero target with the highest HP?
            SpecialStringMaker.ShowHeroTargetWithHighestHP().Condition = () => !Card.IsFlipped;
            // Front side: what villain Incident(s) are in play?
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.DoKeywordsContain("incident") && c.IsVillain, "villain Incident"), () => true).Condition = () => !Card.IsFlipped;
            // Back side: who are the H - 1 hero targets with the highest HP?
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1, H - 1).Condition = () => Card.IsFlipped;
            // Back side: which targets are Renowned?
            SpecialStringMaker.ShowListOfCards(new LinqCardCriteria((Card c) => IsRenownedTarget(c), "Renowned targets", false, false, "Renowned target", "Renowned targets"), () => true).Condition = () => Card.IsFlipped;
            // Back side: how many times has Memorial dealt 2 or more damage to a Renowned target this turn?
            SpecialStringMaker.ShowSpecialString(() => "Memorial has not dealt 2 or more damage to any Renowned targets this turn.", () => true).Condition = () => Card.IsFlipped && !HasBeenSetToTrueThisTurn(RenownedHit1);
            SpecialStringMaker.ShowSpecialString(() => "Memorial has dealt 2 or more damage to a Renowned target 1 time this turn.", () => true).Condition = () => Card.IsFlipped && HasBeenSetToTrueThisTurn(RenownedHit1) && !HasBeenSetToTrueThisTurn(RenownedHit2);
            SpecialStringMaker.ShowSpecialString(() => "Memorial has dealt 2 or more damage to a Renowned target 2 times this turn.", () => true).Condition = () => Card.IsFlipped && HasBeenSetToTrueThisTurn(RenownedHit2) && !HasBeenSetToTrueThisTurn(RenownedHit3);
            SpecialStringMaker.ShowSpecialString(() => "Memorial has dealt 2 or more damage to a Renowned target 3 times this turn.", () => true).Condition = () => Card.IsFlipped && HasBeenSetToTrueThisTurn(RenownedHit3) && !HasBeenSetToTrueThisTurn(RenownedHit4);
            SpecialStringMaker.ShowSpecialString(() => "Memorial has dealt 2 or more damage to a Renowned target 4 times this turn.", () => true).Condition = () => Card.IsFlipped && HasBeenSetToTrueThisTurn(RenownedHit4) && !HasBeenSetToTrueThisTurn(RenownedHit5);
            SpecialStringMaker.ShowSpecialString(() => "Memorial has dealt 2 or more damage to a Renowned target 5 times this turn.", () => true).Condition = () => Card.IsFlipped && HasBeenSetToTrueThisTurn(RenownedHit5) && !HasBeenSetToTrueThisTurn(RenownedHit6);
            SpecialStringMaker.ShowSpecialString(() => "Memorial has dealt 2 or more damage to a Renowned target 6 or more times this turn.", () => true).Condition = () => Card.IsFlipped && HasBeenSetToTrueThisTurn(RenownedHit6);
        }

        protected const string RenownedHit1 = "FirstRenownedHit";
        protected const string RenownedHit2 = "SecondRenownedHit";
        protected const string RenownedHit3 = "ThirdRenownedHit";
        protected const string RenownedHit4 = "FourthRenownedHit";
        protected const string RenownedHit5 = "FifthRenownedHit";
        protected const string RenownedHit6 = "SixthRenownedHit";

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "When a non-Incident villain card would enter play, instead shuffle it into the villain deck and the non-hero target with the highest HP deals the hero target with the highest HP {H - 1} projectile damage."
                SideTriggers.Add(AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardToPlay.IsVillain && pca.CardToPlay != Card && !pca.CardToPlay.DoKeywordsContain("incident"), ShuffleAndShootResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.DealDamage }, TriggerTiming.Before));
                // "When a villain Incident leaves play, remove that Incident from the game."
                SideTriggers.Add(AddTrigger<MoveCardAction>((MoveCardAction mca) => mca.CardToMove.IsVillain && mca.CardToMove.DoKeywordsContain("incident") && mca.Origin.IsInPlay && !mca.Destination.IsInPlay, RemoveResolvedIncidentResponse, new TriggerType[] { TriggerType.RemoveFromGame }, TriggerTiming.After));
                // "At the start of the villain turn, if there are no villain Incidents in play, flip {Memorial}."
                SideTriggers.Add(AddStartOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, CheckAndFlipResponse, TriggerType.FlipCard));

                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // [none yet]
                }
            }
            else
            {
                // Back side:
                // "When a villain Renown enters play, move it next to a non-Renowned hero character target."
                // [handled in RenownCardController]

                // "If there are none, discard it and play the top card of the villain deck."
                //SideTriggers.Add(AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsVillain && cepa.CardEnteringPlay.DoKeywordsContain(RenownKeyword) && !FindCardsWhere((Card c) => c.IsHeroCharacterCard && c.IsTarget && !IsRenownedTarget(c)).Any(), ExtraRenownResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.DiscardCard, TriggerType.PlayCard }, TriggerTiming.Before));

                // "Whenever a villain target deals 2 or more damage to a Renowned target, activate the first effect in this list that hasn't been activated this turn:"
                // "1) {Memorial} regains {H - 1} HP."
                // "2) One player discards a card."
                // "3) Reduce damage dealt to {Memorial} by 1 until the start of the villain turn."
                // "4) Destroy a non-character hero card."
                // "5) Destroy a hero Ongoing or Equipment card."
                // "6) Destroy the non-character non-villain target with the lowest HP."
                SideTriggers.Add(AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageSource.Card.IsVillainTarget && dda.DidDealDamage && dda.Amount >= 2 && ((!dda.DidDestroyTarget && IsRenownedTarget(dda.Target)) || (dda.DidDestroyTarget && dda.DestroyCardAction != null && NumRenownsAt(GameController.Game.Journal.MostRecentDestroyCardEntry((DestroyCardJournalEntry dcje) => dcje.Card == dda.DestroyCardAction.CardToDestroy.Card).OriginalLocation) > 0)), RenownedHitResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DiscardCard, TriggerType.CreateStatusEffect, TriggerType.DestroyCard }, TriggerTiming.After));

                // "At the end of the villain turn, {Memorial} deals the {H - 1} hero targets with the highest HP 3 projectile damage each."
                SideTriggers.Add(AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, PewPewPewResponse, TriggerType.DealDamage));

                if (base.IsGameAdvanced)
                {
                    // Back side, Advanced:
                    // [none yet]
                }
            }
            AddDefeatedIfDestroyedTriggers();
        }

        private IEnumerator ShuffleAndShootResponse(PlayCardAction pca)
        {
            // "... instead shuffle it into the villain deck..."
            Card entering = pca.CardToPlay;
            pca.AllowPutIntoPlayCancel = true;
            IEnumerator messageCoroutine = GameController.SendMessageAction(Card.Title + " isn't ready to reveal himself yet, so " + entering.Title + " is shuffled back into the villain deck, and someone shoots at the heroes from the shadows...", Priority.High, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator cancelCoroutine = CancelAction(pca, showOutput: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator shuffleCoroutine = GameController.ShuffleCardIntoLocation(DecisionMaker, entering, TurnTaker.Deck, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            // "... and the non-hero target with the highest HP deals the hero target with the highest HP {H - 1} projectile damage."
            List<Card> storedResultsHighest = new List<Card>();
            DealDamageAction shot = new DealDamageAction(GameController, null, null, H - 1, DamageType.Projectile, wasOptional: false);
            IEnumerator findCoroutine = GameController.FindTargetWithHighestHitPoints(1, (Card c) => !c.IsHero, storedResultsHighest, shot, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card shooter = storedResultsHighest.FirstOrDefault();
            if (shooter != null)
            {
                IEnumerator damageCoroutine = DealDamageToHighestHP(shooter, 1, (Card c) => c.IsHero, (Card c) => H - 1, DamageType.Projectile, selectTargetEvenIfCannotDealDamage: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator RemoveResolvedIncidentResponse(MoveCardAction mca)
        {
            // "... remove that Incident from the game."
            IEnumerator moveCoroutine = GameController.MoveCard(TurnTakerController, mca.CardToMove, TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: TurnTaker, actionSource: mca, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield break;
        }

        private IEnumerator CheckAndFlipResponse(PhaseChangeAction pca)
        {
            // "... if there are no villain Incidents in play, flip {Memorial}."
            if (!FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsVillain && c.DoKeywordsContain("incident"))).Any())
            {
                IEnumerator flipCoroutine = GameController.FlipCard(this, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            yield break;
        }

        public override IEnumerator ExtraRenownResponse(Card entering)
        {
            Log.Debug("MemorialCharacterCardController.ExtraRenownResponse(" + entering.Title + ") started");
            if (Card.IsFlipped)
            {
                // "... discard it and play the top card of the villain deck."
                IEnumerator discardCoroutine = GameController.MoveCard(TurnTakerController, entering, TurnTaker.Trash, responsibleTurnTaker: TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                if (base.TurnTaker.Deck.Cards.Any((Card c) => !c.DoKeywordsContain(RenownKeyword)) || base.TurnTaker.Trash.Cards.Any((Card c) => !c.DoKeywordsContain(RenownKeyword)))
                {
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("All hero character targets are already Renowned! Discarding " + entering.Title + " and playing the top card of the villain deck...", Priority.High, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                    IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(null);
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
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction("All hero character targets are already Renowned, and there are no non-Renown cards in the villain deck or trash to play.", Priority.High, GetCardSource());
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
            Log.Debug("MemorialCharacterCardController.ExtraRenownResponse(" + entering.Title + ") finished");
            yield break;
        }

        private IEnumerator RenownedHitResponse(DealDamageAction dda)
        {
            // "... activate the first effect in this list that hasn't been activated this turn:"
            if (!HasBeenSetToTrueThisTurn(RenownedHit1))
            {
                SetCardPropertyToTrueIfRealAction(RenownedHit1);
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " scores his first hit against a Renowned target!", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "1) {Memorial} regains {H - 1} HP."
                IEnumerator healCoroutine = GameController.GainHP(Card, H - 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
            }
            else if (HasBeenSetToTrueThisTurn(RenownedHit1) && !HasBeenSetToTrueThisTurn(RenownedHit2))
            {
                SetCardPropertyToTrueIfRealAction(RenownedHit2);
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " scores his second hit against a Renowned target!", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "2) One player discards a card."
                IEnumerator discardCoroutine = GameController.SelectHeroToDiscardCard(DecisionMaker, false, false, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            else if (HasBeenSetToTrueThisTurn(RenownedHit2) && !HasBeenSetToTrueThisTurn(RenownedHit3))
            {
                SetCardPropertyToTrueIfRealAction(RenownedHit3);
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " scores his third hit against a Renowned target!", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "3) Reduce damage dealt to {Memorial} by 1 until the start of the villain turn."
                ReduceDamageStatusEffect cover = new ReduceDamageStatusEffect(1);
                cover.TargetCriteria.IsSpecificCard = Card;
                cover.UntilStartOfNextTurn(TurnTaker);
                cover.UntilCardLeavesPlay(Card);
                IEnumerator statusCoroutine = AddStatusEffect(cover);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(statusCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(statusCoroutine);
                }
            }
            else if (HasBeenSetToTrueThisTurn(RenownedHit3) && !HasBeenSetToTrueThisTurn(RenownedHit4))
            {
                SetCardPropertyToTrueIfRealAction(RenownedHit4);
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " scores his fourth hit against a Renowned target!", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "4) Destroy a non-character hero card."
                IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && !c.IsCharacter && !AskIfCardIsIndestructible(c), "non-character hero"), false, responsibleCard: Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            else if (HasBeenSetToTrueThisTurn(RenownedHit4) && !HasBeenSetToTrueThisTurn(RenownedHit5))
            {
                SetCardPropertyToTrueIfRealAction(RenownedHit5);
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " scores his fifth hit against a Renowned target!", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "5) Destroy a hero Ongoing or Equipment card."
                IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsHero && (c.IsOngoing || IsEquipment(c)) && !AskIfCardIsIndestructible(c), "hero Ongoing or Equipment"), false, responsibleCard: Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            else if (HasBeenSetToTrueThisTurn(RenownedHit5) && !HasBeenSetToTrueThisTurn(RenownedHit6))
            {
                SetCardPropertyToTrueIfRealAction(RenownedHit6);
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " scores his sixth hit against a Renowned target!", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "6) Destroy the non-character non-villain target with the lowest HP."
                List<Card> storedResultsLowest = new List<Card>();
                IEnumerator findCoroutine = GameController.FindTargetWithLowestHitPoints(1, (Card c) => !c.IsVillain && !c.IsCharacter, storedResultsLowest, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                if (DidFindCard(storedResultsLowest))
                {
                    IEnumerator destroyCoroutine = GameController.DestroyCard(DecisionMaker, storedResultsLowest.First(), overrideOutput: Card.Title + " destroys " + storedResultsLowest.First().Title + "!", responsibleCard: Card, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                }
                else
                {
                    IEnumerator failMessageCoroutine = GameController.SendMessageAction("There are no non-character non-villain targets for " + Card.Title + " to destroy.", Priority.High, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(failMessageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(failMessageCoroutine);
                    }
                }
            }
            else if (HasBeenSetToTrueThisTurn(RenownedHit6))
            {
                IEnumerator messageCoroutine = GameController.SendMessageAction(base.Card.Title + " has already scored six or more hits against Renowned targets this turn.", Priority.High, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator PewPewPewResponse(PhaseChangeAction pca)
        {
            // "... {Memorial} deals the {H - 1} hero targets with the highest HP 3 projectile damage each."
            IEnumerator damageCoroutine = DealDamageToHighestHP(Card, 1, (Card c) => c.IsHero, (Card c) => 3, DamageType.Projectile, numberOfTargets: () => H - 1);
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

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator coroutine = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            // [Memorial becomes a target]
            coroutine = base.GameController.ChangeMaximumHP(base.Card, base.Card.Definition.FlippedHitPoints.Value, alsoSetHP: true, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            // "When {Memorial} flips to this side, search the villain deck for Causality Buffer and put it into play."
            IEnumerator playBufferCoroutine = PlayCardFromLocation(TurnTaker.Deck, "CausalityBuffer", shuffleAfterwardsIfDeck: false);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playBufferCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playBufferCoroutine);
            }
            // "Reveal cards from the villain deck until 3 Renowns are revealed. Put those Renowns into play. Shuffle the other revealed cards back into the villain deck."
            IEnumerator renownCoroutine = RevealCards_MoveMatching_ReturnNonMatchingCards(TurnTakerController, TurnTaker.Deck, false, true, false, new LinqCardCriteria((Card c) => c.DoKeywordsContain(RenownKeyword), "Renown"), 3, showMessage: true, revealedCardDisplay: RevealedCardDisplay.ShowMatchingCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(renownCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(renownCoroutine);
            }
        }
    }
}
