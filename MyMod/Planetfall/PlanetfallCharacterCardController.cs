using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    internal class PlanetfallCharacterCardController : VillainCharacterCardController
    {
        public PlanetfallCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Both sides: show number of Chip cards in the villain deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, ChipKeyword), "Chip"));
            // Front side: show number of Mega cards in the villain deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MegaKeyword), "Mega")).Condition = () => !Card.IsFlipped;
            // Front side: show hero character with lowest HP
            SpecialStringMaker.ShowHeroCharacterCardWithLowestHP().Condition = () => !Card.IsFlipped;
            // Back side: show hero target with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP().Condition = () => Card.IsFlipped;
            // Back side: show number of Micro cards in the villain deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => GameController.DoesCardContainKeyword(c, MicroKeyword), "Micro")).Condition = () => Card.IsFlipped;
            // Back side: show H-1 hero targets with highest HP
            SpecialStringMaker.ShowHeroTargetWithHighestHP(numberOfTargets: H - 1).Condition = () => Card.IsFlipped;
        }

        public readonly string ChipKeyword = "chip";
        public readonly string MegaKeyword = "mega";
        public readonly string MicroKeyword = "micro";

        public bool IsChipEnteringFromVillainDeck(CardEntersPlayAction cepa)
        {
            if (!GameController.DoesCardContainKeyword(cepa.CardEnteringPlay, ChipKeyword))
            {
                return false;
            }
            bool result = false;
            MoveCardJournalEntry entry = (from mc in Journal.MoveCardEntriesThisTurn() where mc.Card == cepa.CardEnteringPlay && mc.ToLocation.IsInPlay select mc).LastOrDefault();
            if (entry != null && entry.FromLocation.IsVillain)
            {
                if (entry.FromLocation.IsDeck)
                {
                    result = true;
                }
                else if (entry.FromLocation.IsRevealed)
                {
                    MoveCardJournalEntry previous = (from mc in Journal.MoveCardEntriesThisTurn() where mc.Card == cepa.CardEnteringPlay && mc.ToLocation.IsRevealed select mc).LastOrDefault();
                    if (previous != null && previous.FromLocation.IsDeck && previous.FromLocation.IsVillain)
                    {
                        result = true;
                    }
                }
            }
            return result;
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Whenever {Planetfall} would be dealt 3 or more damage, reduce that damage by 2."
                AddSideTrigger(AddReduceDamageTrigger((DealDamageAction dda) => dda.Amount >= 3, 2, null, (Card c) => c == CharacterCard));
                // "When a Chip enters play from the villain deck, {Planetfall} regains 2 HP, then play the top card of the villain deck."
                AddSideTrigger(AddTrigger<CardEntersPlayAction>(IsChipEnteringFromVillainDeck, HealPlayResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.PlayCard }, TriggerTiming.After));
                // "When a villain Mega card enters play, flip {Planetfall}."
                AddSideTrigger(AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsVillain && GameController.DoesCardContainKeyword(cepa.CardEnteringPlay, MegaKeyword), (CardEntersPlayAction cepa) => FlipThisCharacterCardResponse(cepa), TriggerType.FlipCard, TriggerTiming.After));
                // "At the end of the villain turn, {Planetfall} deals the hero character with the lowest HP {H - 2} toxic damage."
                AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(TurnTaker, Card, (Card c) => IsHeroCharacterCard(c), TargetType.LowestHP, H - 2, DamageType.Toxic));
            }
            else
            {
                // Back side:
                // "Whenever {Planetfall} would be dealt 2 or fewer damage, reduce that damage by 1."
                AddSideTrigger(AddReduceDamageTrigger((DealDamageAction dda) => dda.Amount <= 2, 1, null, (Card c) => c == CharacterCard));
                // "When a Chip enters play from the villain deck, {Planetfall} deals the hero target with the highest HP 2 projectile damage, then play the top card of the villain deck."
                AddSideTrigger(AddTrigger<CardEntersPlayAction>(IsChipEnteringFromVillainDeck, ShootPlayResponse, new TriggerType[] { TriggerType.DealDamage, TriggerType.PlayCard }, TriggerTiming.After));
                // "When a villain Micro card enters play, flip {Planetfall}."
                AddSideTrigger(AddTrigger((CardEntersPlayAction cepa) => cepa.CardEnteringPlay.IsVillain && GameController.DoesCardContainKeyword(cepa.CardEnteringPlay, MicroKeyword), (CardEntersPlayAction cepa) => FlipThisCharacterCardResponse(cepa), TriggerType.FlipCard, TriggerTiming.After));
                // "At the end of the villain turn, {Planetfall} deals the {H - 1} hero targets with the highest HP 2 sonic damage each."
                AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(TurnTaker, Card, (Card c) => IsHeroTarget(c), TargetType.HighestHP, 2, DamageType.Sonic, numberOfTargets: H - 1));
            }
            AddDefeatedIfDestroyedTriggers();
            AddDefeatedIfMovedOutOfGameTriggers();
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator inheritedCoroutine = base.AfterFlipCardImmediateResponse();
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(inheritedCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(inheritedCoroutine);
            }
            if (IsGameAdvanced)
            {
                // Advanced: "When {Planetfall} flips to this side, ..."
                IEnumerator respondCoroutine = DoNothing();
                if (Card.IsFlipped)
                {
                    // "... she deals each non-villain target 1 melee damage."
                    respondCoroutine = DealDamage(CharacterCard, (Card c) => !IsVillainTarget(c), 1, DamageType.Melee);
                }
                else
                {
                    // "... {H - 2} players each discard 1 card."
                    respondCoroutine = GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsPlayer && !tt.IsIncapacitatedOrOutOfGame && (tt as HeroTurnTaker).HasCardsInHand, "heroes with cards in hand"), SelectionType.DiscardCard, (TurnTaker tt) => GameController.SelectAndDiscardCards(GameController.FindHeroTurnTakerController((HeroTurnTaker)tt), 1, false, 0, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource()), H - 2, false, H - 2, cardSource: GetCardSource());
                }
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(respondCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(respondCoroutine);
                }
            }
        }

        public IEnumerator HealPlayResponse(CardEntersPlayAction cepa)
        {
            // "... {Planetfall} regains 2 HP, ..."
            IEnumerator healCoroutine = GameController.GainHP(CharacterCard, 2, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healCoroutine);
            }
            // "... then play the top card of the villain deck."
            IEnumerator playCoroutine = GameController.PlayTopCard(DecisionMaker, TurnTakerController, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        public IEnumerator ShootPlayResponse(CardEntersPlayAction cepa)
        {
            // "... {Planetfall} deals the hero target with the highest HP 2 projectile damage, ..."
            IEnumerator projectileCoroutine = DealDamageToHighestHP(Card, 1, (Card c) => IsHeroTarget(c), (Card c) => 2, DamageType.Projectile);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(projectileCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(projectileCoroutine);
            }
            // "... then play the top card of the villain deck."
            IEnumerator playCoroutine = GameController.PlayTopCard(DecisionMaker, TurnTakerController, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
        }
    }
}
