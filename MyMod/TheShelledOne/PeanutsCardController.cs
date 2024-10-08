﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class PeanutsCardController : BlaseballWeatherCardController
    {
        public PeanutsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowLocationOfCards(new LinqCardCriteria((Card c) => c.Identifier == "GiantPeanutShell"));
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Increase damage dealt by Pods by 1."
            AddIncreaseDamageTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard && (dda.DamageSource.Card.DoKeywordsContain("pod") || dda.DamageSource.Card.NextToLocation.HasCard(base.TurnTaker.FindCard("GiantPeanutShell"))), 1);
            //AddTrigger((DealDamageAction dda) => dda.DamageSource != null && dda.DamageSource.IsCard, CheckCriteriaResponse, TriggerType.Hidden, TriggerTiming.Before);
            // "At the start of the villain turn, each player may discard a card. If fewer than {H} cards were discarded this way, search the villain deck and trash for {GiantPeanutShell} and put it into play, then shuffle the villain deck."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardSearchResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PutIntoPlay });
        }

        public IEnumerator CheckCriteriaResponse(DealDamageAction dda)
        {
            // Debugging
            Card dealer = dda.DamageSource.Card;
            Log.Debug("Damage dealer: " + dealer.Title);
            Log.Debug("DoKeywordsContain(\"pod\"): " + dealer.DoKeywordsContain("pod").ToString());
            IEnumerable<string> keywords = dealer.GetKeywords();
            foreach(string k in keywords)
            {
                Log.Debug("    keyword: " + k);
            }
            Log.Debug("NextToLocation.HasCard(base.TurnTaker.FindCard(\"GiantPeanutShell\")): " + dealer.NextToLocation.HasCard(base.TurnTaker.FindCard("GiantPeanutShell")).ToString());
            IEnumerable<Card> nextToCards = dealer.NextToLocation.Cards;
            foreach(Card c in nextToCards)
            {
                Log.Debug("    attached card: " + c.Title);
            }
            yield break;
        }

        public IEnumerator DiscardSearchResponse(GameAction ga)
        {
            // "... each player may discard a card."
            List<DiscardCardAction> discards = new List<DiscardCardAction>();
            IEnumerator discardCoroutine = base.GameController.SelectTurnTakersAndDoAction(DecisionMaker, new LinqTurnTakerCriteria((TurnTaker tt) => IsHero(tt) && !tt.IsIncapacitatedOrOutOfGame && tt.ToHero().HasCardsInHand), SelectionType.DiscardCard, (TurnTaker tt) => SelectAndDiscardCards(FindHeroTurnTakerController(tt.ToHero()), 1, optional: false, requiredDecisions: 0, responsibleTurnTaker: tt, storedResults: discards), requiredDecisions: 0, allowAutoDecide: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If fewer than {H} cards were discarded this way, search the villain deck and trash for {GiantPeanutShell} and put it into play, then shuffle the villain deck."
            if (GetNumberOfCardsDiscarded(discards) < H)
            {
                IEnumerator searchCoroutine = PlayCardFromLocations(new Location[] { base.TurnTaker.Deck, base.TurnTaker.Trash }, "GiantPeanutShell", isPutIntoPlay: true, shuffleAfterwardsIfDeck: false);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(searchCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(searchCoroutine);
                }
                IEnumerator shuffleCoroutine = ShuffleDeck(DecisionMaker, base.TurnTaker.Deck);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(shuffleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(shuffleCoroutine);
                }
            }
        }
    }
}
