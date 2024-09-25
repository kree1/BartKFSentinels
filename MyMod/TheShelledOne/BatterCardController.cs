using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class BatterCardController : CardController
    {
        public BatterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            BasePoolIdentifier = base.Card.Identifier + "BasePool";
            SpecialStringMaker.ShowTokenPool(base.Card.Identifier, BasePoolIdentifier);
            SpecialStringMaker.ShowHeroTargetWithHighestHP();
            SpecialStringMaker.ShowIfElseSpecialString(() => HasBeenSetToTrueThisTurn(OncePerTurn), () => Card.Title + " has already reacted to damage this turn.", () => Card.Title + " has not yet reacted to damage this turn.");
        }

        public string BasePoolIdentifier;
        public const string OncePerTurn = "DamageOncePerTurn";

        public int BasePoolValue()
        {
            TokenPool basePool = base.Card.FindTokenPool(BasePoolIdentifier);
            if (basePool != null)
            {
                return basePool.CurrentValue;
            }
            else
            {
                return 0;
            }
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Reduce damage dealt to this card by 1."
            AddReduceDamageTrigger((Card c) => c == base.Card, 1);
            // "X on this card = the number of tokens on this card."

            // "The first time any villain target is dealt damage each turn, put a token on this card. Then, if X is 3 or less, this card deals the hero target with the highest HP X melee damage. Otherwise, this card deals each hero target X toxic damage and is put on the bottom of the villain deck."
            AddTrigger((DealDamageAction dda) => !HasBeenSetToTrueThisTurn(OncePerTurn) && IsVillainTarget(dda.Target) && dda.DidDealDamage, EasyPitchResponse, new TriggerType[] { TriggerType.AddTokensToPool, TriggerType.DealDamage, TriggerType.MoveCard }, TriggerTiming.After);
            AddAfterLeavesPlayAction((GameAction ga) => ResetFlagAfterLeavesPlay(OncePerTurn), TriggerType.Hidden);
            // Cards out of play can't have tokens
            AddBeforeLeavesPlayAction(ResetPoolResponse, TriggerType.ModifyTokens);
        }

        public override IEnumerator Play()
        {
            base.Card.FindTokenPool(BasePoolIdentifier).SetToInitialValue();
            yield break;
        }

        public IEnumerator ResetPoolResponse(GameAction ga)
        {
            base.Card.FindTokenPool(BasePoolIdentifier).SetToInitialValue();
            yield break;
        }

        public IEnumerator EasyPitchResponse(DealDamageAction dda)
        {
            SetCardPropertyToTrueIfRealAction(OncePerTurn);
            TokenPool basePool = base.Card.FindTokenPool(BasePoolIdentifier);
            // "... put a token on this card."
            IEnumerator addTokenCoroutine = base.GameController.AddTokensToPool(basePool, 1, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(addTokenCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(addTokenCoroutine);
            }

            if (BasePoolValue() <= 3)
            {
                // "Then, if X is 3 or less, this card deals the hero target with the highest HP X melee damage."
                IEnumerator swingCoroutine = DealDamageToHighestHP(base.Card, 1, (Card c) => IsHeroTarget(c), (Card c) => BasePoolValue(), DamageType.Melee);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(swingCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(swingCoroutine);
                }
            }
            else
            {
                // "Otherwise, this card deals each hero target X toxic damage..."
                IEnumerator runCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => IsHeroTarget(c), BasePoolValue(), DamageType.Toxic, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(runCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(runCoroutine);
                }
                // "... and is put on the bottom of the villain deck."
                IEnumerator cycleCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.Deck, toBottom: true, showMessage: true, responsibleTurnTaker: base.TurnTaker, actionSource: dda, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cycleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cycleCoroutine);
                }
                /*// If this interrupted damage being dealt to this card, cancel that damage
                if (dda.Target == base.Card)
                {
                    IEnumerator cancelCoroutine = base.GameController.CancelAction(dda, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(cancelCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(cancelCoroutine);
                    }
                }*/
            }
        }
    }
}
