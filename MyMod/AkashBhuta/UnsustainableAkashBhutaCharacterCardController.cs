using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.AkashBhuta
{
    internal class UnsustainableAkashBhutaCharacterCardController : VillainCharacterCardController
    {
        public UnsustainableAkashBhutaCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Front side: show H+2 non-villain targets with lowest HP
            SpecialStringMaker.ShowNonVillainTargetWithLowestHP(numberOfTargets: H + 2).Condition = () => !Card.IsFlipped;
            // Back side: show non-villain target with highest HP
            SpecialStringMaker.ShowNonVillainTargetWithHighestHP(numberOfTargets: 1).Condition = () => Card.IsFlipped;
            // Back side: show H+2 non-villain targets with highest HP
            SpecialStringMaker.ShowNonVillainTargetWithHighestHP(numberOfTargets: H + 2).Condition = () => Card.IsFlipped;
            // Both sides: show whether Akash'bhuta has flipped this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(HasFlipped), () => Card.Title + " has already flipped this turn.", () => Card.Title + " has not yet flipped this turn.");
            // Both sides: if she hasn't flipped this turn, show whether a non-hero card has entered the trash this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(EnteredTrash1), () => "1 non-hero card has entered its trash this turn.", () => "No non-hero cards have entered their trash this turn.").Condition = () => !HasBeenSetToTrueThisTurn(HasFlipped);
        }

        public string EnteredTrash1 = "EnteredTrash1";
        public string EnteredTrash2 = "EnteredTrash2";
        public string HasFlipped = "HasFlipped";

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            // Both sides:
            // "The second time a non-hero card enters its trash each turn, flip {AkashBhuta}. {AkashBhuta} flips only once each turn."
            AddSideTrigger(AddTrigger((MoveCardAction mca) => !HasBeenSetToTrueThisTurn(EnteredTrash2) && !HasBeenSetToTrueThisTurn(HasFlipped) && mca.WasCardMoved && !IsHero(mca.CardToMove) && mca.Destination == mca.CardToMove.NativeTrash, SingleTrashResponse, TriggerType.FlipCard, TriggerTiming.After));
            AddSideTrigger(AddTrigger((BulkMoveCardsAction bmca) => !HasBeenSetToTrueThisTurn(EnteredTrash2) && !HasBeenSetToTrueThisTurn(HasFlipped) && bmca.CardsToMove.Where((Card c) => !IsHero(c) && bmca.Destination == c.NativeTrash).Count() == 1, SingleTrashResponse, TriggerType.FlipCard, TriggerTiming.After));
            AddSideTrigger(AddTrigger((BulkMoveCardsAction bmca) => !HasBeenSetToTrueThisTurn(EnteredTrash2) && !HasBeenSetToTrueThisTurn(HasFlipped) && bmca.CardsToMove.Where((Card c) => !IsHero(c) && bmca.Destination == c.NativeTrash).Count() > 1, MultiTrashResponse, TriggerType.FlipCard, TriggerTiming.After));
            if (!Card.IsFlipped)
            {
                // Front side:
                // "When a primeval limb card enters play, it deals itself 3 toxic damage and {AkashBhuta} regains 1 HP."
                AddSideTrigger(AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsPrimevalLimb, ChokeResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.GainHP }, TriggerTiming.After));
                // "When {AkashBhuta} would deal herself energy damage, she deals the {H + 2} non-villain targets with the lowest HP 1 toxic damage instead."
                AddSideTrigger(AddPreventDamageTrigger((DealDamageAction dda) => dda.Target == CharacterCard && dda.DamageSource != null && dda.DamageSource.Card == CharacterCard && dda.DamageType == DamageType.Energy, (DealDamageAction dda) => DealDamageToLowestHPEx(CharacterCard, 1, (Card c) => !IsVillainTarget(c), (Card c) => 1, DamageType.Toxic, numberOfTargets: () => H + 2), new TriggerType[] { TriggerType.GainHP }));
                // "At the end of the villain turn, {AkashBhuta} regains {H} HP."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == TurnTaker, (PhaseChangeAction pca) => GameController.GainHP(CharacterCard, H, cardSource: GetCardSource()), TriggerType.GainHP));
            }
            else
            {
                // Back side:
                // "When a primeval limb card enters play, it deals itself 4 fire damage and deals the non-villain target with the highest HP 1 fire damage."
                AddSideTrigger(AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsPrimevalLimb, BurnResponse, TriggerType.DealDamage, TriggerTiming.After));
                // "When {AkashBhuta} would deal herself energy damage, destroy 1 hero ongoing or equipment card instead."
                AddSideTrigger(AddPreventDamageTrigger((DealDamageAction dda) => dda.Target == CharacterCard && dda.DamageSource != null && dda.DamageSource.Card == CharacterCard && dda.DamageType == DamageType.Energy, (DealDamageAction dda) => GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => IsEquipment(c) || (IsHero(c) && IsOngoing(c)), "hero ongoing or equipment"), false, cardSource: GetCardSource()), new TriggerType[] { TriggerType.DestroyCard }));
                // "At the end of the villain turn, {AkashBhuta} deals the {H + 2} non-villain targets with the highest HP 2 fire damage each."
                AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(TurnTaker, CharacterCard, (Card c) => !IsVillainTarget(c), TargetType.HighestHP, 2, DamageType.Fire, numberOfTargets: H + 2));
            }
            AddDefeatedIfDestroyedTriggers();
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator baseCoroutine = base.AfterFlipCardImmediateResponse();
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(baseCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(baseCoroutine);
            }
            SetCardPropertyToTrueIfRealAction(HasFlipped);
            if (IsGameAdvanced)
            {
                if (!Card.IsFlipped)
                {
                    // Front side, Advanced:
                    // "When {AkashBhuta} flips to this side, play the top card of the environment deck."
                    IEnumerator playCoroutine = PlayTheTopCardOfTheEnvironmentDeckResponse(null);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
                else
                {
                    // Back side, Advanced:
                    // "When {Akashbhuta} flips to this side, destroy an environment card."
                    List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
                    IEnumerator destroyCoroutine = GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, storedResultsAction: destroyResults, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(destroyCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(destroyCoroutine);
                    }
                    // "If a card was destroyed this way, {H - 1} players discard a card."
                    if (DidDestroyCard(destroyResults))
                    {
                        IEnumerator discardCoroutine = GameController.SelectTurnTakersAndDoActionEx(new SelectTurnTakersDecision(GameController, DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && tt.ToHero().HasCardsInHand), SelectionType.DiscardCard, numberOfTurnTakers: H - 1, cardSource: GetCardSource()), (TurnTaker tt) => GameController.SelectAndDiscardCard(FindTurnTakerController(tt).ToHero(), cardSource: GetCardSource()), allowInitialYesNoDecision: false, cardSource: GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(discardCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(discardCoroutine);
                        }
                    }
                }
            }
        }

        public IEnumerator SingleTrashResponse(GameAction ga)
        {
            if (!HasBeenSetToTrueThisTurn(EnteredTrash1))
            {
                SetCardPropertyToTrueIfRealAction(EnteredTrash1);
            }
            else
            {
                SetCardPropertyToTrueIfRealAction(EnteredTrash2);
                // "... flip {AkashBhuta}."
                IEnumerator flipCoroutine = FlipThisCharacterCardResponse(ga);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
        }

        public IEnumerator MultiTrashResponse(GameAction ga)
        {
            if (!HasBeenSetToTrueThisTurn(EnteredTrash1))
            {
                SetCardPropertyToTrueIfRealAction(EnteredTrash1);
            }
            SetCardPropertyToTrueIfRealAction(EnteredTrash2);
            // "... flip {AkashBhuta}."
            IEnumerator flipCoroutine = FlipThisCharacterCardResponse(ga);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(flipCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(flipCoroutine);
            }
        }

        public IEnumerator ChokeResponse(CardEntersPlayAction cepa)
        {
            Card limb = cepa.CardEnteringPlay;
            // "... it deals itself 3 toxic damage..."
            IEnumerator toxicCoroutine = DealDamage(limb, limb, 3, DamageType.Toxic, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(toxicCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(toxicCoroutine);
            }
            // "... and {AkashBhuta} regains 1 HP."
            IEnumerator healCoroutine = GameController.GainHP(CharacterCard, 1, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healCoroutine);
            }
        }

        public IEnumerator BurnResponse(CardEntersPlayAction cepa)
        {
            Card limb = cepa.CardEnteringPlay;
            // "... it deals itself 4 fire damage..."
            IEnumerator selfCoroutine = DealDamage(limb, limb, 4, DamageType.Fire, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selfCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selfCoroutine);
            }
            // "... and deals the non-villain target with the highest HP 1 fire damage."
            IEnumerator otherCoroutine = DealDamageToHighestHP(limb, 1, (Card c) => !IsVillainTarget(c), (Card c) => 1, DamageType.Fire);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(otherCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(otherCoroutine);
            }
        }
    }
}
