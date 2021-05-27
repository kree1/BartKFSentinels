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
    public class AxelTrolololCardController : CardController
    {
        public AxelTrolololCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, each player discards the top card of their deck in turn order. If the discarded card is a One-Shot, that player's hero may use a power now. Otherwise, this card deals that player's hero 2 projectile damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, DiscardDamageOrPowerResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.UsePower, TriggerType.DealDamage });
        }

        public IEnumerator DiscardDamageOrPowerResponse(GameAction ga)
        {
            // "... each player discards the top card of their deck in turn order."
            IEnumerable<HeroTurnTaker> heroDeckOwners = base.GameController.AllHeroes;
            foreach (HeroTurnTaker player in heroDeckOwners)
            {
                List<MoveCardAction> moveResults = new List<MoveCardAction>();
                IEnumerator discardCoroutine = base.GameController.DiscardTopCard(player.Deck, moveResults, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
                MoveCardAction discard = moveResults.FirstOrDefault();
                if (discard != null && discard.CardToMove != null)
                {
                    // "... If the discarded card is a One-Shot, that player's hero may use a power now. Otherwise, this card deals that player's hero 2 projectile damage."
                    Card discarded = discard.CardToMove;
                    if (discarded.DoKeywordsContain("one-shot"))
                    {
                        // "... that player's hero may use a power now."
                        IEnumerator powerCoroutine = base.GameController.SelectHeroToUsePower(DecisionMaker, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt == player), cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(powerCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(powerCoroutine);
                        }
                    }
                    else
                    {
                        // "... this card deals that player's hero 2 projectile damage."
                        List<Card> storedCharacter = new List<Card>();
                        IEnumerator findCoroutine = FindCharacterCardToTakeDamage(player, storedCharacter, base.Card, 2, DamageType.Projectile);
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(findCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(findCoroutine);
                        }
                        if (storedCharacter != null && storedCharacter.FirstOrDefault() != null)
                        {
                            Card character = storedCharacter.FirstOrDefault();
                            IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => c == character, 2, DamageType.Projectile, cardSource: GetCardSource());
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(damageCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(damageCoroutine);
                            }
                        }
                    }
                }
            }
            yield break;
        }
    }
}
