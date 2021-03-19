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
    public class EvidenceStorageUtilityCardController : CardController
    {
        public EvidenceStorageUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        public IEnumerator DigForStorage()
        {
            // "... discard cards from the top of the environment deck until a Storage card is discarded. Put it into play."
            IEnumerator digCoroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(base.TurnTakerController, base.TurnTaker.Deck, null, new LinqCardCriteria((Card c) => c.DoKeywordsContain("storage"), "Storage"), isPutIntoPlay: true, fromBottom: false, revealUntilNumberOfMatchingCards: 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(digCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(digCoroutine);
            }
            yield break;
        }
    }
}
