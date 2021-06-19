using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheGoalie
{
    public class ExtraTimeCardController : TheGoalieUtilityCardController
    {
        public ExtraTimeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, GoalpostsCards);
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, GoalpostsCards);
        }

        public override IEnumerator Play()
        {
            // "Search your deck and trash for a Goalposts card and put it into play. Shuffle your deck."
            IEnumerator searchCoroutine = base.FetchGoalpostsResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(searchCoroutine);
            }
            // "{TheGoalieCharacter} regains 4 HP."
            IEnumerator healCoroutine = base.GameController.GainHP(base.CharacterCard, new int?(4), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            // "Immediately end your turn."
            IEnumerator skipCoroutine = base.GameController.ImmediatelyEndTurn(base.TurnTakerController, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(skipCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(skipCoroutine);
            }
            yield break;
        }
    }
}
