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
    public class SoloCardController : CardController
    {
        public SoloCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "One hero regains 2 HP and may use a power."
            return GameController.SelectCardAndDoAction(new SelectCardDecision(GameController, DecisionMaker, SelectionType.GainHPAndUsePower, FindCardsWhere(new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && !c.IsIncapacitated && IsHeroCharacterCard(c), "active hero character")), cardSource: GetCardSource()), (SelectCardDecision d) => GainHPAndUsePower(d.SelectedCard));
        }

        public IEnumerator GainHPAndUsePower(Card c)
        {
            if (c != null)
            {
                IEnumerator healCoroutine = GameController.GainHP(c, 2, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(healCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(healCoroutine);
                }
                IEnumerator powerCoroutine = SelectAndUsePower(FindCardController(c));
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(powerCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(powerCoroutine);
                }
            }
        }
    }
}
