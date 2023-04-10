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
    public class BreakawayCharacterCardController : VillainCharacterCardController
    {
        public BreakawayCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Display strings...
            // Front side: has Momentum been dealt damage this turn?
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(momentumTakenDamage), () => "Momentum has already been dealt damage this turn.", () => "Momentum has not yet been dealt damage this turn.").Condition = () => !base.Card.IsFlipped;
            // Front side: what environment cards have dealt damage to Momentum this turn?
            SpecialStringMaker.ShowSpecialString(HitMomentumThisTurn).Condition = () => !base.Card.IsFlipped;
            // Back side: what is Momentum's current HP? What effects does that have?
            SpecialStringMaker.ShowSpecialString(DeadEndJobMomentumEffects).Condition = () => base.Card.IsFlipped;
            // Back side: has a hero card entered play this turn?
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(heroCardEntered), () => "A hero card has already entered play this turn.", () => "A hero card has not yet entered play this turn.").Condition = () => base.Card.IsFlipped;
            // Back side: who are the 2 hero targets with the highest HP?
            SpecialStringMaker.ShowHeroTargetWithHighestHP(1, 2).Condition = () => base.Card.IsFlipped;
            // Back side, Advanced: if it's a hero turn, has Breakaway dealt damage this turn?
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(dealtDamageHero), () => "Breakaway has already dealt damage this turn", () => "Breakaway has not yet dealt damage this turn", () => IsHero(base.GameController.ActiveTurnTaker)).Condition = () => (base.Game.IsAdvanced && base.Card.IsFlipped);
        }

        private List<Card> hitMomentumLastTurn = new List<Card>();

        protected const string momentumTakenDamage = "MomentumTookDamageThisTurn";
        protected const string heroCardEntered = "HeroCardEnteredPlayThisTurn";
        protected const string dealtDamageHero = "BreakawayDealtDamageThisHeroTurn";

        private string DeadEndJobMomentumEffects()
        {
            string effectsList = "";
            string[] allEffects = { "Damage dealt by Breakaway is increased by 1.", "Damage dealt to Breakaway is reduced by 1.", "Damage dealt by Breakaway is irreducible." };
            int momentumHP = base.TurnTaker.FindCard("MomentumCharacter").HitPoints.Value;
            int threshold = (int) (momentumHP-1) / Game.H;
            effectsList = "Momentum's current HP is " + momentumHP.ToString();
            if (threshold > 0)
            {
                effectsList += " (> " + (Game.H * threshold).ToString() + "). The following effects are active:";
                for (int i = 3; i > 0; i--)
                {
                    if (threshold >= i)
                    {
                        effectsList += "\n* " + allEffects[i - 1];
                    }
                }
            }
            else
            {
                effectsList += " (<= " + Game.H.ToString() + ").";
            }
            return effectsList;
        }

        private string HitMomentumThisTurn()
        {
            string hitList = base.TurnTaker.FindCard("MomentumCharacter").Title + " has not been dealt damage by environment cards this turn.";
            if (hitMomentumLastTurn.Any())
            {
                hitList = "Environment cards that dealt damage to " + base.TurnTaker.FindCard("MomentumCharacter").Title + " this turn: ";
                for (int i = 0; i < hitMomentumLastTurn.Count - 1; i++)
                {
                    hitList += hitMomentumLastTurn.ElementAt(i).Title + ", ";
                }
                hitList += hitMomentumLastTurn.Last().Title + ".";
            }
            return hitList;
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "{Breakaway} is immune to damage."
                base.AddSideTrigger(base.AddImmuneToDamageTrigger((DealDamageAction d) => d.Target == base.CharacterCard));

                // "When {Breakaway}'s current HP is equal to his maximum HP, he escapes with the loot! [b]GAME OVER.[/b]"
                base.AddSideTrigger(base.AddTrigger<GainHPAction>((GainHPAction gha) => gha.IsSuccessful && gha.HpGainer.Equals(this.Card), BreakawayHPCheckResponse, TriggerType.GameOver, TriggerTiming.After));

                // "The first time {Momentum} is dealt damage each turn, if that damage reduces its HP to 0 or less, remove 2 HP from {Breakaway}."
                base.AddSideTrigger(base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(momentumTakenDamage) && dda.Target == base.TurnTaker.FindCard("MomentumCharacter") && dda.Amount > 0, MomentumFirstDamageResponse, TriggerType.Other, TriggerTiming.After, isConditional: true));

                // "At the start of each turn, {Momentum} becomes immune to damage dealt by each environment card that dealt damage to it during the previous turn."
                base.AddSideTrigger(base.AddTrigger<DealDamageAction>((DealDamageAction dda) => (dda.DamageSource.IsEnvironmentCard || dda.DamageSource.IsEnvironmentSource) && dda.Target == base.TurnTaker.FindCard("MomentumCharacter"), MomentumHitByEnvironmentResponse, TriggerType.Hidden, TriggerTiming.After));
                base.AddSideTrigger(base.AddStartOfTurnTrigger((TurnTaker tt) => true, CriminalCourierStartEachTurnResponse, TriggerType.CreateStatusEffect));
                
                // "Whenever a villain card would go anywhere except the villain trash, deck, or play area, first reveal that card. If {TheClient} is revealed this way, flip {Breakaway}."
                base.AddSideTrigger(base.AddTrigger<MoveCardAction>((MoveCardAction mca) =>  IsVillain(mca.CardToMove) && (!mca.Destination.IsVillain || !(mca.Destination.IsInPlay || mca.Destination.IsTrash || mca.Destination.IsDeck)), UnusualMoveResponse, TriggerType.RevealCard, TriggerTiming.Before));
                
                // "Whenever a Terrain card enters play, destroy all other Terrain cards and all environment cards, then play the top card of the environment deck."
                base.AddSideTrigger(base.AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardToPlay.DoKeywordsContain("terrain"), EnteringTerrainSequenceResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.PlayCard }, TriggerTiming.After));
                
                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // "At the start of the villain turn, Momentum regains 1 HP."
                    base.AddSideTrigger(base.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, AdvancedStartOfTurnResponse, TriggerType.GainHP));
                }
            }
            else
            {
                // Back side:
                // "At the start of the villain turn, flip {Momentum} twice."
                base.AddSideTrigger(base.AddStartOfTurnTrigger((TurnTaker tt) => tt == this.TurnTaker, DeadEndJobStartOfTurnResponse, TriggerType.FlipCard));

                // "As long as {Momentum} has more than..."
                // "... {H * 3} HP, damage dealt by {Breakaway} is irreducible."
                // "... {H} HP, increase damage dealt by {Breakaway} by 1."
                base.AddSideTrigger(base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == this.Card, DeadEndJobDealingDamageResponse, new TriggerType[] { TriggerType.MakeDamageIrreducible, TriggerType.IncreaseDamage }, TriggerTiming.Before));

                // "As long as {Momentum} has more than..."
                // "... {H * 2} times 2 HP, reduce damage dealt to {Breakaway} by 1."
                base.AddSideTrigger(base.AddReduceDamageTrigger((Card c) => c == this.Card && base.TurnTaker.FindCard("MomentumCharacter").HitPoints > Game.H * 2, 1));

                // "Whenever {Momentum}'s current HP becomes equal to its maximum HP, {Breakaway} deals each target 1 melee damage."
                base.AddSideTrigger(base.AddTrigger<GainHPAction>((GainHPAction gha) => gha.HpGainer == this.TurnTaker.FindCard("MomentumCharacter"), MomentumHPCheckResponse, TriggerType.DealDamage, TriggerTiming.After));
                base.AddSideTrigger(base.AddTrigger<SetHPAction>((SetHPAction sha) => sha.HpGainer == this.TurnTaker.FindCard("MomentumCharacter"), MomentumHPCheckResponse2, TriggerType.DealDamage, TriggerTiming.After));

                // "The first time a hero card enters play each turn, {Breakaway} deals that hero and the other hero target with the highest HP 0 melee damage each."
                base.AddSideTrigger(base.AddTrigger<PlayCardAction>((PlayCardAction pca) => !HasBeenSetToTrueThisTurn(heroCardEntered) && IsHero(pca.CardToPlay), DeadEndJobHeroPlayResponse, TriggerType.DealDamage, TriggerTiming.After));

                if (base.IsGameAdvanced)
                {
                    // Back side, Advanced:
                    // "The first time {Breakaway} would deal damage each hero turn, increase that damage by 1."
                    base.AddSideTrigger(base.AddTrigger<DealDamageAction>((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(dealtDamageHero) && dda.DamageSource != null && dda.DamageSource.IsCard && dda.DamageSource.Card == this.Card && IsHero(base.GameController.ActiveTurnTaker), DeadEndJobFirstDamageDealtResponse, TriggerType.IncreaseDamage, TriggerTiming.After, requireActionSuccess: false));
                }
            }

            // If Breakaway is destroyed, the heroes win.
            base.AddDefeatedIfDestroyedTriggers();
            base.AddDefeatedIfMovedOutOfGameTriggers();
            // If Breakaway's HP is set to 0 or less, destroy him.
            base.AddTrigger<SetHPAction>((SetHPAction sha) => sha.HpGainer == this.Card && sha.Amount <= 0, (SetHPAction sha) => base.GameController.GameOver(EndingResult.VillainDestroyedVictory, "The heroes catch up to Breakaway and recover the loot!"), TriggerType.GameOver, TriggerTiming.After, requireActionSuccess: true, isActionOptional: false, orderMatters: false, priority: TriggerPriority.High);
        }

        private string EscapeMessage()
        {
            return this.Card.Title + " made it to " + this.Card.MaximumHitPoints.ToString() + " HP! He escapes with the loot and the heroes lose!";
        }

        public IEnumerator BreakawayHPCheckResponse(GainHPAction gha)
        {
            // "When {Breakaway}'s current HP is equal to his maximum HP, he escapes with the loot! [b]GAME OVER.[/b]"
            if (this.Card.HitPoints != this.Card.MaximumHitPoints)
            {
                yield break;
            }
            else
            {
                // Reached maximum HP? Game over!
                IEnumerator escapeCoroutine = base.GameController.GameOver(EndingResult.AlternateDefeat, EscapeMessage(), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(escapeCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(escapeCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator MomentumFirstDamageResponse(DealDamageAction dda)
        {
            // "The first time {Momentum} is dealt damage each turn, if that damage reduces its HP to 0 or less, remove 2 HP from {Breakaway}."
            Card momentum = base.TurnTaker.FindCard("MomentumCharacter");
            base.SetCardPropertyToTrueIfRealAction(momentumTakenDamage);
            if (momentum.HitPoints <= 0)
            {
                IEnumerator loseHPCoroutine = base.GameController.SetHP(this.Card, this.Card.HitPoints.Value - 2, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(loseHPCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(loseHPCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator MomentumHitByEnvironmentResponse(DealDamageAction dda)
        {
            // Keep track of which environment cards dealt damage to Momentum this turn, so that UnflippedStartEachTurnResponse can make it immune to them
            DamageSource source = dda.DamageSource;
            if (source.IsCard)
            {
                Card sourceCard = source.Card;
                if (!hitMomentumLastTurn.Contains(sourceCard))
                {
                    hitMomentumLastTurn.Add(sourceCard);
                }
            }
            yield break;
        }

        public IEnumerator CriminalCourierStartEachTurnResponse(PhaseChangeAction pca)
        {
            // "At the start of each turn, {Momentum} becomes immune to damage dealt by each environment card that dealt damage to it during the previous turn."
            // For each card in hitMomentumLastTurn, if it's still in play...
            foreach(Card c in hitMomentumLastTurn)
            {
                if (c.IsInPlayAndHasGameText)
                {
                    // make Momentum immune to its damage until it leaves play.
                    ImmuneToDamageStatusEffect status = new ImmuneToDamageStatusEffect
                    {
                        TargetCriteria = { IsSpecificCard = base.TurnTaker.FindCard("MomentumCharacter") },
                        SourceCriteria = { IsSpecificCard = c }
                    };
                    status.UntilCardLeavesPlay(c);

                    IEnumerator statusCoroutine = base.GameController.AddStatusEffect(status, true, GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(statusCoroutine);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(statusCoroutine);
                    }
                }
            }
            // Then, clear hitMomentumLastTurn for the new turn
            hitMomentumLastTurn = new List<Card>();
            yield break;
        }

        public IEnumerator UnusualMoveResponse(MoveCardAction mca)
        {
            // "Whenever a villain card would go anywhere except the villain trash, deck, or play area, first reveal that card."
            // "If {TheClient} is revealed this way, flip {Breakaway}."
            if (mca.CardToMove != null)
            {
                Card revealedCard = mca.CardToMove;
                IEnumerator clientMessageCoroutine = DoNothing();
                IEnumerator breakawayMessageCoroutine = DoNothing();
                IEnumerator resultCoroutine = DoNothing();
                if (revealedCard == this.TurnTaker.FindCard("TheClient"))
                {
                    // Show the card to the players (with a note on what happened to The Client in game terms)
                    string clientFate = this.Card.Title + " can't find " + revealedCard.Title + " anywhere!";
                    if (IsHero(mca.CardSource.Card))
                    {
                        // One of the good guys scared the Client away or captured them
                        TurnTaker responsibleHero = mca.CardSource.TurnTakerController.TurnTaker;
                        if (!mca.Destination.IsInGame)
                        {
                            clientFate = responsibleHero.NameRespectingVariant + " scared " + revealedCard.Title + " away!";
                        }
                        else if (mca.Destination.IsHeroPlayAreaRecursive || mca.Destination.IsUnderCard)
                        {
                            clientFate = responsibleHero.NameRespectingVariant + " captured " + revealedCard.Title + "!";
                        }
                        clientMessageCoroutine = base.GameController.SendMessageAction(clientFate, Priority.High, cardSource: mca.CardSource, showCardSource: true);
                    }
                    else if (mca.CardSource.Card.IsEnvironment)
                    {
                        // The environment is too hostile
                        TurnTaker responsiblePlace = mca.CardSource.TurnTakerController.TurnTaker;
                        if (!mca.Destination.IsInGame)
                        {
                            clientFate = revealedCard.Title + " is nowhere to be found in " + responsiblePlace.Name + "!";
                        }
                        else if (mca.Destination.IsEnvironment || mca.Destination.IsUnderCard)
                        {
                            clientFate = revealedCard.Title + " is lost somewhere in " + responsiblePlace.Name + "!";
                        }
                        clientMessageCoroutine = base.GameController.SendMessageAction(clientFate, Priority.High, cardSource: mca.CardSource, showCardSource: true);
                    }
                    else
                    {
                        // The Client booked it because the heroes are too close
                        // TheClientCardController notifies the players before removing the card, so no special message is needed here
                    }
                    string breakawayReaction = this.Card.Title + " is still on the loose, but his heist is ruined! Frustrated, he turns on the heroes...";
                    breakawayMessageCoroutine = base.GameController.SendMessageAction(breakawayReaction, Priority.High, cardSource: GetCardSource());
                    // Flip Breakaway
                    resultCoroutine = base.GameController.FlipCard(this, cardSource: GetCardSource());
                }
                else
                {
                    // Show the card to the players, but then go on with the move as normal
                    clientMessageCoroutine = base.GameController.SendMessageAction(this.Card.Title + " reveals " + revealedCard.Title + ".", Priority.High, cardSource: GetCardSource(), associatedCards: revealedCard.ToEnumerable(), showCardSource: true);
                    resultCoroutine = DoNothing();
                }

                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(clientMessageCoroutine);
                    yield return this.GameController.StartCoroutine(breakawayMessageCoroutine);
                    yield return this.GameController.StartCoroutine(resultCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(clientMessageCoroutine);
                    this.GameController.ExhaustCoroutine(breakawayMessageCoroutine);
                    this.GameController.ExhaustCoroutine(resultCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator EnteringTerrainSequenceResponse(PlayCardAction pca)
        {
            // "Whenever a Terrain card enters play, destroy all other Terrain cards and all environment cards..."
            Card newTerrain = pca.CardToPlay;
            IEnumerator destroyCoroutine = base.GameController.DestroyCards(this.DecisionMaker, new LinqCardCriteria((Card c) => (c.DoKeywordsContain("terrain") || c.IsEnvironment) && c != newTerrain), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(destroyCoroutine);
            }

            // "... then play the top card of the environment deck."
            IEnumerator playEnvironmentCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(pca);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(playEnvironmentCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(playEnvironmentCoroutine);
            }
            yield break;
        }

        public IEnumerator AdvancedStartOfTurnResponse(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, Momentum regains 1 HP."
            IEnumerator hpGainCoroutine = base.GameController.GainHP(base.TurnTaker.FindCard("MomentumCharacter"), 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(hpGainCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(hpGainCoroutine);
            }
            yield break;
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            RemoveSideTriggers();

            if (base.Card.IsFlipped)
            {
                // "When {Breakaway} flips to this side..."
                // "... remove {TheClient} from the game."
                Card theClient = base.TurnTaker.FindCard("The Client");
                IEnumerator removeCoroutine = base.GameController.MoveCard(this.DecisionMaker, theClient, base.TurnTaker.OutOfGame, showMessage: true, responsibleTurnTaker: this.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(removeCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(removeCoroutine);
                }
                // "... flip {Momentum} to its "Under Pressure" side."
                Card momentum = base.TurnTaker.FindCard("MomentumCharacter");
                IEnumerator flipCoroutine = DoNothing();
                if (momentum.Definition.Body.FirstOrDefault() != "Under Pressure")
                {
                    // Flip Momentum
                    flipCoroutine = base.GameController.FlipCard(FindCardController(momentum), cardSource: GetCardSource());
                }
                else
                {
                    // Don't need to flip, just inform the players the condition is already true
                    flipCoroutine = base.GameController.SendMessageAction(momentum.Title + " is already on its Under Pressure side.", Priority.High, cardSource: GetCardSource(), associatedCards: new Card[] { momentum }, showCardSource: true);
                }
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(flipCoroutine);
                }
                // "... destroy {H} non-character hero cards."
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(this.DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && !c.IsCharacter, "non-character hero"), Game.H, responsibleCard: this.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(destroyCoroutine);
                }
                // "... {Breakaway} regains {H * 5} HP."
                IEnumerator hpGainCoroutine = base.GameController.GainHP(this.Card, Game.H * 5, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(hpGainCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(hpGainCoroutine);
                }
            }

            AddSideTriggers();

            yield break;
        }

        public IEnumerator DeadEndJobStartOfTurnResponse(PhaseChangeAction pca)
        {
            // "At the start of the villain turn, flip {Momentum} twice."
            Card momentum = base.TurnTaker.FindCard("MomentumCharacter");
            for (int i = 0; i < 2; i++)
            {
                IEnumerator flipCoroutine = base.GameController.FlipCard(FindCardController(momentum), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DeadEndJobDealingDamageResponse(DealDamageAction dda)
        {
            // "As long as {Momentum} has more than..."
            Card momentum = base.TurnTaker.FindCard("MomentumCharacter");
            int momentumHP = momentum.HitPoints.Value;
            // "... {H * 3} HP, damage dealt by {Breakaway} is irreducible."
            if (momentumHP > Game.H * 3)
            {
                IEnumerator irreducibleCoroutine = base.GameController.MakeDamageIrreducible(dda, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(irreducibleCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(irreducibleCoroutine);
                }
            }
            // "... {H} HP, increase damage dealt by {Breakaway} by 1."
            if (momentumHP > Game.H)
            {
                IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(increaseCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(increaseCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator MomentumHPCheckResponse(GainHPAction gha)
        {
            // "Whenever {Momentum}'s current HP becomes equal to its maximum HP, {Breakaway} deals each target 1 melee damage."
            Card momentum = base.TurnTaker.FindCard("MomentumCharacter");
            if (momentum.HitPoints != momentum.MaximumHitPoints)
            {
                yield break;
            }
            else
            {
                IEnumerator damageCoroutine = DealDamage(base.Card, (Card c) => true, 1, DamageType.Melee);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator MomentumHPCheckResponse2(SetHPAction sha)
        {
            // "Whenever {Momentum}'s current HP becomes equal to its maximum HP, {Breakaway} deals each target 1 melee damage."
            Card momentum = base.TurnTaker.FindCard("MomentumCharacter");
            if (momentum.HitPoints != momentum.MaximumHitPoints)
            {
                yield break;
            }
            else
            {
                IEnumerator damageCoroutine = DealDamage(base.Card, (Card c) => true, 1, DamageType.Melee);
                if (base.UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DeadEndJobHeroPlayResponse(PlayCardAction pca)
        {
            // "The first time a hero card enters play each turn, {Breakaway} deals that hero and the other hero target with the highest HP 0 melee damage each."
            base.SetCardPropertyToTrueIfRealAction(heroCardEntered);
            // Find the hero associated with the card entering play
            List<Card> heroTargetsChosen = new List<Card>();
            TurnTaker cardOwner = pca.CardToPlay.Owner;
            IEnumerator ownerChoiceCoroutine = base.FindCharacterCardToTakeDamage(cardOwner, heroTargetsChosen, this.Card, 0, DamageType.Melee);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(ownerChoiceCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(ownerChoiceCoroutine);
            }
            string chosenNames = "";
            if (heroTargetsChosen.Any())
            {
                chosenNames = heroTargetsChosen.First().Title;
                if (heroTargetsChosen.Count() > 1)
                {
                    chosenNames += ", " + heroTargetsChosen.ElementAt(1).Title;
                }
            }
            //Log.Debug("heroTargetsChosen after ownerChoiceCoroutine: " + chosenNames);
            // Find the hero target with the highest HP other than that one
            Card firstTarget = heroTargetsChosen.First();
            List<Card> highestHeroTargets = new List<Card>();
            IEnumerator secondChoiceCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsHeroTarget(c) && c != firstTarget, highestHeroTargets, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(secondChoiceCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(secondChoiceCoroutine);
            }
            heroTargetsChosen = heroTargetsChosen.Concat(highestHeroTargets).ToList();
            chosenNames = "";
            if (heroTargetsChosen.Any())
            {
                chosenNames = heroTargetsChosen.First().Title;
                if (heroTargetsChosen.Count() > 1)
                {
                    chosenNames += ", " + heroTargetsChosen.ElementAt(1).Title;
                }
            }
            //Log.Debug("heroTargetsChosen after secondChoiceCoroutine: " + chosenNames);
            // Deal damage
            IEnumerator damageCoroutine = base.GameController.DealDamage(this.DecisionMaker, this.Card, (Card c) => heroTargetsChosen.Contains(c), 0, DamageType.Melee, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }

        public IEnumerator DeadEndJobFirstDamageDealtResponse(DealDamageAction dda)
        {
            // "The first time {Breakaway} would deal damage each hero turn, increase that damage by 1."
            base.SetCardPropertyToTrueIfRealAction(dealtDamageHero);
            IEnumerator increaseCoroutine = base.GameController.IncreaseDamage(dda, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(increaseCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(increaseCoroutine);
            }
            yield break;
        }
    }
}
