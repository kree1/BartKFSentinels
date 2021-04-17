using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class ReconNodeCardController : ClusterCardController
    {
        public ReconNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, X players may each draw a card, where X = the number of targets destroyed this turn."
            AddWhenDestroyedTrigger(DrawCardsResponse, TriggerType.DrawCard);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            if (Journal.GetCardPropertiesBoolean(base.Card, IgnoreEntersPlay) != true)
            {
                // "When this card enters play, you may play a card."
                IEnumerator playCoroutine = SelectAndPlayCardFromHand(base.HeroTurnTakerController, associateCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator DrawCardsResponse(DestroyCardAction dca)
        {
            // "... X players may each draw a card, where X = the number of targets destroyed this turn."
            int x = base.NumTargetsDestroyedThisTurn();
            IEnumerator drawCoroutine = base.GameController.SelectTurnTakersAndDoAction(base.HeroTurnTakerController, new LinqTurnTakerCriteria((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt.IsHero), SelectionType.DrawCard, (TurnTaker tt) => DrawCard(tt.ToHero(), optional: true), x, requiredDecisions: 0, allowAutoDecide: x >= base.H, cardSource: GetCardSource());
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
    }
}
