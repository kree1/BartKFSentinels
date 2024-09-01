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
    public class BioInfiltrationCardController : CardController
    {
        public BioInfiltrationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero with highest HP
            SpecialStringMaker.ShowHeroCharacterCardWithHighestHP();
        }

        public override IEnumerator Play()
        {
            // "{Planetfall} deals the hero with the highest HP {H - 1} irreducible melee damage. Reduce damage dealt by that target by 1 until the start of the villain turn."
            IEnumerator meleeCoroutine = DealDamageToHighestHP(CharacterCard, 1, (Card c) => IsHeroCharacterCard(c), (Card c) => H - 1, DamageType.Melee, isIrreducible: true, addStatusEffect: (DealDamageAction dda) => ReduceDamageDealtByThatTargetUntilTheStartOfYourNextTurnResponse(dda, 1), selectTargetEvenIfCannotDealDamage: true);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(meleeCoroutine);
            }
        }
    }
}
