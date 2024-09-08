using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Dreadnought
{
    public class EmeraldCharacterCardController : HeroCharacterCardController
    {
        public EmeraldCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show number of Ongoing cards in deck
            SpecialStringMaker.ShowNumberOfCardsAtLocation(TurnTaker.Deck, new LinqCardCriteria((Card c) => IsOngoing(c), "Ongoing"));
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "Discard the top 3 cards of your deck. You may play an Ongoing card discarded this way."
            int numDiscards = GetPowerNumeral(0, 3);
            List<MoveCardAction> actions = new List<MoveCardAction>();
            IEnumerator discardCoroutine = GameController.DiscardTopCards(DecisionMaker, TurnTaker.Deck, numDiscards, storedResults: actions, responsibleTurnTaker: TurnTaker, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(discardCoroutine);
            }
            List<Card> discarded = (from MoveCardAction mca in actions where mca.WasCardMoved select mca.CardToMove).Distinct().ToList();
            IEnumerator playCoroutine = GameController.SelectAndPlayCard(DecisionMaker, (Card c) => discarded.Contains(c) && IsOngoing(c), optional: true, cardSource: GetCardSource(), noValidCardsMessage: "No Ongoing cards were discarded.");
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(playCoroutine);
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may draw a card now."
            yield return GameController.SelectHeroToDrawCard(DecisionMaker, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption2()
        {
            // "Destroy an environment card."
            yield return GameController.SelectAndDestroyCard(DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment"), false, responsibleCard: CharacterCard, cardSource: GetCardSource());
        }

        private IEnumerator UseIncapOption3()
        {
            // "One target regains 2 HP."
            yield return GameController.SelectAndGainHP(DecisionMaker, 2, cardSource: GetCardSource());
        }
    }
}
