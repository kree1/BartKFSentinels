using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheShelledOne
{
    public class FeedbackCardController : BlaseballWeatherCardController
    {
        public FeedbackCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            AddThisCardControllerToList(CardControllerListType.ReplacesCards);
            AddThisCardControllerToList(CardControllerListType.ReplacesTurnTakerController);
			SpecialStringMaker.ShowHeroCharacterCardWithHighestHP();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of the villain turn, destroy {H - 1} Equipment cards. If fewer than {H - 1} cards were destroyed this way, replace the hero character card with the highest HP with a variant of that hero."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DestroyReplaceResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.MoveCard });
        }

        public IEnumerator DestroyReplaceResponse(GameAction ga)
        {
            // "... destroy {H - 1} Equipment cards."
            List<DestroyCardAction> destroyed = new List<DestroyCardAction>();
            LinqCardCriteria equipmentInPlay = new LinqCardCriteria((Card c) => c.DoKeywordsContain("equipment") && c.IsInPlayAndHasGameText && base.GameController.IsCardVisibleToCardSource(c, GetCardSource()), "Equipment");
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(DecisionMaker, equipmentInPlay, H - 1, requiredDecisions: H - 1, storedResultsAction: destroyed, responsibleCard: base.Card, allowAutoDecide: base.GameController.FindCardsWhere(equipmentInPlay).Count() <= H - 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If fewer than {H - 1} cards were destroyed this way, replace the hero character card with the highest HP with a variant of that hero."
            if (destroyed.Where((DestroyCardAction dca) => dca.WasCardDestroyed).Count() < H - 1)
            {
                List<Card> highest = new List<Card>();
                IEnumerator findCoroutine = base.GameController.FindTargetWithHighestHitPoints(1, (Card c) => c.IsHeroCharacterCard, highest, evenIfCannotDealDamage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                Card highestHero = highest.FirstOrDefault();
                if (highestHero != null)
                {
					// Replace highestHero with another variant
					// Copy-pasted from Completionist Guise, be careful
					// If you selected a hero to store...
					List<string> list = new List<string>();
					list.Add(highestHero.ParentDeck.QualifiedIdentifier);
					list.Add(highestHero.QualifiedPromoIdentifierOrIdentifier);
					base.GameController.AddCardPropertyJournalEntry(highestHero, "OverrideTurnTaker", list);
					List<SelectFromBoxDecision> storedBox = new List<SelectFromBoxDecision>();
					Func<string, bool> identifierCriteria2 = delegate (string s)
					{
						if (FindCardsWhere((Card c) => (c.Identifier == highestHero.Identifier || (highestHero.SharedIdentifier != null && highestHero.SharedIdentifier == c.SharedIdentifier)) && c.QualifiedPromoIdentifierOrIdentifier == s && c.Owner.CharacterCards.Contains(c)).Any())
						{
							return false;
						}
						if (highestHero.SharedIdentifier != null)
						{
							string identifier = highestHero.Identifier;
							CardDefinition cardDefinition2 = highestHero.ParentDeck.GetAllCardDefinitions().FirstOrDefault((CardDefinition d) => d.QualifiedPromoIdentifierOrIdentifier == s);
							if (cardDefinition2 == null)
							{
								cardDefinition2 = ModHelper.GetPromoDefinition(highestHero.ParentDeck.QualifiedIdentifier, s);
							}
							if (cardDefinition2 != null)
							{
								return cardDefinition2.Identifier == identifier;
							}
							return false;
						}
						if (highestHero.ParentDeck.InitialCardIdentifiers.Count() > 1)
						{
							IEnumerable<Card> source = FindCardsWhere((Card c) => c.Identifier == highestHero.Identifier && c.Owner.CharacterCards.Contains(c) && (c.PromoIdentifierOrIdentifier.Contains(s) || s.Contains(c.PromoIdentifierOrIdentifier)));
							source.Select((Card c) => c.PromoIdentifierOrIdentifier);
							return source.Any((Card c) => c.PromoIdentifierOrIdentifier != s);
						}
						return true;
					};
					Func<string, bool> turnTakerCriteria2 = (string tt) => tt == highestHero.ParentDeck.QualifiedIdentifier;
					// Choose replacement hero
					IEnumerator coroutine = base.GameController.SelectFromBox(DecisionMaker, identifierCriteria2, turnTakerCriteria2, SelectionType.HeroCharacterCard, storedBox, optional: false, allowMultiCardSelection: false, GetCardSource());
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
					TurnTakerController turnTakerController = FindTurnTakerController(highestHero.Owner);
					if (selection != null && selection.SelectedIdentifier != null && selection.SelectedTurnTakerIdentifier != null)
					{
						Log.Debug("Selected from box: SelectedIdentifier = " + selection.SelectedIdentifier);
						Log.Debug("Selected from box: SelectedTurnTakerIdentifier = " + selection.SelectedTurnTakerIdentifier);
						Card modelCard = FindCardsWhere((Card c) => c.QualifiedPromoIdentifierOrIdentifier == selection.SelectedIdentifier && !c.Owner.CharacterCards.Contains(c), realCardsOnly: false, null, ignoreBattleZone: true).FirstOrDefault();
						if (modelCard == null)
						{
							Log.Debug("Creating a card for the variant");
							DeckDefinition parentDeck = highestHero.ParentDeck;
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
								// modelCard: the new card being created
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
									// enumerable2: all cards from parentDeck that match modelCard's SharedIdentifier but not its QualifiedPromoIdentifierOrIdentifier
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
							if (highestHero.SharedIdentifier != null)
							{
								Log.Debug("Shared identifier: " + highestHero.SharedIdentifier);
								List<Card> list4 = turnTakerController.TurnTaker.OffToTheSide.Cards.Where((Card c) => c.SharedIdentifier == highestHero.SharedIdentifier).ToList();
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
							coroutine = base.GameController.SwitchCards(highestHero, newCC.Card, playCardIfMovingToPlayArea: false, ignoreFlipped: false, ignoreHitPoints: false, GetCardSource());
							if (base.UseUnityCoroutines)
							{
								yield return base.GameController.StartCoroutine(coroutine);
							}
							else
							{
								base.GameController.ExhaustCoroutine(coroutine);
							}
							if (newCC.TurnTaker.IsHero && newCC.Card.HitPoints > newCC.Card.MaximumHitPoints)
							{
								newCC.Card.SetHitPoints(newCC.Card.MaximumHitPoints.Value);
							}
							if (newCC.Card.IsTarget && !highestHero.HitPoints.HasValue)
							{
								newCC.Card.RemoveTarget();
							}
							Card cardToMove = highestHero;
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
					// ??? SHOULD work

					IEnumerator messageCoroutine = base.GameController.SendMessageAction("Reality flickers in the feedback. Things look different...", Priority.Medium, GetCardSource(), showCardSource: true);
					if (base.UseUnityCoroutines)
					{
						yield return base.GameController.StartCoroutine(messageCoroutine);
					}
					else
					{
						base.GameController.ExhaustCoroutine(messageCoroutine);
					}
				}
            }
            yield break;
        }
    }
}
