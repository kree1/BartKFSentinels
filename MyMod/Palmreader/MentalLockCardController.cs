using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Palmreader
{
    public class MentalLockCardController : PalmreaderUtilityCardController
    {
        public MentalLockCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => IsRelay(c), "goalposts"));
        }

        public override IEnumerator Play()
        {
            // "Search your deck for a Relay card and put it into your hand. Shuffle your deck."
            LinqCardCriteria match = new LinqCardCriteria((Card c) => IsRelay(c));
            IEnumerator searchCoroutine = base.SearchForCards(base.HeroTurnTakerController, true, false, new int?(1), 1, match, false, true, false, optional: false, shuffleAfterwards: new bool?(true));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(searchCoroutine);
            }
            // "You may play a Relay card or draw 2 cards."
            List<Function> options = new List<Function>();
            options.Add(new Function(base.HeroTurnTakerController, "Play a Relay card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.HeroTurnTakerController, cardCriteria: match), onlyDisplayIfTrue: base.HeroTurnTaker.Hand.Cards.Any((Card c) => IsRelay(c))));
            options.Add(new Function(base.HeroTurnTakerController, "Draw 2 cards", SelectionType.DrawCard, () => base.DrawCards(base.HeroTurnTakerController, 2, optional: true)));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, cardSource: GetCardSource());
            IEnumerator playDrawCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playDrawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playDrawCoroutine);
            }
            // "{TheGoalieCharacter} deals 1 target 1 melee damage."
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 1, DamageType.Melee, 1, false, 1, cardSource: GetCardSource());
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
