using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class SlipstreamArsenalCardController : ImpulseUtilityCardController
    {
        public SlipstreamArsenalCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a non-hero card is destroyed by a hero card, you may put it under this card."
            AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.CardSource != null && dca.CardToDestroy.CanBeDestroyed && dca.WasCardDestroyed && dca.CardSource.Card.Owner.IsHero && !dca.CardToDestroy.Card.Owner.IsHero && dca.PostDestroyDestinationCanBeChanged && (dca.DealDamageAction == null || dca.DealDamageAction.DamageSource.IsHero), MoveResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.ChangePostDestroyDestination }, TriggerTiming.After, isActionOptional: true);
            // "At the start of your turn, discard 3 cards from under this card. {ImpulseCharacter} deals 1 target 1 projectile damage for each card discarded this way."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardForDamageResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.DealDamage });
            // Implied: when this card leaves play, put all cards under it into their appropriate trashes
            AddBeforeLeavesPlayActions(EmptyResponse);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        public IEnumerator MoveResponse(DestroyCardAction dca)
        {
            // "Whenever a non-hero card is destroyed by a hero card, you may put it under this card."
            List<YesNoCardDecision> result = new List<YesNoCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.MoveCardToUnderCard, dca.CardToDestroy.Card, storedResults: result, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(result))
            {
                dca.SetPostDestroyDestination(base.Card.UnderLocation, decisionSources: result.CastEnumerable<YesNoCardDecision, IDecision>());
            }
            yield break;
        }

        public IEnumerator DiscardForDamageResponse(PhaseChangeAction pca)
        {
            // "At the start of your turn, discard 3 cards from under this card. {ImpulseCharacter} deals 1 target 1 projectile damage for each card discarded this way."
            for (int i = 0; i < 3; i++)
            {
                if (base.Card.UnderLocation.Cards.Count() > 0)
                {
                    // Choose a card to discard
                    List<SelectCardDecision> selected = new List<SelectCardDecision>();
                    IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.DiscardCard, new LinqCardCriteria((Card c) => c.Location == base.Card.UnderLocation || c.Location == base.Card.BelowLocation), selected, optional: false, allowAutoDecide: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(selectCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(selectCoroutine);
                    }
                    Card chosenCard = selected.FirstOrDefault().SelectedCard;
                    // Discard it
                    MoveCardDestination trash = FindCardController(chosenCard).GetTrashDestination();
                    IEnumerator discardCoroutine = base.GameController.MoveCard(base.TurnTakerController, chosenCard, trash.Location, showMessage: true, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(discardCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(discardCoroutine);
                    }
                    // Impulse deals 1 target 1 projectile damage
                    IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 1, DamageType.Projectile, 1, false, 1, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                }
            }
            yield break;
        }

        public IEnumerator EmptyResponse(GameAction ga)
        {
            // Implied: when this card leaves play, put all cards under it into their appropriate trashes
            while (base.Card.UnderLocation.Cards.Count() > 0)
            {
                Card next = base.Card.UnderLocation.TopCard;
                MoveCardDestination trash = FindCardController(next).GetTrashDestination();
                IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, next, trash.Location, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
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
