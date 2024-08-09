using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class BallparkModCardController : OwnershipUtilityCardController
    {
        public BallparkModCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            // "Play this card in the environment play area."
            storedResults?.Add(new MoveCardDestination(base.GameController.FindEnvironmentTurnTakerController().TurnTaker.PlayArea));
            yield break;
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "When an environment card leaves play, if there are no environment cards in play, destroy this card."
            AddTrigger((MoveCardAction mca) => mca.CardToMove.IsEnvironment && mca.Origin.IsInPlay && !mca.Destination.IsInPlay && mca.WasCardMoved && !base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsEnvironment && c.IsInPlay)).Any(), DestroyThisCardResponse, TriggerType.DestroySelf, TriggerTiming.After);
        }

        public Card StatCardOf(HeroTurnTakerController hero)
        {
            return base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == StatCardIdentifier && c.Location.IsPlayAreaOf(hero.TurnTaker)), visibleToCard: GetCardSource()).FirstOrDefault();
        }

        public Card StatCardOf(TurnTaker tt)
        {
            return StatCardOf(base.GameController.FindHeroTurnTakerController(tt.ToHero()));
        }
    }
}
