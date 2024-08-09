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
    public class AlternateCardController : TeamModCardController
    {
        public AlternateCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator Play()
        {
            // "When this card enters play, replace a hero character card in this play area with a variant of that hero, ..."
            // Copied from Completionist Guise & Feedback; watch yourself
            List<Function> collectFunctions = new List<Function>();
            Func<string, bool> identifierCriteria = (string s) => !FindCardsWhere((Card c) => c.QualifiedPromoIdentifierOrIdentifier == s && c.Owner.CharacterCards.Contains(c), realCardsOnly: false, null, ignoreBattleZone: true).Any();
            Func<string, bool> turnTakerCriteria = delegate (string ttIdentifier)
            {
                Card card5 = FindCardsWhere((Card c) => c.Identifier == "RepresentativeOfEarth" && c.IsInPlayAndHasGameText, realCardsOnly: false, null, ignoreBattleZone: true).FirstOrDefault();
                return base.GameController.AllTurnTakers.Select((TurnTaker t) => t.QualifiedIdentifier).Contains(ttIdentifier) || (card5 != null && card5.NextToLocation.TopCard != null && card5.NextToLocation.TopCard.ParentDeck.QualifiedIdentifier == ttIdentifier);
            };
            IEnumerable<KeyValuePair<string, string>> allCardsInBox = base.GameController.GetHeroCardsInBox(identifierCriteria, turnTakerCriteria);
            // Edited this line to only find in-box variants of characters in this play area
            IEnumerable<Card> enumerable = from c in FindCardsWhere((Card c) => c.IsHeroCharacterCard && c.IsInPlayAndHasGameText && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation, realCardsOnly: true)
                                           where allCardsInBox.Any((KeyValuePair<string, string> a) => a.Key == c.ParentDeck.QualifiedIdentifier)
                                           select c;
            List<SelectCardDecision> storedHero = new List<SelectCardDecision>();
            IEnumerator coroutine;
            if (enumerable.Any())
            {
                coroutine = base.GameController.SelectCardAndStoreResults(DecisionMaker, SelectionType.ReplaceHero, enumerable, storedHero);
            }
            else
            {
                string message = "There are no more variants available to select.";
                coroutine = base.GameController.SendMessageAction(message, Priority.Medium, GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            SelectCardDecision hero = storedHero.FirstOrDefault();
            if (hero != null && hero.SelectedCard != null)
            {
                List<string> list = new List<string>();
                list.Add(hero.SelectedCard.ParentDeck.QualifiedIdentifier);
                list.Add(hero.SelectedCard.QualifiedPromoIdentifierOrIdentifier);
                base.GameController.AddCardPropertyJournalEntry(hero.SelectedCard, "OverrideTurnTaker", list);
                List<SelectFromBoxDecision> storedBox = new List<SelectFromBoxDecision>();
                Func<string, bool> identifierCriteria2 = delegate (string s)
                {
                    if (FindCardsWhere((Card c) => (c.Identifier == hero.SelectedCard.Identifier || (hero.SelectedCard.SharedIdentifier != null && hero.SelectedCard.SharedIdentifier == c.SharedIdentifier)) && c.QualifiedPromoIdentifierOrIdentifier == s && c.Owner.CharacterCards.Contains(c)).Any())
                    {
                        return false;
                    }
                    if (hero.SelectedCard.SharedIdentifier != null)
                    {
                        string identifier = hero.SelectedCard.Identifier;
                        CardDefinition cardDefinition2 = hero.SelectedCard.ParentDeck.GetAllCardDefinitions().FirstOrDefault((CardDefinition d) => d.QualifiedPromoIdentifierOrIdentifier == s);
                        if (cardDefinition2 == null)
                        {
                            cardDefinition2 = ModHelper.GetPromoDefinition(hero.SelectedCard.ParentDeck.QualifiedIdentifier, s);
                        }
                        if (cardDefinition2 != null)
                        {
                            return cardDefinition2.Identifier == identifier;
                        }
                        return false;
                    }
                    if (hero.SelectedCard.ParentDeck.InitialCardIdentifiers.Count() > 1)
                    {
                        IEnumerable<Card> source2 = FindCardsWhere((Card c) => c.Identifier == hero.SelectedCard.Identifier && c.Owner.CharacterCards.Contains(c) && (c.PromoIdentifierOrIdentifier.Contains(s) || s.Contains(c.PromoIdentifierOrIdentifier)));
                        source2.Select((Card c) => c.PromoIdentifierOrIdentifier);
                        return source2.Any((Card c) => c.PromoIdentifierOrIdentifier != s);
                    }
                    return true;
                };
                Func<string, bool> turnTakerCriteria2 = (string tt) => tt == hero.SelectedCard.ParentDeck.QualifiedIdentifier;
                coroutine = base.GameController.SelectFromBox(DecisionMaker, identifierCriteria2, turnTakerCriteria2, SelectionType.HeroCharacterCard, storedBox, optional: false, allowMultiCardSelection: false, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                CardController newCC = null;
                SelectFromBoxDecision selection = storedBox.FirstOrDefault();
                TurnTakerController turnTakerController = FindTurnTakerController(hero.SelectedCard.Owner);
                if (selection != null && selection.SelectedIdentifier != null && selection.SelectedTurnTakerIdentifier != null)
                {
                    Log.Debug("Selected from box: SelectedIdentifier = " + selection.SelectedIdentifier);
                    Log.Debug("Selected from box: SelectedTurnTakerIdentifier = " + selection.SelectedTurnTakerIdentifier);
                    Card modelCard = FindCardsWhere((Card c) => c.QualifiedPromoIdentifierOrIdentifier == selection.SelectedIdentifier && !c.Owner.CharacterCards.Contains(c), realCardsOnly: false, null, ignoreBattleZone: true).FirstOrDefault();
                    if (modelCard == null)
                    {
                        Log.Debug("Creating a card for the variant");
                        DeckDefinition parentDeck = hero.SelectedCard.ParentDeck;
                        Log.Debug("Owner deck definition: " + parentDeck);
                        CardDefinition cardDefinition = (from cd in parentDeck.CardDefinitions.Concat(parentDeck.PromoCardDefinitions)
                                                         where cd.QualifiedPromoIdentifierOrIdentifier == selection.SelectedIdentifier
                                                         select cd).FirstOrDefault();
                        if (cardDefinition == null)
                        {
                            cardDefinition = ModHelper.GetPromoDefinition(selection.SelectedTurnTakerIdentifier, selection.SelectedIdentifier);
                        }
                        Log.Debug("Card definition: " + cardDefinition);
                        if (cardDefinition != null)
                        {
                            modelCard = new Card(cardDefinition, base.TurnTaker, 0, selection.UseFoilVersion);
                            base.TurnTaker.OffToTheSide.AddCard(modelCard);
                            Log.Debug("Creating card controller!");
                            string overrideNamespace = selection.SelectedTurnTakerIdentifier;
                            if (!string.IsNullOrEmpty(cardDefinition.Namespace))
                            {
                                overrideNamespace = $"{cardDefinition.Namespace}.{parentDeck.Identifier}";
                            }
                            newCC = CardControllerFactory.CreateInstance(modelCard, base.TurnTakerController, overrideNamespace);
                            base.TurnTakerController.AddCardController(newCC);
                            List<string> list2 = new List<string>();
                            list2.Add(selection.SelectedTurnTakerIdentifier);
                            list2.Add(selection.SelectedIdentifier);
                            base.GameController.AddCardPropertyJournalEntry(modelCard, "OverrideTurnTaker", list2);
                            if (modelCard.SharedIdentifier != null)
                            {
                                IEnumerable<CardDefinition> enumerable2 = from cd in parentDeck.GetAllCardDefinitions()
                                                                          where cd.QualifiedPromoIdentifierOrIdentifier != modelCard.QualifiedPromoIdentifierOrIdentifier && cd.SharedIdentifier == modelCard.SharedIdentifier
                                                                          select cd;
                                if (modelCard.IsPromoCard && modelCard.IsModContent)
                                {
                                    IEnumerable<CardDefinition> second = from cd in ModHelper.GetSharedPromoDefinitions(cardDefinition)
                                                                         where cd.QualifiedPromoIdentifierOrIdentifier != modelCard.QualifiedPromoIdentifierOrIdentifier
                                                                         select cd;
                                    enumerable2 = enumerable2.Concat(second);
                                }
                                foreach (CardDefinition item3 in enumerable2)
                                {
                                    Card card = new Card(item3, base.TurnTaker, 0);
                                    base.TurnTaker.OffToTheSide.AddCard(card);
                                    CardController card2 = CardControllerFactory.CreateInstance(card, base.TurnTakerController, overrideNamespace);
                                    base.TurnTakerController.AddCardController(card2);
                                    List<string> list3 = new List<string>();
                                    list3.Add(modelCard.ParentDeck.QualifiedIdentifier);
                                    list3.Add(card.QualifiedPromoIdentifierOrIdentifier);
                                    base.GameController.AddCardPropertyJournalEntry(card, "OverrideTurnTaker", list3);
                                }
                            }
                        }
                        else
                        {
                            Log.Error("Could not find card definition: " + selection.SelectedIdentifier);
                        }
                    }
                    else
                    {
                        newCC = FindCardController(modelCard);
                    }
                    if (newCC != null)
                    {
                        if (hero.SelectedCard.SharedIdentifier != null)
                        {
                            Log.Debug("Shared identifier: " + hero.SelectedCard.SharedIdentifier);
                            List<Card> list4 = turnTakerController.TurnTaker.OffToTheSide.Cards.Where((Card c) => c.SharedIdentifier == hero.SelectedCard.SharedIdentifier).ToList();
                            foreach (Card otherSize in list4)
                            {
                                Log.Debug("Switching other card: " + otherSize.QualifiedPromoIdentifierOrIdentifier);
                                if (base.GameController.GetCardPropertyJournalEntryStringList(base.Card, "OverrideTurnTaker", supressWarnings: true) == null)
                                {
                                    List<string> list5 = new List<string>();
                                    list5.Add(newCC.Card.ParentDeck.QualifiedIdentifier);
                                    list5.Add(otherSize.QualifiedPromoIdentifierOrIdentifier);
                                    base.GameController.AddCardPropertyJournalEntry(otherSize, "OverrideTurnTaker", list5);
                                }
                                Card card3 = base.TurnTaker.GetCardsWhere((Card c) => c.Identifier == otherSize.Identifier && c.SharedIdentifier == newCC.Card.SharedIdentifier).FirstOrDefault();
                                if (card3 != null)
                                {
                                    IEnumerator coroutine2 = base.GameController.SwitchCards(otherSize, card3, playCardIfMovingToPlayArea: false, ignoreFlipped: false, ignoreHitPoints: false, GetCardSource());
                                    if (base.UseUnityCoroutines)
                                    {
                                        yield return base.GameController.StartCoroutine(coroutine2);
                                    }
                                    else
                                    {
                                        base.GameController.ExhaustCoroutine(coroutine2);
                                    }
                                }
                                else
                                {
                                    Log.Warning("Could not find Guise's copy of the card!");
                                }
                            }
                        }
                        Card cardToMove = hero.SelectedCard;
                        coroutine = base.GameController.SwitchCards(cardToMove, newCC.Card, playCardIfMovingToPlayArea: false, ignoreFlipped: false, ignoreHitPoints: false, GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        if (newCC.TurnTaker.IsPlayer && newCC.Card.HitPoints > newCC.Card.MaximumHitPoints)
                        {
                            newCC.Card.SetHitPoints(newCC.Card.MaximumHitPoints.Value);
                        }
                        if (newCC.Card.IsTarget && !cardToMove.HitPoints.HasValue)
                        {
                            newCC.Card.RemoveTarget();
                        }
                        coroutine = base.GameController.MoveCard(base.TurnTakerController, cardToMove, cardToMove.Owner.InTheBox, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: false, null, base.TurnTaker, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(coroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(coroutine);
                        }
                        cardToMove.PlayIndex = null;
                    }
                }
            }
            // "... then destroy this card."
            IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destructCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destructCoroutine);
            }
        }
    }
}
