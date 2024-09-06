using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class StrongerThanATankCannonCardController : DreadnoughtUtilityCardController
    {
        public StrongerThanATankCannonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show whether Dreadnought has dealt damage to another hero target this turn
            SpecialStringMaker.ShowIfElseSpecialString(DealtDamageToOtherHeroTargetThisTurn, () => CharacterCard.Title + " has already dealt damage to another hero target this turn.", () => CharacterCard.Title + " has not dealt damage to any other hero targets this turn.");
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by {Dreadnought} by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsSameCard(CharacterCard), 1);
            // "At the end of your turn, {Dreadnought} deals 1 villain target 0 psychic damage. Then, if {Dreadnought} has dealt no damage to other hero targets this turn, she deals 1 other hero character target 0 psychic damage unless you put the bottom card of your trash on the bottom of your deck."
            // ...
        }

        public bool DealtDamageToOtherHeroTargetThisTurn()
        {
            return Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.SourceCard == CharacterCard && ddje.Amount > 0 && IsHeroTarget(ddje.TargetCard) && ddje.TargetCard != CharacterCard).Any();
        }

        public IEnumerator PsychicStressResponse(PhaseChangeAction pca)
        {
            // "... {Dreadnought} deals 1 villain target 0 psychic damage."
            IEnumerator villainCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 0, DamageType.Psychic, 1, false, 1, additionalCriteria: (Card c) => IsVillainTarget(c), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(villainCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(villainCoroutine);
            }
            // "Then, if {Dreadnought} has dealt no damage to other hero targets this turn, ..."
            if (!DealtDamageToOtherHeroTargetThisTurn())
            {
                // "... she deals 1 other hero character target 0 psychic damage unless you put the bottom card of your trash on the bottom of your deck."
                List<MoveCardAction> moved = new List<MoveCardAction>();
                // If there are any cards to move:
                if (TurnTaker.Trash.Cards.Any())
                {
                    // Player chooses whether to move cards, with preview of what will happen if they don't
                    DealDamageAction preview = new DealDamageAction(GameController, new DamageSource(GameController, CharacterCard), null, 0, DamageType.Psychic);
                    SelectionType tag = SelectionType.MoveCardOnBottomOfDeck;
                    YesNoDecision choice = new YesNoDecision(GameController, DecisionMaker, tag, gameAction: preview, cardSource: GetCardSource());
                    IEnumerator chooseCoroutine = GameController.MakeDecisionAction(choice);
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(chooseCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(chooseCoroutine);
                    }
                    // If they said yes, cards are moved
                    IEnumerable<Card> toMove = TurnTaker.Trash.Cards.Take(1);
                    IEnumerator moveCoroutine = GameController.MoveCards(TurnTakerController, toMove, TurnTaker.Deck, toBottom: true, responsibleTurnTaker: TurnTaker, storedResultsAction: moved, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
                // If not enough cards were moved, Dreadnought deals another hero character target 0 psychic damage
                IEnumerable<Card> wasMoved = (from MoveCardAction mca in moved where mca.WasCardMoved select mca.CardToMove).Distinct();
                if (wasMoved.Count() < 1)
                {
                    IEnumerator psychicCoroutine = GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(GameController, CharacterCard), 0, DamageType.Psychic, 1, false, 1, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.IsTarget && c != CharacterCard, cardSource: GetCardSource());
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(psychicCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(psychicCoroutine);
                    }
                }
            }
        }
    }
}
