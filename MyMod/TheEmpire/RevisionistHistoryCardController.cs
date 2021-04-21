using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEmpire
{
    public class RevisionistHistoryCardController : EmpireUtilityCardController
    {
        public RevisionistHistoryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            SpecialStringMaker.ShowIfElseSpecialString(() => CardsUnder() == "1", () => "There is 1 card under " + base.Card.Title + ".", () => "There are " + CardsUnder() + " cards under " + base.Card.Title + ".", showInEffectsList: () => base.Card.IsInPlayAndHasGameText);
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.OutOfGame, new LinqCardCriteria((Card c) => c.Owner == base.TurnTaker), showInEffectsList: () => base.Card.Location.IsOutOfGame).Condition = () => base.Card.Location.IsOutOfGame;
        }

        protected const string IsBeingErased = "IsBeingErased";

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // "This card and cards underneath it are indestructible."
            if (card == base.Card || card.Location == base.Card.UnderLocation)
            {
                return true;
            }
            else
            {
                return base.AskIfCardIsIndestructible(card);
            }
        }

        public string CardsUnder()
        {
            int numCards = base.Card.UnderLocation.NumberOfCards;
            string value = "";
            if (numCards == 0)
            {
                value = "no";
            }
            else
            {
                value = numCards.ToString();
            }
            return value;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by Imperial cards by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.Card.DoKeywordsContain(AuthorityKeyword), (DealDamageAction dda) => 1);
            // "When there are 3 cards under this one, each hero target regains 8 HP and each player may return a card from their trash to their hand. Then, remove this card and all cards under it from the game."
            AddTrigger<GameAction>((GameAction ga) => base.Card.UnderLocation.NumberOfCards >= 3 && !HasBeenSetToTrueThisTurn(IsBeingErased), TimelineFixedResponse, new TriggerType[] { TriggerType.GainHP, TriggerType.MoveCard, TriggerType.RemoveFromGame }, TriggerTiming.After);
        }

        public IEnumerator TimelineFixedResponse(GameAction ga)
        {
            SetCardPropertyToTrueIfRealAction(IsBeingErased);
            string announcement = "All key divergences in this world's timeline have been resolved!";
            IEnumerator announceCoroutine = base.GameController.SendMessageAction(announcement, Priority.High, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(announceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(announceCoroutine);
            }
            string effect = base.Card.Title + " and its effects are removed from this timeline!";
            IEnumerator messageCoroutine = base.GameController.SendMessageAction(effect, Priority.High, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            // "... each hero target regains 8 HP..."
            IEnumerator healCoroutine = base.GameController.GainHP(base.DecisionMaker, (Card c) => c.IsHero, 8, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "... and each player may return a card from their trash to their hand."
            IEnumerator selectCoroutine = base.GameController.SelectTurnTakersAndDoAction(base.DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero), SelectionType.MoveCardToHandFromTrash, MoveCardToHandResponse, allowAutoDecide: true, numberOfCards: 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            // "Then, remove this card and all cards under it from the game."
            while(base.Card.UnderLocation.HasCards)
            {
                IEnumerator removeCoroutine1 = base.GameController.MoveCard(base.TurnTakerController, base.Card.UnderLocation.TopCard, base.TurnTaker.OutOfGame, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(removeCoroutine1);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(removeCoroutine1);
                }
            }
            IEnumerator removeCoroutine2 = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.OutOfGame, responsibleTurnTaker: base.TurnTaker, evenIfIndestructible: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(removeCoroutine2);
            }
            else
            {
                base.GameController.ExhaustCoroutine(removeCoroutine2);
            }
            yield break;
        }

        public IEnumerator MoveCardToHandResponse(TurnTaker tt)
        {
            // "... may return a card from their trash to their hand."
            HeroTurnTakerController httc = FindHeroTurnTakerController(tt.ToHero());
            List<MoveCardDestination> dest = new List<MoveCardDestination>();
            dest.Add(new MoveCardDestination(httc.HeroTurnTaker.Hand));
            IEnumerator moveCoroutine = base.GameController.SelectCardFromLocationAndMoveIt(httc, tt.Trash, new LinqCardCriteria((Card c) => c.Location == tt.Trash), dest, optional: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            yield break;
        }
    }
}
