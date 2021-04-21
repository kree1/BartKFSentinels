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
    public class TorrentUtilityCardController : CardController
    {
        public TorrentUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            if (base.Card.DoKeywordsContain("cluster"))
            {
                SpecialStringMaker.ShowSpecialString(() => base.Card.Title + " is already being destroyed.", () => false).Condition = () => base.Card.IsBeingDestroyed;
            }
        }

        public int NumTargetsDestroyedThisTurn()
        {
            /*IEnumerable<DestroyCardJournalEntry> cardsDestroyedThisTurn = base.Journal.DestroyCardEntriesThisTurn();
            Log.Debug(cardsDestroyedThisTurn.Count().ToString() + " cards destroyed this turn:");
            foreach(DestroyCardJournalEntry entry in cardsDestroyedThisTurn)
            {
                Log.Debug("* " + entry.Card.Title);
            }*/
            IEnumerable<DestroyCardJournalEntry> targetsDestroyedThisTurn = base.Journal.DestroyCardEntriesThisTurn().Where((DestroyCardJournalEntry dcje) => dcje.WasTargetWhenDestroyed);
            int count = targetsDestroyedThisTurn.Count();
            /*Log.Debug(count.ToString() + " targets destroyed this turn:");
            foreach(DestroyCardJournalEntry entry in targetsDestroyedThisTurn)
            {
                Log.Debug("* " + entry.Card.Title);
            }*/
            return count;
        }

        public static LinqCardCriteria TargetWithOneHP()
        {
            return new LinqCardCriteria((Card c) => c.IsInPlay && c.IsTarget && c.HitPoints.Value == 1, "targets with 1 HP", false, false, "target with 1 HP", "targets with 1 HP");
        }
    }
}
