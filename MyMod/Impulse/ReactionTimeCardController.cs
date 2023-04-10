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
    public class ReactionTimeCardController : ImpulseUtilityCardController
    {
        public ReactionTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AllowFastCoroutinesDuringPretend = false;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When a villain One-Shot or a non-target environment card would enter play, you may discard it instead. If you do, destroy this card and {ImpulseCharacter} deals himself 2 melee and 2 energy damage."
            base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction cepa) => (IsVillain(cepa.CardEnteringPlay) && cepa.CardEnteringPlay.IsOneShot) || (cepa.CardEnteringPlay.IsEnvironment && !cepa.CardEnteringPlay.IsTarget), EntersPlayResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.DiscardCard, TriggerType.DealDamage, TriggerType.DestroySelf }, TriggerTiming.Before, isActionOptional: true);
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, you may draw a card."
            IEnumerator drawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            yield break;
        }

        public IEnumerator EntersPlayResponse(CardEntersPlayAction cepa)
        {
            // "When a villain One-Shot or a non-target environment card would enter play, you may discard it instead."
            Card entering = cepa.CardEnteringPlay;
            List<YesNoCardDecision> result = new List<YesNoCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.DiscardCard, entering, storedResults: result, cardSource: GetCardSource());
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
                // Discard the card instead of putting it into play
                AddCardPropertyJournalEntry("CardBlocked", entering);
                IEnumerator cancelCoroutine = CancelAction(cepa);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                MoveCardDestination trash = FindCardController(entering).GetTrashDestination();
                IEnumerator discardCoroutine = base.GameController.MoveCard(base.TurnTakerController, entering, trash.Location, responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }

                // "If you do, destroy this card and {ImpulseCharacter} deals himself 2 melee and 2 energy damage."
                IEnumerator meleeCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, 2, DamageType.Melee, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(meleeCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(meleeCoroutine);
                }
                IEnumerator energyCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, 2, DamageType.Energy, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(energyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(energyCoroutine);
                }
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator SelfDamageSequence()
        {
            // "... and {ImpulseCharacter} deals himself 2 melee and 2 energy damage."
            IEnumerator meleeCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, 2, DamageType.Melee, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(meleeCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(meleeCoroutine);
            }
            IEnumerator energyCoroutine = base.GameController.DealDamageToSelf(base.HeroTurnTakerController, (Card c) => c == base.CharacterCard, 2, DamageType.Energy, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(energyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(energyCoroutine);
            }
            yield break;
        }
    }
}
