using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.EvidenceStorage
{
    public class OfficerCardController : EvidenceStorageUtilityCardController
    {
        public OfficerCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override void AddStartOfGameTriggers()
        {
            // "When this card is discarded from the environment deck, put it into play."
            AddTrigger<MoveCardAction>((MoveCardAction mca) => mca.Destination.IsTrash && mca.CardToMove == base.Card, PutIntoPlayIfDiscarded, TriggerType.PutIntoPlay, TriggerTiming.After, isActionOptional: false, outOfPlayTrigger: true);
            base.AddStartOfGameTriggers();
        }

        public IEnumerator PutIntoPlayIfDiscarded(MoveCardAction mca)
        {
            // "When this card is discarded from the environment deck, put it into play."
            if (mca.IsDiscard)
            {
                MoveCardJournalEntry entry = (from mc in base.Journal.MoveCardEntriesThisTurn() where mc.Card == base.Card && mc.ToLocation.IsTrash select mc).LastOrDefault();
                bool accepted = false;
                if (entry != null && entry.FromLocation.IsEnvironment)
                {
                    if (entry.FromLocation.IsDeck)
                    {
                        accepted = true;
                    }
                    else if (entry.FromLocation.IsRevealed)
                    {
                        MoveCardJournalEntry revealEntry = (from mc in base.Journal.MoveCardEntriesThisTurn() where mc.Card == base.Card && mc.ToLocation.IsRevealed select mc).LastOrDefault();
                        if (revealEntry != null && revealEntry.FromLocation.IsDeck)
                        {
                            accepted = true;
                        }
                    }
                }
                if (accepted)
                {
                    IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, base.Card, base.TurnTaker.PlayArea, isPutIntoPlay: true, showMessage: false, responsibleTurnTaker: base.TurnTaker, actionSource: mca, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(moveCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(moveCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
