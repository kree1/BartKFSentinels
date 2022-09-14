using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class RazorDartCardController : EvidenceStorageUtilityCardController
    {
        public RazorDartCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // If in play, show current play area
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c == base.Card, base.Card.Title, useCardsSuffix: false), specifyPlayAreas: true).Condition = () => base.Card.IsInPlayAndHasGameText;
            AddThisCardControllerToList(CardControllerListType.ModifiesKeywords);
        }

        public override void AddTriggers()
        {
            // "When a non-Device target in this play area deals damage to a target from another deck, increase that damage by 1 and change its type to projectile."
            base.AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && !dda.DamageSource.Card.DoKeywordsContain("device") && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageSource.Card.Owner != dda.Target.Owner, 1);
            base.AddChangeDamageTypeTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && !dda.DamageSource.Card.DoKeywordsContain("device") && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageSource.Card.Owner != dda.Target.Owner, DamageType.Projectile);
            // "Then, if the damaged target is still in play, move this card to the damaged target’s play area."
            base.AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && !dda.DamageSource.Card.DoKeywordsContain("device") && dda.DamageSource.Card.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation && dda.DamageSource.Card.Owner != dda.Target.Owner, ThrowResponse, TriggerType.MoveCard, TriggerTiming.After, isConditional: true, isActionOptional: false);
            base.AddTriggers();
        }

        public override bool AskIfCardContainsKeyword(Card card, string keyword, bool evenIfUnderCard = false, bool evenIfFaceDown = false)
        {
            // "As long as this card is in a hero play area, it has the keyword Equipment."
            if (keyword == "equipment" && card == base.Card && base.Card.Location.HighestRecursiveLocation.IsHero)
            {
                return true;
            }
            return base.AskIfCardContainsKeyword(card, keyword, evenIfUnderCard, evenIfFaceDown);
        }

        public override IEnumerable<string> AskForCardAdditionalKeywords(Card card)
        {
            // "As long as this card is in a hero play area, it has the keyword Equipment."
            if (card == base.Card && base.Card.Location.HighestRecursiveLocation.IsHero)
            {
                return new string[] { "equipment" };
            }
            return base.AskForCardAdditionalKeywords(card);
        }

        public IEnumerator ThrowResponse(DealDamageAction dda)
        {
            // "Then, if the damaged target is still in play, move this card to the damaged target’s play area."
            Card damaged = dda.Target;
            if (damaged.IsInPlay)
            {
                Location dest = damaged.Location.HighestRecursiveLocation;
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, dest, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, actionSource: dda, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
            yield break;
        }
    }
}
