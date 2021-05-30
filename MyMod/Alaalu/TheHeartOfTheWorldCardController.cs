using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class TheHeartOfTheWorldCardController : CardController
    {
        public TheHeartOfTheWorldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => base.Card.Title + " has already shuffled a card into its deck this turn.", () => base.Card.Title + " has not yet shuffled a card into its deck this turn.");
        }

        protected const string OncePerTurn = "ShuffleOncePerTurn";

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "The first time a target would be put into a trash from play each turn, shuffle it into its deck instead."
            AddTrigger<MoveCardAction>((MoveCardAction mca) => !HasBeenSetToTrueThisTurn(OncePerTurn) && mca.CardToMove.IsTarget && mca.Origin.IsInPlay && mca.Destination.IsTrash, ShuffleInsteadResponse, TriggerType.ShuffleCardIntoDeck, TriggerTiming.Before);
            // "When a character card would be reduced to 0 or fewer HP, instead restore it to 5 HP and destroy this card."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.IsCharacter && dda.Target.HitPoints.HasValue && dda.Target.HitPoints.Value - dda.Amount <= 0 && dda.IsSuccessful, RestoreDestructResponse, TriggerType.WouldBeDealtDamage, TriggerTiming.Before);
            AddTrigger<DestroyCardAction>((DestroyCardAction dca) => dca.CardToDestroy.Card.IsCharacter && dca.CardToDestroy.Card.HitPoints.HasValue && dca.CardToDestroy.Card.HitPoints.Value <= 0, RestoreDestructResponse, TriggerType.CancelAction, TriggerTiming.Before);
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.IsCharacter && dda.Target.HitPoints.HasValue && dda.Target.HitPoints.Value <= 0, RestoreDestructResponse, TriggerType.GainHP, TriggerTiming.After);
            // "When this card leaves play, deal each Alaalid 3 psychic damage."
            AddBeforeLeavesPlayAction(DamageResponse, TriggerType.DealDamage);
        }

        public IEnumerator ShuffleInsteadResponse(MoveCardAction mca)
        {
            // "... shuffle it into its deck instead."
            base.SetCardPropertyToTrueIfRealAction(OncePerTurn);
            Card target = mca.CardToMove;
            IEnumerator cancelCoroutine = base.GameController.CancelAction(mca, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(cancelCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(cancelCoroutine);
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(target.Title + " is preserved by " + base.Card.Title + "...", Priority.Medium, GetCardSource(), associatedCards: new Card[] { target }, showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            IEnumerator shuffleCoroutine = base.GameController.ShuffleCardIntoLocation(DecisionMaker, target, target.Owner.Deck, false, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }
            yield break;
        }

        public IEnumerator RestoreDestructResponse(GameAction ga)
        {
            // "... instead restore it to 5 HP and destroy this card."
            Card characterInPeril = null;
            if (ga is DealDamageAction)
            {
                characterInPeril = (ga as DealDamageAction).Target;
            }
            else if (ga is SetHPAction)
            {
                characterInPeril = (ga as SetHPAction).HpGainer;
            }
            if (characterInPeril != null)
            {
                IEnumerator messageCoroutine = base.GameController.SendMessageAction(base.Card.Title + " prevents " + characterInPeril.Title + " from leaving the battle!", Priority.Medium, GetCardSource(), associatedCards: new Card[] { characterInPeril }, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(messageCoroutine);
                }
                IEnumerator cancelCoroutine = base.GameController.CancelAction(ga, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                int targetHP = Math.Min(5, characterInPeril.MaximumHitPoints.Value);
                IEnumerator restoreCoroutine = base.GameController.SetHP(characterInPeril, targetHP, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(restoreCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(restoreCoroutine);
                }
                IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, actionSource: ga, responsibleCard: base.Card, associatedCards: new List<Card>(new Card[] { characterInPeril }), cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destructCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destructCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DamageResponse(GameAction ga)
        {
            // "... deal each Alaalid 3 psychic damage."
            IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => c.DoKeywordsContain("alaalid"), 3, DamageType.Psychic, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
