using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class ShatterSanctuaryCardController : PlanetfallUtilityCardController
    {
        public ShatterSanctuaryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Destroy 2 non-target environment cards."
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment && !c.IsTarget, "non-target environment"), 2, requiredDecisions: 2, responsibleCard: Card, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "{Planetfall} deals each Chip and each non-villain target X projectile damage, where X = 3 plus the number of non-target cards destroyed this turn."
            IEnumerator chipDamageCoroutine = DealDamage(CharacterCard, (Card c) => GameController.DoesCardContainKeyword(c, ChipKeyword), (Card c) => 3 + Game.Journal.DestroyCardEntriesThisTurn().Where((DestroyCardJournalEntry dcje) => !dcje.WasTargetWhenDestroyed).Count(), DamageType.Projectile);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(chipDamageCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(chipDamageCoroutine);
            }
            IEnumerator nonVillainDamageCoroutine = DealDamage(CharacterCard, (Card c) => !IsVillainTarget(c), (Card c) => 3 + Game.Journal.DestroyCardEntriesThisTurn().Where((DestroyCardJournalEntry dcje) => !dcje.WasTargetWhenDestroyed).Count(), DamageType.Projectile);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(nonVillainDamageCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(nonVillainDamageCoroutine);
            }
        }
    }
}
