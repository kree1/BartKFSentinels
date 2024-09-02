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
    public class KineticEnclosureCardController : ChipCardController
    {
        public KineticEnclosureCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero character with highest HP
            SpecialStringMaker.ShowHeroCharacterCardWithHighestHP();
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this next to the hero with the highest HP."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = GameController.FindTargetWithHighestHitPoints(1, (Card c) => IsHeroCharacterCard(c), results, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            Card highest = results.FirstOrDefault();
            if (highest != null)
            {
                storedResults?.Add(new MoveCardDestination(highest.NextToLocation));
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever that hero would deal 3 or more damage, prevent that damage and this card regains 1 HP."
            AddPreventDamageTrigger((DealDamageAction dda) => dda.Amount >= 3 && dda.DamageSource != null && dda.DamageSource.IsSameCard(GetCardThisCardIsNextTo()), (DealDamageAction dda) => GameController.GainHP(Card, 1, cardSource: GetCardSource()), new TriggerType[] { TriggerType.GainHP }, isPreventEffect: true);
            // "When this card is destroyed, put it on top of the villain deck."
            AddWhenDestroyedTrigger(PutOnDeckResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.ChangePostDestroyDestination }, (DestroyCardAction dca) => dca.PostDestroyDestinationCanBeChanged);
        }

        public IEnumerator PutOnDeckResponse(DestroyCardAction dca)
        {
            // "... put it on top of the villain deck."
            Log.Debug("KineticEnclosureCardController.PutOnDeckResponse activated");
            if (dca.PostDestroyDestinationCanBeChanged)
            {
                Log.Debug("KineticEnclosureCardController.PutOnDeckResponse: PostDestroyDestinationCanBeChanged returned true");
                AddInhibitorException((GameAction ga) => ga is MessageAction || ga is MoveCardAction);
                IEnumerator announceCoroutine = GameController.SendMessageAction(Card.Title + " moves itself to the top of the villain deck.", Priority.Medium, GetCardSource(), showCardSource: true);
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(announceCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(announceCoroutine);
                }
                dca.SetPostDestroyDestination(TurnTaker.Deck);
                dca.PostDestroyDestinationCanBeChanged = false;
                RemoveInhibitorException();
            }
            yield break;
        }
    }
}
