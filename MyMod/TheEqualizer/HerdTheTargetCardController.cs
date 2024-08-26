using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class HerdTheTargetCardController : EqualizerUtilityCardController
    {
        public HerdTheTargetCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show non-Marked hero target with highest HP
            SpecialStringMaker.ShowHighestHP(cardCriteria: new LinqCardCriteria((Card c) => IsHeroTarget(c) && !IsMarked(c), "non-Marked hero", singular: "target", plural: "targets"));
        }

        public override IEnumerator Play()
        {
            // "{TheEqualizer} deals the non-[b][i]Marked[/i][/b] hero target with the highest HP {H + 1} projectile damage."
            IEnumerator projectileCoroutine = DealDamageToHighestHP(CharacterCard, 1, (Card c) => IsHeroTarget(c) && !IsMarked(c), (Card c) => H + 1, DamageType.Projectile);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(projectileCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(projectileCoroutine);
            }
            // "Destroy {H} hero Ongoing cards."
            IEnumerator destroyCoroutine = GameController.SelectAndDestroyCards(DecisionMaker, new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c), "hero Ongoing"), H, responsibleCard: Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "The next damage dealt to the [b][i]Marked[/i][/b] target is irreducible."
            Card marked = MarkedTarget(GetCardSource());
            MakeDamageIrreducibleStatusEffect exposed = new MakeDamageIrreducibleStatusEffect();
            exposed.TargetCriteria.IsSpecificCard = marked;
            exposed.NumberOfUses = 1;
            exposed.UntilCardLeavesPlay(marked);
            IEnumerator statusCoroutine = AddStatusEffect(exposed);
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
