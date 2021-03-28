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
    public class SoftwareUpdateCardController : TorrentUtilityCardController
    {
        public SoftwareUpdateCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.DoKeywordsContain("ongoing")), () => false);
            SpecialStringMaker.ShowListOfCardsInPlay(TargetWithOneHP(), () => false);
        }

        public override IEnumerator Play()
        {
            // "Search your deck or trash for an Ongoing card and put it into play. Shuffle your deck."
            IEnumerator searchCoroutine = SearchForCards(base.HeroTurnTakerController, true, true, 1, 1, new LinqCardCriteria((Card c) => c.DoKeywordsContain("ongoing"), "Ongoing"), true, false, false, shuffleAfterwards: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(searchCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(searchCoroutine);
            }
            // "You may destroy a target with 1 HP. If you do, you may play a card or destroy an Ongoing card."
            List<DestroyCardAction> destroyResults = new List<DestroyCardAction>();
            IEnumerator destroyTargetCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, TargetWithOneHP(), true, destroyResults, base.Card, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyTargetCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyTargetCoroutine);
            }
            if (destroyResults != null && destroyResults.Count > 0 && DidDestroyCard(destroyResults.First()))
            {
                List<Function> options = new List<Function>();
                options.Add(new Function(base.HeroTurnTakerController, "Play a card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.HeroTurnTakerController, associateCardSource: true), onlyDisplayIfTrue: base.HeroTurnTaker.HasCardsInHand));
                options.Add(new Function(base.HeroTurnTakerController, "Destroy an Ongoing card", SelectionType.DestroyCard, () => base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.DoKeywordsContain("ongoing"), "Ongoing"), true, responsibleCard: base.Card, cardSource: GetCardSource()), onlyDisplayIfTrue: FindCardsWhere((Card c) => c.DoKeywordsContain("ongoing"), visibleToCard: GetCardSource()).Count() > 0));
                SelectFunctionDecision choice = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, options, true, cardSource: GetCardSource());
                IEnumerator playDestroyCoroutine = base.GameController.SelectAndPerformFunction(choice);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playDestroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playDestroyCoroutine);
                }
            }
            yield break;
        }
    }
}
