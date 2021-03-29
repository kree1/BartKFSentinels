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
    public class DivertAttentionCardController : TorrentUtilityCardController
    {
        public DivertAttentionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsAtLocations(() => from httc in base.GameController.FindHeroTurnTakerControllers()
                                                                       where !httc.IsIncapacitatedOrOutOfGame
                                                                       select httc.TurnTaker.Trash);
            SpecialStringMaker.ShowListOfCardsInPlay(TargetWithOneHP());
        }

        public override IEnumerator Play()
        {
            // "Each player may put a card other than Divert Attention from their trash on top of their deck."
            IEnumerator moveCoroutine = EachPlayerMovesCards(0, 1, SelectionType.MoveCardOnDeck, new LinqCardCriteria((Card c) => c.Identifier != "DivertAttention", "other than Divert Attention", false, true), (HeroTurnTaker htt) => htt.Trash, (HeroTurnTaker htt) => new List<MoveCardDestination> { new MoveCardDestination(htt.Deck) });
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            // "Destroy 2 targets with 1 HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, TargetWithOneHP(), 2, false, 2, responsibleCard: base.Card, allowAutoDecide: FindCardsWhere(TargetWithOneHP()).Count() <= 2, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }
    }
}
