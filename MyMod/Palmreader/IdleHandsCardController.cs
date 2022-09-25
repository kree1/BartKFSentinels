using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Palmreader
{
    public class IdleHandsCardController : PalmreaderUtilityCardController
    {
        public IdleHandsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => IsRelay(c) && c.IsInPlayAndHasGameText), specifyPlayAreas: true).Condition = () => NumRelaysInPlay() > 0;
            SpecialStringMaker.ShowSpecialString(() => "There are no Relay cards in play.").Condition = () => NumRelaysInPlay() <= 0;
        }

        public override IEnumerator Play()
        {
            // "{PalmreaderCharacter} deals 1 target 2 melee damage."
            List<SelectCardDecision> firstTargeting = new List<SelectCardDecision>();
            IEnumerator meleeCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Melee, new int?(1), false, new int?(1), storedResultsDecisions: firstTargeting, selectTargetsEvenIfCannotDealDamage: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            // "If that target was in a play area with a Relay card, {PalmreaderCharacter} deals another target in that play area 2 irreducible psychic damage."
            if (firstTargeting != null && firstTargeting.Count > 0)
            {
                Card firstTarget = firstTargeting.FirstOrDefault().SelectedCard;
                Log.Debug("IdleHandsCardController.Play: firstTarget: " + firstTarget.ToString());
                // Figure out where firstTarget was when it was dealt damage
                Location playArea = firstTarget.Location;
                Log.Debug("IdleHandsCardController.Play: firstTarget's current location: " + playArea.GetFriendlyName());
                DealDamageJournalEntry damage = Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.TargetCard == firstTarget && ddje.SourceCard == base.CharacterCard && ddje.DamageType == DamageType.Melee && ddje.CardThatCausedDamageToOccur == base.Card).LastOrDefault();
                Log.Debug("IdleHandsCardController.Play: compiling list of MoveCardJournalEntries since damage was dealt...");
                List<MoveCardJournalEntry> movesSince = Journal.MoveCardEntriesThisTurn().Where((MoveCardJournalEntry mcje) => mcje.Card == firstTarget && mcje.FromLocation.IsInPlay && Journal.GetEntryIndex(mcje) > Journal.GetEntryIndex(damage)).ToList();
                if (movesSince.Any())
                {
                    Log.Debug("IdleHandsCardController.Play: checking movesSince...");
                    foreach(MoveCardJournalEntry move in movesSince)
                    {
                        Log.Debug("IdleHandsCardController.Play: move: " + move.ToString());
                        Log.Debug("IdleHandsCardController.Play: move.FromLocation: " + move.FromLocation.GetFriendlyName());
                        Log.Debug("IdleHandsCardController.Play: move.ToLocation: " + move.ToLocation.GetFriendlyName());
                        Log.Debug("IdleHandsCardController.Play: move.FromLocation.HighestRecursiveLocation: " + move.FromLocation.HighestRecursiveLocation.GetFriendlyName());
                        Log.Debug("IdleHandsCardController.Play: move.FromLocation.HighestRecursiveLocation.IsInPlay: " + move.FromLocation.HighestRecursiveLocation.IsInPlay.ToString());
                        Log.Debug("IdleHandsCardController.Play: NumRelaysAt(move.FromLocation.HighestRecursiveLocation): " + NumRelaysAt(move.FromLocation.HighestRecursiveLocation).ToString());
                    }
                    MoveCardJournalEntry firstMove = movesSince.FirstOrDefault();
                    Log.Debug("IdleHandsCardController.Play: firstMove: " + firstMove.ToString());
                    playArea = firstMove.FromLocation.HighestRecursiveLocation;
                    // If the target was moved from a place that was next to a card or under a card, figure out where *that* card was when damage was dealt
                    if (firstMove.FromLocation.IsNextToCard || firstMove.FromLocation.IsUnderCard)
                    {
                        Log.Debug("IdleHandsCardController.Play: firstTarget was moved from " + firstMove.FromLocation.GetFriendlyName() + "...");
                        Card anchor = firstMove.FromLocation.OwnerCard;
                        Log.Debug("IdleHandsCardController.Play: firstMove.FromLocation.OwnerCard: " + anchor.Title);
                        List<MoveCardJournalEntry> anchorMovesSince = Journal.MoveCardEntriesThisTurn().Where((MoveCardJournalEntry mcje) => mcje.Card == anchor && mcje.FromLocation.IsInPlay && Journal.GetEntryIndex(mcje) > Journal.GetEntryIndex(damage)).ToList();
                        if (anchorMovesSince.Any())
                        {
                            foreach(MoveCardJournalEntry anchorMove in anchorMovesSince)
                            {
                                Log.Debug("IdleHandsCardController.Play: anchorMove: " + anchorMove.ToString());
                                Log.Debug("IdleHandsCardController.Play: anchorMove.FromLocation: " + anchorMove.FromLocation.GetFriendlyName());
                                Log.Debug("IdleHandsCardController.Play: anchorMove.ToLocation: " + anchorMove.ToLocation.GetFriendlyName());
                                Log.Debug("IdleHandsCardController.Play: anchorMove.FromLocation.HighestRecursiveLocation: " + anchorMove.FromLocation.HighestRecursiveLocation.GetFriendlyName());
                                Log.Debug("IdleHandsCardController.Play: anchorMove.FromLocation.HighestRecursiveLocation.IsInPlay: " + anchorMove.FromLocation.HighestRecursiveLocation.IsInPlay.ToString());
                                Log.Debug("IdleHandsCardController.Play: NumRelaysAt(anchorMove.FromLocation.HighestRecursiveLocation): " + NumRelaysAt(anchorMove.FromLocation.HighestRecursiveLocation).ToString());
                            }
                            MoveCardJournalEntry firstAnchorMove = anchorMovesSince.FirstOrDefault();
                            Log.Debug("IdleHandsCardController.Play: firstAnchorMove: " + firstAnchorMove.ToString());
                            playArea = firstAnchorMove.FromLocation.HighestRecursiveLocation;
                        }
                    }
                }
                if (playArea.IsInPlay && NumRelaysAt(playArea) > 0)
                {
                    IEnumerator projectileCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Psychic, new int?(1), false, new int?(1), isIrreducible: true, additionalCriteria: (Card c) => c.Location.HighestRecursiveLocation == playArea && c.IsInPlayAndHasGameText && c != firstTarget, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(projectileCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(projectileCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
