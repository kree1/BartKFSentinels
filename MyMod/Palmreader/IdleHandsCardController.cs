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
            // "If that target was in a play area with a Relay card, {PalmreaderCharacter} deals another target in that play area 2 irreducible projectile damage."
            if (firstTargeting != null && firstTargeting.Count > 0)
            {
                Card firstTarget = firstTargeting.FirstOrDefault().SelectedCard;
                // Figure out where firstTarget was when it was dealt damage
                Location playArea = firstTarget.Location;
                DealDamageJournalEntry damage = Journal.DealDamageEntriesThisTurn().Where((DealDamageJournalEntry ddje) => ddje.TargetCard == firstTarget && ddje.SourceCard == base.CharacterCard && ddje.DamageType == DamageType.Melee && ddje.CardThatCausedDamageToOccur == base.Card).LastOrDefault();
                List<MoveCardJournalEntry> movesSince = Journal.MoveCardEntriesThisTurn().Where((MoveCardJournalEntry mcje) => mcje.Card == firstTarget && Journal.GetEntryIndex(mcje) > Journal.GetEntryIndex(damage)).ToList();
                if (movesSince.Any())
                {
                    MoveCardJournalEntry firstMove = movesSince.FirstOrDefault();
                    playArea = firstMove.FromLocation;
                }
                if (playArea.IsInPlay && NumRelaysAt(playArea) > 0)
                {
                    IEnumerator projectileCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Projectile, new int?(1), false, new int?(1), isIrreducible: true, additionalCriteria: (Card c) => c.Location == playArea && c != firstTarget, cardSource: GetCardSource());
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
