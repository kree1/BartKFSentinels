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
    public class RemoteDetonationCardController : PlanetfallUtilityCardController
    {
        public RemoteDetonationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Chip cards in villain trash
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Trash, ChipCriteria());
        }

        public override IEnumerator Play()
        {
            // "{Planetfall} deals the X hero targets with the highest HP 2 projectile damage and 2 fire damage each, where X = the number of Chips in the villain trash."
            int x = GameController.FindCardsWhere(ChipInTrashCriteria(), visibleToCard: GetCardSource()).Count();
            List<DealDamageAction> instances = new List<DealDamageAction>();
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(GameController, CharacterCard), null, 2, DamageType.Projectile));
            instances.Add(new DealDamageAction(GetCardSource(), new DamageSource(GameController, CharacterCard), null, 2, DamageType.Fire));
            IEnumerator damageCoroutine = DealMultipleInstancesOfDamageToHighestLowestHP(instances, (Card c) => IsHeroTarget(c), HighestLowestHP.HighestHP, numberOfTargets: x);
            if (x <= 0)
            {
                damageCoroutine = GameController.SendMessageAction("There are no " + ChipKeyword + " cards in the villain trash, so " + CharacterCard.Title + " will not deal damage.", Priority.Medium, GetCardSource());
            }
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "If X was 1 or less, play the top card of the villain deck."
            if (x <= 1)
            {
                IEnumerator playCoroutine = GameController.PlayTopCard(DecisionMaker, TurnTakerController, responsibleTurnTaker: TurnTaker, showMessage: true, cardSource: GetCardSource());
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
}
