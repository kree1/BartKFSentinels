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
    class TheShelledOneCharacterCardController : VillainCharacterCardController
    {
        public TheShelledOneCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            AddThisCardControllerToList(CardControllerListType.ModifiesKeywords);
            SpecialStringMaker.ShowTokenPool(base.Card.Identifier, StrikePoolIdentifier).Condition = () => !base.Card.IsFlipped;
            SpecialStringMaker.ShowNumberOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsVillainTarget && !c.IsCharacter, "non-character villain targets", false, false, "non-character villain target", "non-character villain targets")).Condition = () => base.Card.IsFlipped;
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain("weather effect"), "Weather Effect")).Condition = () => base.Card.IsFlipped;
        }

        public const string GiantPeanutShellIdentifier = "GiantPeanutShell";
        public const string StrikePoolIdentifier = "TheShelledOneStrikePool";
        public const int MinimumMaxHP = 6;
        protected const string OnePodPerTurn = "PlayPodOncePerTurn";

        public Func<Card, bool> getsMaxHPSet = (Card c) => (c.IsVillain && c.DoKeywordsContain("ongoing")) || (c.IsEnvironment && (!c.IsTarget || c.MaximumHitPoints < MinimumMaxHP));
        public Func<Card, bool> removeMaxHP = (Card c) => (c.IsVillain || c.IsEnvironment) && ((!c.IsCharacter && !c.Definition.HitPoints.HasValue) || (c.IsCharacter && !c.IsFlipped && !c.Definition.HitPoints.HasValue) || (c.IsCharacter && c.IsFlipped && !c.Definition.FlippedHitPoints.HasValue));
        public Func<Card, bool> resetMaxHP = (Card c) => c.IsEnvironment && c.Definition.HitPoints.HasValue && c.MaximumHitPoints.HasValue && c.MaximumHitPoints.Value == MinimumMaxHP && c.Definition.HitPoints.Value < c.MaximumHitPoints.Value;

        public override bool AskIfCardIsIndestructible(Card card)
        {
            if (!base.Card.IsFlipped)
            {
                // "Strikes are indestructible..."
                // "Cards under this card are indestructible."
                if (card.DoKeywordsContain("strike") || card.Location == base.Card.UnderLocation)
                {
                    return true;
                }
            }
            else
            {
                // "Giant Peanut Shell is indestructible."
                if (card.Identifier == GiantPeanutShellIdentifier)
                {
                    return true;
                }
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override bool AskIfCardContainsKeyword(Card card, string keyword, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
        {
            if (base.Card.IsFlipped)
            {
                // "The hero next to [Giant Peanut Shell] gains the keyword Pod."
                if (card.IsHeroCharacterCard && keyword == "pod" && card.NextToLocation.HasCard(base.TurnTaker.FindCard(GiantPeanutShellIdentifier)) && base.GameController.IsCardVisibleToCardSource(card, GetCardSource()) && base.GameController.IsCardVisibleToCardSource(base.TurnTaker.FindCard(GiantPeanutShellIdentifier), GetCardSource()))
                {
                    return true;
                }
            }
            return base.AskIfCardContainsKeyword(card, keyword, evenIfUnderCard, evenIfFaceDown);
        }

        public override IEnumerable<string> AskForCardAdditionalKeywords(Card card)
        {
            if (base.Card.IsFlipped)
            {
                // "The hero next to [Giant Peanut Shell] gains the keyword Pod."
                if (card.IsHeroCharacterCard && card.NextToLocation.HasCard(base.TurnTaker.FindCard(GiantPeanutShellIdentifier)) && base.GameController.IsCardVisibleToCardSource(card, GetCardSource()) && base.GameController.IsCardVisibleToCardSource(base.TurnTaker.FindCard(GiantPeanutShellIdentifier), GetCardSource()))
                {
                    return new string[] { "pod" };
                }
            }
            return base.AskForCardAdditionalKeywords(card);
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Strikes are ... immune to damage."
                base.AddSideTrigger(base.AddImmuneToDamageTrigger((DealDamageAction dda) => dda.Target.DoKeywordsContain("strike")));
                // "Villain Ongoing cards and environment cards with no printed HP or a printed HP of less than 6 have a maximum HP of 6."
                base.AddSideTriggers(base.AddMaintainTargetTriggers((Card c) => getsMaxHPSet(c), MinimumMaxHP, new List<string> { "ongoing" }));
                // "At the end of the villain turn, each environment target deals each hero target 1 psychic damage. Then, play the top card of the environment deck."
                base.AddSideTrigger(base.AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, EnvironmentDamagePlayResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard }));
                // "When there are 4 tokens on this card, flip this card."
                AddSideTrigger(AddTrigger((AddTokensToPoolAction tpa) => tpa.TokenPool.Identifier == StrikePoolIdentifier && tpa.TokenPool.CurrentValue >= 4, FourStrikesResponse, TriggerType.FlipCard, TriggerTiming.After));

                if (base.IsGameAdvanced)
                {
                    // "Whenever a Strike causes a token to be put on this card, remove that Strike from the game."
                    AddSideTrigger(AddTrigger((AddTokensToPoolAction tpa) => tpa.TokenPool.Identifier == StrikePoolIdentifier && tpa.CardSource.Card.DoKeywordsContain("strike"), RemoveStrikeResponse, TriggerType.RemoveFromGame, TriggerTiming.After));
                }
            }
            else
            {
                // Back side:
                // "The hero next to [Giant Peanut Shell] gains the keyword Pod."
                AddSideTrigger(AddTrigger((MoveCardAction mca) => mca.CardToMove == base.TurnTaker.FindCard(GiantPeanutShellIdentifier), RemoveKeywordResponse, TriggerType.Hidden, TriggerTiming.After));
                AddSideTrigger(AddTrigger((MoveCardAction mca) => mca.CardToMove == base.TurnTaker.FindCard(GiantPeanutShellIdentifier), AddKeywordResponse, TriggerType.Hidden, TriggerTiming.After));
                // "The first time a Pod is discarded from the villain deck each turn, put that Pod into play."
                AddSideTrigger(AddTrigger((MoveCardAction mca) => !HasBeenSetToTrueThisTurn(OnePodPerTurn) && mca.IsDiscard && mca.Origin.IsVillain, CheckForDiscardedPodResponse, TriggerType.PutIntoPlay, TriggerTiming.After));
                AddSideTrigger(AddTrigger((BulkMoveCardsAction bmca) => !HasBeenSetToTrueThisTurn(OnePodPerTurn) && bmca.IsDiscard, CheckForBulkDiscardedPodResponse, TriggerType.PutIntoPlay, TriggerTiming.After));
                // "At the end of the villain turn, discard cards from the top of the villain deck until you discard a Weather Effect. Put that Weather Effect into play. Then, {TheShelledOne} regains HP equal to the number of non-character villain targets in play. Then, the hero with the highest HP deals {TheShelledOne} 0 projectile damage."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealPitchWeatherResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.DealDamage, TriggerType.DiscardCard, TriggerType.PlayCard }));

                if (base.IsGameAdvanced)
                {
                    // "Reduce damage dealt to {TheShelledOne} by 1."
                    base.AddSideTrigger(AddReduceDamageTrigger((Card c) => c == base.Card, 1));
                }
            }
            // If The Shelled One is destroyed or removed, the heroes win.
            base.AddDefeatedIfDestroyedTriggers();
            base.AddDefeatedIfMovedOutOfGameTriggers();
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            RemoveSideTriggers();
            AddSideTriggers();
            if (base.Card.IsFlipped)
            {
                // Cards that were only targets due to this side's effects stop being targets
                IEnumerator removeTargetCoroutine = base.GameController.RemoveTargets((Card c) => removeMaxHP(c), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(removeTargetCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(removeTargetCoroutine);
                }
                // Cards that had their maximum HP increased by this card's effects have it restored to its usual value
                IEnumerable<Card> toReset = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => resetMaxHP(c)));
                foreach (Card c in toReset)
                {
                    bool editCurrent = false;
                    if (c.HitPoints.HasValue)
                    {
                        editCurrent = c.HitPoints.Value > c.Definition.HitPoints.Value;
                    }
                    IEnumerator resetCoroutine = base.GameController.ChangeMaximumHP(c, c.Definition.HitPoints.Value, editCurrent, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(resetCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(resetCoroutine);
                    }
                }
                // This card starts being a target
                IEnumerator targetCoroutine = base.GameController.ChangeMaximumHP(base.Card, base.Card.Definition.FlippedHitPoints.Value, alsoSetHP: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(targetCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(targetCoroutine);
                }
                // "When {TheShelledOne} flips to this side..."
                // "... remove all Strikes from the game."
                IEnumerator removeCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.DoKeywordsContain("strike"), "Strike"), visibleToCard: GetCardSource()), base.TurnTaker.OutOfGame, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(removeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(removeCoroutine);
                }
                // "... search the villain deck and trash for {GiantPeanutShell} and put it into play."
                IEnumerator searchCoroutine = PlayCardFromLocations(new Location[] { base.TurnTaker.Deck, base.TurnTaker.Trash }, GiantPeanutShellIdentifier, isPutIntoPlay: true, shuffleAfterwardsIfDeck: false);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(searchCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(searchCoroutine);
                }
                IEnumerator messageCoroutine = base.GameController.SendMessageAction("[b]I AM HERE{BR}AND YOU ARE OUT{BR}COME TO ME MY PODS[/b]", Priority.High, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                // "... reveal all cards under this card."
                List<RevealCardsAction> revealing = new List<RevealCardsAction>();
                IEnumerator revealCoroutine = base.GameController.RevealCards(base.TurnTakerController, base.Card.UnderLocation, (Card c) => false, 1, revealing, RevealedCardDisplay.Message, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(revealCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(revealCoroutine);
                }
                // "Put a random Pitcher and a random Batter from among them into play."
                IEnumerable<Card> revealedCards = base.TurnTaker.Revealed.Cards;
                Log.Debug("underCards.Count(): " + revealedCards.Count().ToString());
                foreach (Card c in revealedCards)
                {
                    Log.Debug("Title: " + c.Title);
                    foreach (string k in c.GetKeywords())
                    {
                        Log.Debug("    Keyword: " + k);
                    }
                }
                IEnumerable <Card> revealedPitchers = base.TurnTaker.Revealed.Cards.Where((Card c) => c.DoKeywordsContain("pitcher"));
                Log.Debug("revealedPitchers.Count(): " + revealedPitchers.Count().ToString());
                Card pitcherAssigned = revealedPitchers.ElementAt(Game.RNG.Next(0, revealedPitchers.Count()));
                IEnumerator fieldPitcherCoroutine = base.GameController.PlayCard(base.TurnTakerController, pitcherAssigned, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, evenIfAlreadyInPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(fieldPitcherCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(fieldPitcherCoroutine);
                }
                IEnumerable<Card> revealedBatters = base.TurnTaker.Revealed.Cards.Where((Card c) => c.DoKeywordsContain("batter"));
                Log.Debug("revealedBatters.Count(): " + revealedBatters.Count().ToString());
                Card batterAssigned = revealedBatters.ElementAt(Game.RNG.Next(0, revealedBatters.Count()));
                IEnumerator fieldBatterCoroutine = base.GameController.PlayCard(base.TurnTakerController, batterAssigned, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, evenIfAlreadyInPlay: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(fieldBatterCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(fieldBatterCoroutine);
                }
                // "... shuffle the other revealed cards into the villain deck."
                IEnumerator shuffleCoroutine = base.GameController.ShuffleCardsIntoLocation(DecisionMaker, base.TurnTaker.Revealed.Cards, base.TurnTaker.Deck, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator EnvironmentDamagePlayResponse(GameAction ga)
        {
            // "... each environment target deals each hero target 1 psychic damage."
            IEnumerator damageCoroutine = MultipleDamageSourcesDealDamage(new LinqCardCriteria((Card c) => c.IsEnvironmentTarget, "environment targets", false, false, "environment target", "environment targets"), TargetType.All, null, new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget, "hero target", false, false, "hero target", "hero targets"), 1, DamageType.Psychic);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Then, play the top card of the environment deck."
            IEnumerator playCoroutine = base.GameController.PlayTopCard(DecisionMaker, FindEnvironment(), responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }

        public IEnumerator FourStrikesResponse(GameAction ga)
        {
            // "... flip this card."
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("[i]EMERGENCY ALERT{BR}INCOMING{BR}SEEK SHELTER[/i]", Priority.High, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator flipCoroutine = base.GameController.FlipCard(this, actionSource: ga, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(flipCoroutine);
            }
            yield break;
        }

        public IEnumerator RemoveStrikeResponse(AddTokensToPoolAction tpa)
        {
            // "... remove that Strike from the game."
            Card responsible = tpa.CardSource.Card;
            if (responsible.DoKeywordsContain("strike"))
            {
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, responsible, base.TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: tpa, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator AddKeywordResponse(GameAction ga)
        {
            Card shell = base.TurnTaker.FindCard(GiantPeanutShellIdentifier);
            List<Card> affectedCards = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.NextToLocation.HasCard(shell), "hero next to Giant Peanut Shell", false, false, "hero next to Giant Peanut Shell", "heroes next to Giant Peanut Shell"), visibleToCard: GetCardSource()).ToList();
            IEnumerator addCoroutine = base.GameController.ModifyKeywords("pod", true, affectedCards: affectedCards, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(addCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(addCoroutine);
            }
            yield break;
        }

        public IEnumerator RemoveKeywordResponse(GameAction ga)
        {
            Card shell = base.TurnTaker.FindCard(GiantPeanutShellIdentifier);
            List<Card> unaffectedCards = FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && !c.NextToLocation.HasCard(shell), "hero not next to Giant Peanut Shell", false, false, "hero not next to Giant Peanut Shell", "heroes not next to Giant Peanut Shell")).ToList();
            IEnumerator removeCoroutine = base.GameController.ModifyKeywords("pod", false, affectedCards: unaffectedCards, cardSource: GetCardSource());
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

        public IEnumerator CheckForDiscardedPodResponse(MoveCardAction mca)
        {
            //Log.Debug("TheShelledOneCharacterCardController.CheckForDiscardedPodResponse activated");
            //Log.Debug("CheckForDiscardedPodResponse: mca.WasCardMoved: " + mca.WasCardMoved.ToString());
            //Log.Debug("CheckForDiscardedPodResponse: mca.CardToMove == null: " + (mca.CardToMove == null).ToString());
            // "... put that Pod into play."
            if (mca.WasCardMoved && mca.CardToMove != null)
            {
                Card discarded = mca.CardToMove;
                // Make sure the Pod was discarded from the villain deck or the villain revealed zone
                bool flag = false;
                MoveCardJournalEntry mostRecent = (from mc in base.Journal.MoveCardEntriesThisTurn() where mc.Card == discarded && mc.ToLocation.IsTrash select mc).LastOrDefault();
                /*Log.Debug("CheckForDiscardedPodResponse: mostRecent == null: " + (mostRecent == null).ToString());
                if (mostRecent != null)
                {
                    Log.Debug("CheckForDiscardedPodResponse: mostRecent.FromLocation.IsVillain: " + mostRecent.FromLocation.IsVillain.ToString());
                }*/
                if (mostRecent != null && mostRecent.FromLocation.IsVillain)
                {
                    //Log.Debug("CheckForDiscardedPodResponse: mostRecent.FromLocation.IsDeck: " + mostRecent.FromLocation.IsDeck.ToString());
                    //Log.Debug("CheckForDiscardedPodResponse: mostRecent.FromLocation.IsRevealed: " + mostRecent.FromLocation.IsRevealed.ToString());
                    if (mostRecent.FromLocation.IsDeck)
                    {
                        flag = true;
                    }
                    else if (mostRecent.FromLocation.IsRevealed)
                    {
                        MoveCardJournalEntry mostRecentReveal = (from mc in base.Journal.MoveCardEntriesThisTurn() where mc.Card == discarded && mc.ToLocation.IsRevealed select mc).LastOrDefault();
                        /*Log.Debug("CheckForDiscardedPodResponse: mostRecentReveal == null: " + (mostRecentReveal == null).ToString());
                        if (mostRecentReveal != null)
                        {
                            Log.Debug("CheckForDiscardedPodResponse: mostRecentReveal.FromLocation.IsDeck: " + mostRecentReveal.FromLocation.IsDeck.ToString());
                        }*/
                        if (mostRecentReveal != null && mostRecentReveal.FromLocation.IsDeck)
                        {
                            flag = true;
                        }
                    }
                }
                //Log.Debug("CheckForDiscardedPodResponse: flag: " + flag.ToString());
                //Log.Debug("CheckForDiscardedPodResponse: discarded.DoKeyWordsContain(\"pod\", true, true): " + discarded.DoKeywordsContain("pod", true, true).ToString());
                // If so, set the "did this already this turn" CardProperty and put it into play
                if (flag && discarded.DoKeywordsContain("pod", true, true))
                {
                    SetCardPropertyToTrueIfRealAction(OnePodPerTurn);
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " plays the first discarded Pod!", Priority.Medium, GetCardSource(), associatedCards: discarded.ToEnumerable(), showCardSource: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                    IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, discarded, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, actionSource: mca, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(putCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(putCoroutine);
                    }
                }
            }
            yield break;
        }

        public IEnumerator CheckForBulkDiscardedPodResponse(BulkMoveCardsAction bmca)
        {
            // "... put that Pod into play."
            //Log.Debug("TheShelledOneCharacterCardController.CheckForBulkDiscardedPodResponse activated");
            //Log.Debug("CheckForBulkDiscardedPodResponse: bmca.IsDiscard: " + bmca.IsDiscard.ToString());
            //Log.Debug("CheckForBulkDiscardedPodResponse: bmca.CardsToMove.Any((Card c) => c.DoKeywordsContain(\"pod\", true, true)): " + bmca.CardsToMove.Any((Card c) => c.DoKeywordsContain("pod", true, true)).ToString());
            if (bmca.IsDiscard && bmca.CardsToMove.Any((Card c) => c.DoKeywordsContain("pod", true, true)))
            {
                Card discardedPod = bmca.CardsToMove.Where((Card c) => c.DoKeywordsContain("pod", true, true)).FirstOrDefault();
                // Make sure the Pod was discarded from the villain deck or the villain revealed zone
                bool flag = false;
                MoveCardJournalEntry mostRecent = (from mc in base.Journal.MoveCardEntriesThisTurn() where mc.Card == discardedPod && mc.ToLocation.IsTrash select mc).LastOrDefault();
                /*Log.Debug("CheckForBulkDiscardedPodResponse: mostRecent == null: " + (mostRecent == null).ToString());
                if (mostRecent != null)
                {
                    Log.Debug("CheckForBulkDiscardedPodResponse: mostRecent.FromLocation.IsVillain: " + mostRecent.FromLocation.IsVillain.ToString());
                }*/
                if (mostRecent != null && mostRecent.FromLocation.IsVillain)
                {
                    //Log.Debug("CheckForBulkDiscardedPodResponse: mostRecent.FromLocation.IsDeck: " + mostRecent.FromLocation.IsDeck.ToString());
                    //Log.Debug("CheckForBulkDiscardedPodResponse: mostRecent.FromLocation.IsRevealed: " + mostRecent.FromLocation.IsRevealed.ToString());
                    if (mostRecent.FromLocation.IsDeck)
                    {
                        flag = true;
                    }
                    else if (mostRecent.FromLocation.IsRevealed)
                    {
                        MoveCardJournalEntry mostRecentReveal = (from mc in base.Journal.MoveCardEntriesThisTurn() where mc.Card == discardedPod && mc.ToLocation.IsRevealed select mc).LastOrDefault();
                        /*Log.Debug("CheckForBulkDiscardedPodResponse: mostRecentReveal == null: " + (mostRecentReveal == null).ToString());
                        if (mostRecentReveal != null)
                        {
                            Log.Debug("CheckForBulkDiscardedPodResponse: mostRecentReveal.FromLocation.IsDeck: " + mostRecentReveal.FromLocation.IsDeck.ToString());
                        }*/
                        if (mostRecentReveal != null && mostRecentReveal.FromLocation.IsDeck)
                        {
                            flag = true;
                        }
                    }
                }
                //Log.Debug("CheckForBulkDiscardedPodResponse: flag: " + flag.ToString());
                //Log.Debug("CheckForBulkDiscardedPodResponse: discarded.DoKeyWordsContain(\"pod\", true, true): " + discardedPod.DoKeywordsContain("pod", true, true).ToString());
                // If so, set the "did this already this turn" CardProperty and put it into play
                if (flag && discardedPod.DoKeywordsContain("pod", true, true))
                {
                    SetCardPropertyToTrueIfRealAction(OnePodPerTurn);
                    IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " plays the first discarded Pod!", Priority.Medium, GetCardSource(), associatedCards: discardedPod.ToEnumerable(), showCardSource: true);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(messageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(messageCoroutine);
                    }
                    IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, discardedPod, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, actionSource: bmca, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(putCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(putCoroutine);
                    }
                }
            }
            yield break;
        }

        public IEnumerator HealPitchWeatherResponse(GameAction ga)
        {
            // "... discard cards from the top of the villain deck until you discard a Weather Effect. Put that Weather Effect into play."
            // The reveal cards -> play some -> discard others helper methods will stop if they run out of cards to reveal, so we can't use those
            List<MoveCardAction> moves = new List<MoveCardAction>();
            while (!moves.Where((MoveCardAction mca) => mca.IsDiscard && mca.CardToMove != null && mca.CardToMove.DoKeywordsContain("weather effect", true, true)).Any())
            {
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, moves, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            List<MoveCardAction> relevantDiscards = moves.Where((MoveCardAction mca) => mca.IsDiscard && mca.CardToMove != null && mca.CardToMove.DoKeywordsContain("weather effect", true, true)).ToList();
            MoveCardAction first = relevantDiscards.FirstOrDefault();
            if (first != null)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " brings about a change in the weather!", Priority.Medium, GetCardSource(), showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                Card toPlay = first.CardToMove;
                if (toPlay != null)
                {
                    IEnumerator putCoroutine = base.GameController.PlayCard(base.TurnTakerController, toPlay, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(putCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(putCoroutine);
                    }
                }
            }
            // "Then, {TheShelledOne} regains HP equal to the number of non-character villain targets in play."
            IEnumerator healCoroutine = base.GameController.GainHP(base.Card, base.GameController.FindCardsWhere((Card c) => c.IsVillainTarget && c.IsInPlayAndHasGameText && !c.IsCharacter).Count(), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "Then, the hero with the highest HP deals {TheShelledOne} 0 projectile damage."
            List<Card> highestResults = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHeroCharacterCard, highestResults, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            Card pitchingHero = highestResults.FirstOrDefault();
            if (pitchingHero != null)
            {
                IEnumerator pitchCoroutine = base.GameController.DealDamage(DecisionMaker, pitchingHero, (Card c) => c == base.Card, 0, DamageType.Projectile, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(pitchCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(pitchCoroutine);
                }
            }
            yield break;
        }
    }
}
