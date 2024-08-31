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
    public class CeiffCardController : AlaaluUtilityCardController
    {
        public CeiffCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNonEnvironmentTargetWithLowestHP(ranking: 1, numberOfTargets: 1);
            SpecialStringMaker.ShowNumberOfCardsInPlay(LivestockCriteria());
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, the non-environment target with the lowest HP regains X HP, where X is 1 plus the number of Livestock in play."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HealResponse, TriggerType.GainHP);
        }

        public IEnumerator HealResponse(GameAction ga)
        {
            // "... the non-environment target with the lowest HP regains X HP, where X is 1 plus the number of Livestock in play."
            List<Card> lowest = new List<Card>();
            IEnumerator findCoroutine = base.GameController.FindTargetWithLowestHitPoints(1, (Card c) => c.IsTarget && !c.IsEnvironmentTarget, lowest, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(findCoroutine);
            }
            //Log.Debug("lowest.Any(): " + lowest.Any().ToString());
            if (lowest.Any())
            {
                Card lowestCard = lowest.FirstOrDefault();
                //Log.Debug("lowestCard: " + lowestCard.Title);
                IEnumerator healCoroutine = base.GameController.GainHP(lowestCard, base.GameController.FindCardsWhere(LivestockInPlayCriteria()).Count() + 1, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(healCoroutine);
                }
            }
            yield break;
        }
    }
}
