using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class HyperRetentionCardController : ImpulseUtilityCardController
    {
        public HyperRetentionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "Up to 2 other players may each return a card from their trash to their hand."
            SelectTurnTakersDecision selectPlayers = new SelectTurnTakersDecision(base.GameController, base.HeroTurnTakerController, new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker && tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && GameController.IsTurnTakerVisibleToCardSource(tt, GetCardSource())), SelectionType.MoveCardToHandFromTrash, 2, isOptional: false, numberOfCards: 1, cardSource: GetCardSource());
            IEnumerator returnCoroutine = base.GameController.SelectTurnTakersAndDoAction(selectPlayers, (TurnTaker tt) => base.GameController.SelectCardFromLocationAndMoveIt(FindHeroTurnTakerController(tt.ToHero()), tt.Trash, new LinqCardCriteria((Card c) => true), new List<MoveCardDestination> { new MoveCardDestination(tt.ToHero().Hand) }, optional: true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource()), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(returnCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(returnCoroutine);
            }
            // "You may draw a card or play an Ongoing card."
            List<Function> options = new List<Function>();
            List<PlayCardAction> plays = new List<PlayCardAction>();
            options.Add(new Function(base.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => base.GameController.DrawCard(base.HeroTurnTaker, optional: true, cardSource: GetCardSource())));
            options.Add(new Function(base.HeroTurnTakerController, "Play an Ongoing card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.HeroTurnTakerController, storedResults: plays, cardCriteria: new LinqCardCriteria((Card c) => c.DoKeywordsContain("ongoing"), "Ongoing"))));
            SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, noSelectableFunctionMessage: base.CharacterCard.Title + " cannot draw or play any cards.", cardSource: GetCardSource());
            IEnumerator chooseCoroutine = base.GameController.SelectAndPerformFunction(choice);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            yield break;
        }
    }
}
