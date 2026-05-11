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
    public class ReminiscenceCardController : DoubleEdgeCardController
    {
        public ReminiscenceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _toDiscard = 1;
        }

        public override IEnumerator OneShotEffect()
        {
            // "Shuffle a card from a trash into its deck."
            IEnumerable<LocationChoice> trashOptions = (from l in FindLocationsWhere((Location l) => l.IsTrash && l.Cards.Any((Card c) => c.IsInTrash)) select new LocationChoice(l));
            if (!trashOptions.Any())
            {
                IEnumerator messageCoroutine = GameController.SendMessageAction("There are no cards in trashes.", Priority.Medium, GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(messageCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(messageCoroutine);
                }
            }
            else
            {
                List<SelectLocationDecision> trashResults = new List<SelectLocationDecision>();
                IEnumerator trashCoroutine = GameController.SelectLocation(DecisionMaker, trashOptions, SelectionType.ShuffleCardIntoDeck, trashResults, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(trashCoroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(trashCoroutine);
                }
                if (DidSelectLocation(trashResults))
                {
                    Location trash = GetSelectedLocation(trashResults);
                    IEnumerator shuffleCoroutine = GameController.SelectCardAndDoAction(new SelectCardDecision(GameController, DecisionMaker, SelectionType.ShuffleCardIntoDeck, trash.Cards, allowAutoDecide: trash.Cards.Count() <= 1, cardSource: GetCardSource()), (SelectCardDecision d) => GameController.ShuffleCardIntoLocation(DecisionMaker, d.SelectedCard, FindDeckFromTrash(trash), false, cardSource: GetCardSource()));
                    if (UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(shuffleCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(shuffleCoroutine);
                    }
                }
            }
        }
    }
}
