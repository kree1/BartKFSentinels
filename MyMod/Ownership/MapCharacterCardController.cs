using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class MapCharacterCardController : OwnershipBaseCharacterCardController
    {
        public MapCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Front side: show total damage dealt to non-hero targets by hero targets this turn
            SpecialStringMaker.ShowSpecialString(() => DamageDealtToNonHeroByHeroThisTurn() + " damage has been dealt to non-hero targets by hero targets this turn.", showInEffectsList: () => true).Condition = () => !base.Card.IsFlipped;
            // Both sides: Show location of each hero marker
            SpecialStringMaker.ShowSpecialString(() => DisplayMarkerLocation(1), () => true).Condition = () => 1 <= H;
            SpecialStringMaker.ShowSpecialString(() => DisplayMarkerLocation(2), () => true).Condition = () => 2 <= H;
            SpecialStringMaker.ShowSpecialString(() => DisplayMarkerLocation(3), () => true).Condition = () => 3 <= H;
            SpecialStringMaker.ShowSpecialString(() => DisplayMarkerLocation(4), () => true).Condition = () => 4 <= H;
            SpecialStringMaker.ShowSpecialString(() => DisplayMarkerLocation(5), () => true).Condition = () => 5 <= H;
        }

        public readonly string GameWonThisTurn = "GameWonThisTurn";

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // Both sides: "This card and villain Stat cards are indestructible."
            if (card == base.Card || (IsVillain(card) && base.GameController.GetAllKeywords(card).Contains(StatKeyword)))
            {
                return true;
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public IEnumerator AssignStatsResponse()
        {
            // Setup: "Each player puts a card from under this card in their play area “eDensity” side up and adds 30 tokens to it."
            Log.Debug("MapCharacterCardController.AssignStatsResponse called");
            IEnumerator setupCoroutine = DoActionToEachTurnTakerInTurnOrder((TurnTakerController ttc) => ttc.IsHero, SetupeDensityResponse);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(setupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(setupCoroutine);
            }
        }

        public IEnumerator SetupeDensityResponse(TurnTakerController ttc)
        {
            Log.Debug("MapCharacterCardController.SetupeDensityResponse called for " + ttc.TurnTaker.Name);
            // "... puts a card from under this card in their play area “eDensity” side up..."
            Card toPlay = base.Card.UnderLocation.TopCard;
            IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, toPlay, isPutIntoPlay: true, overridePlayLocation: ttc.TurnTaker.PlayArea, reassignPlayIndex: true, responsibleTurnTaker: base.TurnTaker, evenIfAlreadyInPlay: true, canBeCancelled: false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            // "... and adds 30 tokens to it."
            IEnumerator addCoroutine = base.GameController.AddTokensToPool(toPlay.FindTokenPool(WeightPoolIdentifier), 30, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(addCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(addCoroutine);
            }
        }

        public int DamageDealtToNonHeroByHeroThisTurn()
        {
            return (from ddje in base.Journal.DealDamageEntriesThisTurn() where !IsHeroTarget(ddje.TargetCard) && ddje.SourceCard != null && IsHeroTarget(ddje.SourceCard) select ddje).Sum((DealDamageJournalEntry ddje) => ddje.Amount);
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "When a hero target deals a non-hero target 3 or more damage, add 2 tokens to that player's Stat card and {SunSun} deals itself 1 infernal damage."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => !IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DidDealDamage && dda.Amount >= 3, RunScoredResponse, new TriggerType[] { TriggerType.AddTokensToPool, TriggerType.DealDamage }, TriggerTiming.After));
                // "When 8 or more damage is dealt to non-hero targets by hero targets during a player's turn, add 5 tokens to their Stat card and {SunSun} deals itself 3 infernal damage."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(GameWonThisTurn) && DamageDealtToNonHeroByHeroThisTurn() >= 8 && !IsHeroTarget(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DidDealDamage, GameWonResponse, new TriggerType[] { TriggerType.AddTokensToPool, TriggerType.DealDamage }, TriggerTiming.After));
                // "When a hero is dealt melee damage by a villain target, remove 2 tokens from their Stat card."
                AddSideTrigger(AddTrigger((DealDamageAction dda) => IsHeroCharacterCard(dda.Target) && dda.DamageSource != null && dda.DamageSource.IsCard && IsVillainTarget(dda.DamageSource.Card) && dda.DidDealDamage && dda.DamageType == DamageType.Melee, (DealDamageAction dda) => base.GameController.RemoveTokensFromPool(StatCardOf(dda.Target.Owner).FindTokenPool(WeightPoolIdentifier), 2, cardSource: GetCardSource()), TriggerType.AddTokensToPool, TriggerTiming.After));
                // "When {SunSun} is destroyed, remove it from the game, then flip {OwnershipCharacter} and this card, leaving all markers in place."
                AddSideTrigger(AddTrigger((DestroyCardAction dca) => dca.CardToDestroy.Card.Identifier == SunSunIdentifier && dca.CardToDestroy.CanBeDestroyed && dca.WasCardDestroyed, RemoveFromGameResponse, new TriggerType[] { TriggerType.RemoveFromGame, TriggerType.FlipCard }, TriggerTiming.After));
                AddSideTrigger(AddTrigger((MoveCardAction mca) => mca.CardToMove.Identifier == SunSunIdentifier && mca.Destination.IsOutOfGame, DoubleFlipResponse, TriggerType.FlipCard, TriggerTiming.After));
            }
            else
            {
                // Back side:
                // "When a player's marker moves to row 4, column 4, put a card from under this card in their play area “Rogue” side up."
                AddSideTrigger(AddTrigger((AddTokensToPoolAction tpa) => tpa.TokenPool.CardWithTokenPool == base.Card && tpa.NumberOfTokensToAdd > 0 && HeroIndexOfPool(tpa.TokenPool) != -1 && HeroMarkerLocation(HeroIndexOfPool(tpa.TokenPool))[0] == 4 && HeroMarkerLocation(HeroIndexOfPool(tpa.TokenPool))[1] == 4, (AddTokensToPoolAction tpa) => GoRogueResponse(HTTCAtIndex(HeroIndexOfPool(tpa.TokenPool))), TriggerType.MoveCard, TriggerTiming.After));
                // "When a player's marker moves to row 0, column 0, destroy each hero target in their play area."
                AddSideTrigger(AddTrigger((RemoveTokensFromPoolAction tpa) => tpa.TokenPool.CardWithTokenPool == base.Card && tpa.NumberOfTokensToRemove > 0 && HeroIndexOfPool(tpa.TokenPool) != -1 && HeroMarkerLocation(HeroIndexOfPool(tpa.TokenPool))[0] == 0 && HeroMarkerLocation(HeroIndexOfPool(tpa.TokenPool))[1] == 0, (RemoveTokensFromPoolAction tpa) => NullifyResponse(HTTCAtIndex(HeroIndexOfPool(tpa.TokenPool))), TriggerType.DestroyCard, TriggerTiming.After));
                // "After a player plays a card or uses a power, each player may discard a card. If {H - 2} cards are discarded this way, move another player's marker 1 space up, down, left, or right."
                AddSideTrigger(AddTrigger((PlayCardAction pca) => pca.WasCardPlayed && pca.TurnTakerController.IsHero, DiscardToMoveResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.AddTokensToPool }, TriggerTiming.After));
                AddSideTrigger(AddTrigger((UsePowerAction upa) => upa.IsSuccessful && upa.HeroUsingPower != null, DiscardToMoveResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.AddTokensToPool }, TriggerTiming.After));
                // "At the end of the villain turn, move each player's marker to the space indicated by the red arrow starting from its current space."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, RotateMapResponse, TriggerType.AddTokensToPool));
            }
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator baseCoroutine = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(baseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(baseCoroutine);
            }
            // Back side: "When this card flips to this side, put each villain Stat card under this card."
            if (base.Card.IsFlipped)
            {
                IEnumerator moveCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsVillain(c) && c.Identifier == StatCardIdentifier && c.Location != base.Card.UnderLocation, "villain Stat"), visibleToCard: GetCardSource()), base.Card.UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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

        public IEnumerator RunScoredResponse(DealDamageAction dda)
        {
            // "... add 2 tokens to that player's Stat card..."
            if (StatCardOf(dda.DamageSource.Card.Owner) != null)
            {
                IEnumerator addCoroutine = base.GameController.AddTokensToPool(StatCardOf(dda.DamageSource.Card.Owner).FindTokenPool(WeightPoolIdentifier), 2, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(addCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(addCoroutine);
                }
            }
            // "... and {SunSun} deals itself 1 infernal damage."
            Card sunsun = FindCard(SunSunIdentifier);
            if (sunsun.IsInPlayAndHasGameText)
            {
                IEnumerator infernalCoroutine = DealDamage(sunsun, sunsun, 1, DamageType.Infernal, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(infernalCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(infernalCoroutine);
                }
            }
        }

        public IEnumerator GameWonResponse(DealDamageAction dda)
        {
            SetCardProperty(GameWonThisTurn, true);
            // "... add 5 tokens to that player's Stat card..."
            if (StatCardOf(base.Game.ActiveTurnTaker) != null)
            {
                IEnumerator addCoroutine = base.GameController.AddTokensToPool(StatCardOf(base.Game.ActiveTurnTaker).FindTokenPool(WeightPoolIdentifier), 5, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(addCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(addCoroutine);
                }
            }
            // "... and {SunSun} deals itself 3 infernal damage."
            Card sunsun = FindCard(SunSunIdentifier);
            if (sunsun.IsInPlayAndHasGameText)
            {
                IEnumerator infernalCoroutine = DealDamage(sunsun, sunsun, 3, DamageType.Infernal, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(infernalCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(infernalCoroutine);
                }
            }
        }

        public IEnumerator RemoveFromGameResponse(DestroyCardAction dca)
        {
            // "...remove it from the game, ..."
            dca.SetPostDestroyDestination(base.TurnTaker.OutOfGame, showMessage: true, cardSource: GetCardSource());
            yield break;
        }

        public IEnumerator DoubleFlipResponse(MoveCardAction mca)
        {
            // "... then flip {OwnershipCharacter} ..."
            IEnumerator flipCoroutine = base.GameController.FlipCard(base.GameController.FindCardController(OwnershipIdentifier), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(flipCoroutine);
            }
            // "... and this card, leaving all markers in place."
            flipCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(flipCoroutine);
            }
        }

        public IEnumerator GoRogueResponse(HeroTurnTakerController httc)
        {
            // "... put a card from under this card in their play area “Rogue” side up."
            Card toPlay = base.Card.UnderLocation.TopCard;
            IEnumerator flipCoroutine = base.GameController.FlipCard(FindCardController(toPlay), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(flipCoroutine);
            }
            IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, toPlay, true, overridePlayLocation: httc.TurnTaker.PlayArea, responsibleTurnTaker: base.TurnTaker, evenIfAlreadyInPlay: true, reassignPlayIndex: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        public IEnumerator NullifyResponse(HeroTurnTakerController httc)
        {
            // "... destroy each hero target in their play area."
            LinqCardCriteria relevant = new LinqCardCriteria((Card c) => IsHeroTarget(c) && c.Location.IsPlayAreaOf(httc.TurnTaker), "in " + httc.TurnTaker.Name + "'s play area", useCardsPrefix: true, useCardsSuffix: false, singular: "hero target", plural: "hero targets");
            IEnumerable<Card> toNullify = base.GameController.FindCardsWhere(relevant, visibleToCard: GetCardSource());
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("The Horizon nullifies " + httc.TurnTaker.Name + "! All hero targets in their play area will be destroyed.", Priority.High, GetCardSource(), associatedCards: toNullify);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(DecisionMaker, relevant, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }

        public IEnumerator DiscardToMoveResponse(GameAction ga)
        {
            HeroTurnTakerController driving = DecisionMaker;
            if (ga is PlayCardAction)
            {
                PlayCardAction pca = (ga as PlayCardAction);
                driving = pca.TurnTakerController.ToHero();
                //Log.Debug("MapCharacterCardController called for " + driving.TurnTaker.Name + " playing " + pca.CardToPlay.Title);
            }
            else if (ga is UsePowerAction)
            {
                UsePowerAction upa = (ga as UsePowerAction);
                driving = upa.HeroUsingPower;
                //Log.Debug("MapCharacterCardController called for " + driving.TurnTaker.Name + " using the power on " + upa.Power.CardController.Card.Title);
            }
            else
            {
                //Log.Debug("MapCharacterCardController called with ga: " + ga.ToString());
            }
            // "... each player may discard a card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = EachPlayerDiscardsCards(0, 1, discards, showCounter: true, sinceAction: ga, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If {H - 2} cards are discarded this way, move another player's marker 1 space up, down, left, or right."
            if (DidDiscardCards(discards, H-2))
            {
                // Player driving: choose another player's marker to move and move it
                IEnumerator moveCoroutine = base.GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(base.GameController, driving, from httc in FindActiveHeroTurnTakerControllers() where httc != driving select httc.TurnTaker, SelectionType.Custom, cardSource: GetCardSource()), (TurnTaker tt) => SelectDirectionAndMove(driving, tt));
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

        public IEnumerator EachPlayerDiscardsCards(int minNumberOfCardsPerHero, int? maxNumberOfCardsPerHero, List<DiscardCardAction> storedResultsDiscard = null, bool allowAutoDecideHeroes = true, int? requiredNumberOfHeroes = null, bool showCounter = false, GameAction sinceAction = null, LinqCardCriteria cardCriteria = null, bool ignoreBattleZone = false, CardSource cardSource = null)
        {
            if (minNumberOfCardsPerHero == 0)
            {
                requiredNumberOfHeroes = 0;
            }
            if (cardCriteria == null)
            {
                cardCriteria = new LinqCardCriteria();
            }
            Func<string> counter = null;
            if (showCounter && cardSource != null)
            {
                bool canCompare = false;
                if (sinceAction != null)
                {
                    JournalEntry record = null;
                    if (sinceAction is PlayCardAction pca)
                    {
                        record = base.GameController.Game.Journal.QueryJournalEntries((PlayCardJournalEntry pcje) => pcje.CardPlayed == pca.CardToPlay && pcje.TurnIndex == base.Game.TurnIndex).LastOrDefault();
                    }
                    else if (sinceAction is UsePowerAction upa)
                    {
                        record = base.GameController.Game.Journal.QueryJournalEntries((UsePowerJournalEntry upje) => upje.PowerUser == upa.HeroUsingPower.HeroTurnTaker && upje.CardWithPower == upa.Power.CardController.CardWithoutReplacements && upje.TurnIndex == base.Game.TurnIndex).LastOrDefault();
                    }
                    if (record != null && base.GameController.Game.Journal.GetEntryIndex(record).HasValue)
                    {
                        canCompare = true;
                        counter = () => "Cards discarded so far: " + (from en in Game.Journal.DiscardCardEntriesThisTurn()
                                                                      where en.Card.Owner.IsPlayer && en.CardSource == cardSource.Card && en.CardSourcePlayIndex == cardSource.Card.PlayIndex && base.GameController.Game.Journal.GetEntryIndex(en).HasValueGreaterThan(base.GameController.Game.Journal.GetEntryIndex(record).Value)
                                                                      select en).Count();
                    }
                }
                if (!canCompare)
                {
                    counter = () => "Cards discarded so far: " + (from en in Game.Journal.DiscardCardEntriesThisTurn()
                                                                  where en.Card.Owner.IsPlayer && en.CardSource == cardSource.Card && en.CardSourcePlayIndex == cardSource.Card.PlayIndex
                                                                  select en).Count();
                }
            }
            int? num = null;
            if (minNumberOfCardsPerHero == maxNumberOfCardsPerHero)
            {
                num = minNumberOfCardsPerHero;
            }
            LinqTurnTakerCriteria turnTakerCriteria = new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && (tt as HeroTurnTaker).HasCardsInHand && (tt as HeroTurnTaker).Hand.Cards.Where(cardCriteria.Criteria).Count() > 0, $"heroes with {cardCriteria.GetDescription()} in hand");
            Func<TurnTaker, IEnumerator> actionWithTurnTaker = (TurnTaker tt) => base.GameController.SelectAndDiscardCards(FindHeroTurnTakerController((HeroTurnTaker)tt), maxNumberOfCardsPerHero, optional: false, minNumberOfCardsPerHero, storedResultsDiscard, allowAutoDecide: false, null, null, counter, cardCriteria, SelectionType.DiscardCard, tt, cardSource);
            int? requiredDecisions = requiredNumberOfHeroes;
            bool allowAutoDecide = allowAutoDecideHeroes;
            Func<string> extraInfo = counter;
            bool ignoreBattleZone2 = ignoreBattleZone;
            int? numberOfCards = num;
            CardSource cardSource2 = cardSource;
            return base.GameController.SelectTurnTakersAndDoAction(null, turnTakerCriteria, SelectionType.DiscardCard, actionWithTurnTaker, null, optional: false, requiredDecisions, null, allowAutoDecide, null, extraInfo, null, ignoreBattleZone2, numberOfCards, cardSource2);
        }

        public IEnumerator SelectDirectionAndMove(HeroTurnTakerController driving, TurnTaker moving)
        {
            int heroIndex = IndexOfHero(base.GameController.FindHeroTurnTakerController(moving.ToHero()));
            int[] currentLocation = HeroMarkerLocation(heroIndex);
            List<Function> options = new List<Function>();
            options.Add(new Function(driving, "Move " + moving.Name + " up to row " + (currentLocation[0] + 1).ToString() + ", column " + currentLocation[1].ToString(), SelectionType.AddTokens, () => MoveHeroMarker(heroIndex, 1, 0, driving.TurnTaker, false, GetCardSource()), currentLocation[0] < 4));
            options.Add(new Function(driving, "Move " + moving.Name + " down to row " + (currentLocation[0] - 1).ToString() + ", column " + currentLocation[1].ToString(), SelectionType.RemoveTokens, () => MoveHeroMarker(heroIndex, -1, 0, driving.TurnTaker, false, GetCardSource()), currentLocation[0] > 0));
            options.Add(new Function(driving, "Move " + moving.Name + " left to row " + currentLocation[0].ToString() + ", column " + (currentLocation[1] - 1).ToString(), SelectionType.RemoveTokens, () => MoveHeroMarker(heroIndex, 0, -1, driving.TurnTaker, false, GetCardSource()), currentLocation[1] > 0));
            options.Add(new Function(driving, "Move " + moving.Name + " right to row " + currentLocation[0].ToString() + ", column " + (currentLocation[1] + 1).ToString(), SelectionType.AddTokens, () => MoveHeroMarker(heroIndex, 0, 1, driving.TurnTaker, false, GetCardSource()), currentLocation[1] < 4));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, driving, options, false, associatedCards: moving.CharacterCards, cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
        }

        public IEnumerator RotateMapResponse(PhaseChangeAction pca)
        {
            // "... move each player's marker to the space indicated by the red arrow starting from its current space."
            IEnumerator rotateCoroutine = DoActionToEachTurnTakerInTurnOrder((TurnTakerController ttc) => ttc.IsHero, DragTokenResponse, responsibleTurnTaker: base.TurnTaker);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(rotateCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(rotateCoroutine);
            }
        }

        public IEnumerator DragTokenResponse(TurnTakerController ttc)
        {
            int heroIndex = IndexOfHero(ttc.ToHero());
            int[] currentLocation = HeroMarkerLocation(heroIndex);
            int rowChange = 0;
            int colChange = 0;
            // Determine direction to move based on current location
            if (currentLocation[0]==2 && currentLocation[1]==2)
            {
                // On the Coin's spot? Move toward Horizon
                rowChange = -1;
                colChange = -1;
            }
            else
            {
                if (currentLocation[0] < 4 && currentLocation[1] < 2)
                {
                    // First two columns, not top row? Going up
                    rowChange = 1;
                }
                else if (currentLocation[0] > 0 && currentLocation[1] > 2)
                {
                    // Last two columns, not bottom row? Going down
                    rowChange = -1;
                }

                if (currentLocation[0] > 2 && currentLocation[1] < 4)
                {
                    // Top two rows, not last column? Going right
                    colChange = 1;
                }
                else if (currentLocation[0] < 2 && currentLocation[1] > 0)
                {
                    // Bottom two rows, not first column? Going left
                    colChange = -1;
                }
            }
            IEnumerator dragCoroutine = MoveHeroMarker(heroIndex, rowChange, colChange, showMessage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(dragCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(dragCoroutine);
            }
        }

        public override CustomDecisionText GetCustomDecisionText(IDecision decision)
        {
            return new CustomDecisionText("Whose marker would you like to move?", "deciding whose marker to move", "Vote for whose marker to move", "whose marker to move", autoDecideText: "marker to move");
        }
    }
}
