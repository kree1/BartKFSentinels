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
    public class SmithyCardController : BallparkModCardController
    {
        public SmithyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Ongoing and Equipment cards in each hero trash
            SpecialStringMaker.ShowNumberOfCardsAtLocations(() => from httc in base.GameController.FindHeroTurnTakerControllers()
                                                                  where !httc.IsIncapacitatedOrOutOfGame
                                                                  select httc.TurnTaker.Trash, new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "Ongoing or Equipment"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, one player may put an Ongoing or Equipment card from their trash on top of their deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt.IsEnvironment, BeckonResponse, TriggerType.MoveCard);
        }

        IEnumerator BeckonResponse(PhaseChangeAction pca)
        {
            // "... one player may put an Ongoing or Equipment card from their trash on top of their deck."
            IEnumerator selectCoroutine = base.GameController.SelectHeroToMoveCardFromTrash(DecisionMaker, (HeroTurnTakerController httc) => httc.TurnTaker.Deck, cardCriteria: new LinqCardCriteria((Card c) => IsOngoing(c) || IsEquipment(c), "Ongoing or Equipment"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
