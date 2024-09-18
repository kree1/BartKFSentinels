using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Victory
{
    public class ImpromptuSurgeryCardController : StressCardController
    {
        public ImpromptuSurgeryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show amount of psychic damage dealt to Victory this turn
            SpecialStringMaker.ShowIfElseSpecialString(() => PsychicDamageDealtToVictoryThisTurn() > 0, () => CharacterCard.Title + " has been dealt " + PsychicDamageDealtToVictoryThisTurn().ToString() + " psychic damage this turn.", () => CharacterCard.Title + " has not been dealt psychic damage this turn.");
        }

        public int PsychicDamageDealtToVictoryThisTurn()
        {
            List<int> amounts = (from DealDamageJournalEntry ddje in Journal.DealDamageEntriesThisTurn() where ddje.TargetCard == CharacterCard && ddje.Amount > 0 && ddje.DamageType == DamageType.Psychic select ddje.Amount).ToList();
            int result = 0;
            foreach (int a in amounts)
            {
                result += a;
            }
            return result;
        }

        public override IEnumerator Play()
        {
            // "{Victory} deals herself 4 irreducible psychic damage unless you put the bottom 3 cards of your trash on the bottom of your deck."
            IEnumerator stressCoroutine = PayStress(3);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(stressCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(stressCoroutine);
            }
            // "One hero target regains 5 HP."
            IEnumerator healCoroutine = GameController.SelectAndGainHP(DecisionMaker, 5, false, (Card c) => IsHeroTarget(c), cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(healCoroutine);
            }
            // "If {Victory} was dealt 2 or more psychic damage this turn, she may regain 2 HP or use a power."
            IEnumerator bonusCoroutine = GameController.SendMessageAction(CharacterCard.Title + " has not been dealt 2 or more psychic damage this turn.", Priority.Medium, GetCardSource());
            if (PsychicDamageDealtToVictoryThisTurn() >= 2)
            {
                List<Function> options = new List<Function>();
                options.Add(new Function(DecisionMaker, "Regain 2 HP", SelectionType.GainHP, () => GameController.GainHPEx(CharacterCard, 2, cardSource: GetCardSource()), repeatDecisionText: "regain 2 HP"));
                options.Add(new Function(DecisionMaker, "Use a power", SelectionType.UsePower, () => GameController.SelectAndUsePowerEx(DecisionMaker, true, cardSource: GetCardSource()), GameController.CanUsePowers(DecisionMaker, GetCardSource()), repeatDecisionText: "use a power"));
                SelectFunctionDecision choice = new SelectFunctionDecision(GameController, DecisionMaker, options, true, cardSource: GetCardSource());
                bonusCoroutine = GameController.SelectAndPerformFunction(choice);
            }
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(bonusCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(bonusCoroutine);
            }
        }
    }
}
