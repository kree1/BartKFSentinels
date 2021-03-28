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
    public class RepurposedHardwareCardController : TorrentUtilityCardController
    {
        public RepurposedHardwareCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNumberOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => c.DoKeywordsContain("cluster"), "Cluster"), showInEffectsList: () => true);
            SpecialStringMaker.ShowIfElseSpecialString(() => base.Journal.DestroyCardEntriesThisTurn().Where((DestroyCardJournalEntry dcje) => dcje.Card.DoKeywordsContain("cluster")).Count() > 0, () => "A Cluster has already been destroyed this turn.", () => "No Clusters have been destroyed this turn.");
        }

        public override IEnumerator Play()
        {
            // "X on this card = the number of Clusters in your trash."
            Func<Card, int?> amount = (Card c) => base.TurnTaker.Trash.Cards.Where((Card d) => d.DoKeywordsContain("cluster")).Count();
            // "{TorrentCharacter} deals 1 target X lightning damage."
            IEnumerator lightningCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), amount, DamageType.Lightning, () => 1, false, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(lightningCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(lightningCoroutine);
            }
            // "If a Cluster was destroyed this turn, {TorrentCharacter} may deal 1 target X energy damage."
            if (base.Journal.DestroyCardEntriesThisTurn().Where((DestroyCardJournalEntry dcje) => dcje.Card.DoKeywordsContain("cluster")).Count() > 0)
            {
                IEnumerator energyCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), amount, DamageType.Energy, () => 1, false, 0, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(energyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(energyCoroutine);
                }
            }
            // "You may shuffle your trash into your deck."
            List<YesNoCardDecision> answer = new List<YesNoCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.ShuffleTrashIntoDeck, base.Card, storedResults: answer, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            if (DidPlayerAnswerYes(answer))
            {
                IEnumerator shuffleCoroutine = base.GameController.ShuffleTrashIntoDeck(base.TurnTakerController, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
            yield break;
        }
    }
}
