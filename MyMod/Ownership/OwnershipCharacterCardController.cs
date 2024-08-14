using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class OwnershipCharacterCardController : OwnershipBaseCharacterCardController
    {
        public OwnershipCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
            // Both sides: list of Weather Effect cards in the villain deck
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Deck, new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(WeatherEffectKeyword), "Weather Effect"));
            // Front side: list of Replica cards in the villain trash
            SpecialStringMaker.ShowListOfCardsAtLocation(base.TurnTaker.Trash, new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(ReplicaKeyword), "Replica")).Condition = () => !base.Card.IsFlipped;
        }

        public readonly string ModificationKeyword = "modification";
        public readonly string WeatherEffectKeyword = "weather effect";
        public readonly string ReplicaKeyword = "replica";
        public readonly string CollapseKeyword = "collapse";
        public readonly string SunKeyword = "sun";

        public override bool AskIfCardIsIndestructible(Card card)
        {
            // Back side:
            // "Villain Collapses are indestructible."
            if (base.Card.IsFlipped)
            {
                return base.GameController.GetAllKeywords(card).Contains(CollapseKeyword) && IsVillain(card);
            }
            return base.AskIfCardIsIndestructible(card);
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Whenever a villain Modification is revealed, put it under this card. Then, if there are 3 or more cards under this card, play 1 of them and discard the rest."
                AddSideTrigger(AddTrigger((RevealCardsAction rca) => rca.RevealedCards.Any((Card c) => IsVillain(c) && base.GameController.GetAllKeywords(c).Contains(ModificationKeyword)), CatchRevealedResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.PlayCard }, TriggerTiming.After));
                // "At the start of each hero turn, reveal cards from the villain deck until a Weather Effect is revealed. Put it into play and discard the other revealed cards. If no card entered play this way, shuffle the villain trash into the villain deck."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => IsHero(tt), StayPositiveWeatherResponse, new TriggerType[] { TriggerType.RevealCard, TriggerType.PutIntoPlay, TriggerType.DiscardCard }));
                // "At the start of the villain turn, play the top card of the environment deck and 1 Replica from the villain trash."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, OpposingLineupResponse, TriggerType.PlayCard));
                if (base.IsGameAdvanced)
                {
                    // Front side, Advanced:
                    // "At the end of the villain turn, add 3 tokens to each player's Stat card."
                    AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, WeighDownResponse, TriggerType.AddTokensToPool));
                }
            }
            else
            {
                // Back side:
                // "When a villain Modification is destroyed, remove it from the game."
                AddSideTrigger(AddTrigger((DestroyCardAction dca) => dca.CardToDestroy.CanBeDestroyed && dca.WasCardDestroyed && IsVillain(dca.CardToDestroy.Card) && base.GameController.GetAllKeywords(dca.CardToDestroy.Card).Contains(ModificationKeyword) && dca.PostDestroyDestinationCanBeChanged, RemoveFromGameResponse, new TriggerType[] { TriggerType.MoveCard, TriggerType.ChangePostDestroyDestination }, TriggerTiming.After));
                AddSideTrigger(AddTrigger((MoveCardAction mca) => mca.Destination.IsOutOfGame && IsVillain(mca.CardToMove) && base.GameController.GetAllKeywords(mca.CardToMove).Contains(SunKeyword) && base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsVillain(c) && base.GameController.GetAllKeywords(c).Contains(SunKeyword) && c.IsOutOfGame)).Count() >= 4, (MoveCardAction mca) => base.GameController.SendMessageAction("[b]What have you done to Our Suns?[/b]", Priority.Medium, GetCardSource(), showCardSource: true), TriggerType.ShowMessage, TriggerTiming.After));
                // "Increase damage dealt to this card by hero targets in play areas that have a villain Stat card by 5."
                AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == base.Card && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == StatCardIdentifier && c.IsInPlayAndNotUnderCard && c.Location.HighestRecursiveLocation == dda.DamageSource.Card.Location.HighestRecursiveLocation)).Any(), (DealDamageAction dda) => 5));
                // "At the start of each hero turn, discard the top card of the villain deck. If it's a Weather Effect, play it."
                AddSideTrigger(AddStartOfTurnTrigger((TurnTaker tt) => IsHero(tt), FireSaleWeatherResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard }));
                // "At the end of the villain turn, {OwnershipCharacter} deals each hero character 5 infernal damage."
                AddSideTrigger(AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.Card, (Card c) => IsHeroCharacterCard(c), TargetType.All, 5, DamageType.Infernal));
                if (base.IsGameAdvanced)
                {
                    // Back side, Advanced:
                    // "At the end of the villain turn, move each marker in row 0 1 space left, then move each other marker 1 space down."
                    AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, HorizonPullResponse, TriggerType.AddTokensToPool));
                }
            }
            AddDefeatedIfDestroyedTriggers();
            AddDefeatedIfMovedOutOfGameTriggers();
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            IEnumerator baseCoroutine = base.AfterFlipCardImmediateResponse();
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(baseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(baseCoroutine);
            }
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("[b]Okay no one panic\nThis is Fine.\nWe will handle this.[/b]", Priority.Medium, GetCardSource(), showCardSource: true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            // "When this card flips to this side, discard each card under it."
            if (base.Card.IsFlipped)
            {
                IEnumerator discardCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.Card.UnderLocation.Cards, (Card u) => new MoveCardDestination(u.NativeTrash), responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
        }

        public IEnumerator CatchRevealedResponse(RevealCardsAction rca)
        {
            foreach (Card c in rca.RevealedCards)
            {
                if (IsVillain(c) && base.GameController.GetAllKeywords(c).Contains(ModificationKeyword))
                {
                    IEnumerator electCoroutine = AddToElectionResponse(c);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(electCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(electCoroutine);
                    }
                }
            }
        }

        public IEnumerator AddToElectionResponse(Card c)
        {
            // "... put it under this card."
            IEnumerator moveCoroutine = base.GameController.MoveCard(base.TurnTakerController, c, base.Card.UnderLocation, playCardIfMovingToPlayArea: false, showMessage: true, responsibleTurnTaker: base.TurnTaker, doesNotEnterPlay: true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(moveCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(moveCoroutine);
            }
            // "Then, if there are 3 or more cards under this card, play 1 of them and discard the rest."
            if (base.Card.UnderLocation.NumberOfCards >= 3)
            {
                IEnumerator announceCoroutine = base.GameController.SendMessageAction("[b]You deserve a say.\nIt's only Fair.[/b]", Priority.Medium, GetCardSource(), associatedCards: base.Card.UnderLocation.Cards, showCardSource: true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(announceCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(announceCoroutine);
                }
                IEnumerator playCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, base.Card.UnderLocation.Cards, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                IEnumerator discardCoroutine = base.GameController.MoveCards(base.TurnTakerController, base.Card.UnderLocation.Cards, (Card u) => new MoveCardDestination(u.NativeTrash), responsibleTurnTaker: base.TurnTaker, isDiscard: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
        }

        public IEnumerator StayPositiveWeatherResponse(PhaseChangeAction pca)
        {
            // "... reveal cards from the villain deck until a Weather Effect is revealed. Put it into play and discard the other revealed cards."
            List<Card> played = new List<Card>();
            IEnumerator revealCoroutine = RevealCards_PutSomeIntoPlay_DiscardRemaining(base.TurnTakerController, base.TurnTaker.Deck, null, new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(WeatherEffectKeyword), "Weather Effect"), playedCards: played, revealUntilNumberOfMatchingCards: 1);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(revealCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(revealCoroutine);
            }
            // "If no card entered play this way, shuffle the villain trash into the villain deck."
            if (!played.Any())
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
        }

        public IEnumerator OpposingLineupResponse(PhaseChangeAction pca)
        {
            // "... play the top card of the environment deck..."
            IEnumerator envPlayCoroutine = PlayTheTopCardOfTheEnvironmentDeckWithMessageResponse(null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(envPlayCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(envPlayCoroutine);
            }
            // "... and 1 Replica from the villain trash."
            LinqCardCriteria replicaCriteria = new LinqCardCriteria((Card c) => base.GameController.GetAllKeywords(c).Contains(ReplicaKeyword) && c.Location.IsTrash && c.Location.IsVillain, "Replica", singular: "card in the villain trash", plural: "cards in the villain trash");
            List<Card> availableReplicas = base.GameController.FindCardsWhere(replicaCriteria, visibleToCard: GetCardSource()).ToList();
            IEnumerator replicaCoroutine;
            if (availableReplicas.Any())
            {
                replicaCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, availableReplicas, cardSource: GetCardSource());
            }
            else
            {
                replicaCoroutine = base.GameController.SendMessageAction("[b]There are no Replicas in stock at the moment.\nPlease check back soon.[/b]", Priority.Medium, GetCardSource(), showCardSource: true);
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(replicaCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(replicaCoroutine);
            }
        }

        public IEnumerator WeighDownResponse(PhaseChangeAction pca)
        {
            // "... add 3 tokens to each player's Stat card."
            List<Card> activeStats = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Identifier == StatCardIdentifier && c.Location.IsHero, "Stat", singular: "card in a hero play area", plural: "cards in hero play areas"), visibleToCard: GetCardSource()).ToList();
            foreach (Card c in activeStats)
            {
                IEnumerator addCoroutine = base.GameController.AddTokensToPool(c.FindTokenPool(WeightPoolIdentifier), 3, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(addCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(addCoroutine);
                }
            }
        }

        public IEnumerator RemoveFromGameResponse(DestroyCardAction dca)
        {
            IEnumerator announceCoroutine = base.GameController.SendMessageAction(base.Card.Title + " removes " + dca.CardToDestroy.Card.Title + " from the game.", Priority.Medium, GetCardSource(), associatedCards: dca.CardToDestroy.Card.ToEnumerable());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(announceCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(announceCoroutine);
            }
            dca.SetPostDestroyDestination(base.TurnTaker.OutOfGame, cardSource: GetCardSource());
        }

        public IEnumerator FireSaleWeatherResponse(PhaseChangeAction pca)
        {
            // "... discard the top card of the villain deck."
            List<MoveCardAction> discards = new List<MoveCardAction>();
            IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.TurnTaker.Deck, discards, (Card c) => true, base.TurnTaker, GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            // "If it's a Weather Effect, play it."
            MoveCardAction move = discards.FirstOrDefault();
            if (move != null && base.GameController.GetAllKeywords(move.CardToMove).Contains(WeatherEffectKeyword))
            {
                IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, move.CardToMove, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
            }
        }

        public IEnumerator HorizonPullResponse(PhaseChangeAction pca)
        {
            // "... move each marker in row 0 1 space left, ..."
            for (int i = 1; i <= H; i++)
            {
                int[] iLocation = HeroMarkerLocation(i);
                if (iLocation[0] == 0)
                {
                    IEnumerator leftCoroutine = MoveHeroMarker(i, 0, -1, base.TurnTaker, showMessage: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(leftCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(leftCoroutine);
                    }
                }
            }
            // "... then move each other marker 1 space down."
            for (int i = 1; i < H; i++)
            {
                int[] iLocation = HeroMarkerLocation(i);
                if (iLocation[0] > 0)
                {
                    IEnumerator downCoroutine = MoveHeroMarker(i, -1, 0, base.TurnTaker, showMessage: true, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(downCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(downCoroutine);
                    }
                }
            }
        }
    }
}
