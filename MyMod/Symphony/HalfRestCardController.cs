using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Symphony
{
    public class HalfRestCardController : BenefitCardController
    {
        public HalfRestCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 1;
        }

        public override IEnumerator OneShotEffect()
        {
            // "Each other player may draw a card or have their hero regain 2 HP."
            Func<HeroTurnTakerController, IEnumerable<Function>> options = (HeroTurnTakerController httc) => new Function[2]
            {
                new Function(httc, "Draw a card", SelectionType.DrawCard, () => DrawCard(httc.HeroTurnTaker), onlyDisplayIfTrue: CanDrawCards(httc), repeatDecisionText: "draw a card"),
                new Function(httc, "Your hero regains 2 HP", SelectionType.GainHP, () => GameController.SelectAndGainHP(httc, 2, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Owner == httc.TurnTaker, cardSource: GetCardSource()), repeatDecisionText: "your hero regains 2 HP")
            };
            IEnumerator selectCoroutine = EachPlayerSelectsFunction((HeroTurnTakerController httc) => httc != DecisionMaker && !httc.IsIncapacitatedOrOutOfGame, options);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(selectCoroutine);
            }
        }
    }
}
