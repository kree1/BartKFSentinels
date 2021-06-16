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
    public class StartingWhistleCardController : TheGoalieUtilityCardController
    {
        public StartingWhistleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, GoalpostsCards);
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, GoalpostsCards);
        }

        public override IEnumerator Play()
        {
            // "Search your deck for a Goalposts card and put it into your hand. Shuffle your deck."
            IEnumerator searchCoroutine = FetchGoalpostsResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(searchCoroutine);
            }
            // "{TheGoalieCharacter} deals 1 target 2 melee damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            // "Draw a card."
            IEnumerator drawCoroutine = base.DrawCard(base.HeroTurnTaker);
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
