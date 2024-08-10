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
    public class OwnershipUtilityCardController : CardController
    {
        public OwnershipUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string OwnershipIdentifier = "OwnershipCharacter";
        public readonly string MapCardIdentifier = "MapCharacter";
        public readonly string StatCardIdentifier = "StatCharacter";
        public readonly string WeightPoolIdentifier = "StatCardWeightPool";
        public readonly string SunSunIdentifier = "SunSun";
        public readonly string StatKeyword = "stat";
        public readonly string SunKeyword = "sun";
        public readonly string ModificationKeyword = "modification";

        public int DamageDealtToNonHeroByHeroThisTurn()
        {
            return (from ddje in base.Journal.DealDamageEntriesThisTurn() where !IsHeroTarget(ddje.TargetCard) && ddje.SourceCard != null && IsHeroTarget(ddje.SourceCard) select ddje).Sum((DealDamageJournalEntry ddje) => ddje.Amount);
        }

        public IEnumerator IncreaseNextDamageDealtByAHeroTarget(int amount)
        {
            IncreaseDamageStatusEffect buff = new IncreaseDamageStatusEffect(amount);
            buff.SourceCriteria.IsHero = true;
            buff.SourceCriteria.IsTarget = true;
            buff.NumberOfUses = 1;
            IEnumerator statusCoroutine = AddStatusEffect(buff);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
        }
    }
}
