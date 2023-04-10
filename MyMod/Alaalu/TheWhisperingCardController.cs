using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class TheWhisperingCardController : CardController
    {
        public TheWhisperingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the environment turn, play the top card of the environment deck."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayTheTopCardOfTheEnvironmentDeckResponse, TriggerType.PlayCard);
            // "At the start of the environment turn, put the top card of a hero trash into play. If that card is a One-Shot, deal the associated hero (H) minus 1 psychic damage and destroy this card."
            AddStartOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PutIntoPlayFromTrashResponse, new TriggerType[] { TriggerType.PutIntoPlay, TriggerType.DealDamage, TriggerType.DestroySelf });
        }

        public IEnumerator PutIntoPlayFromTrashResponse(GameAction ga)
        {
            // "... put the top card of a hero trash into play."
            List<PlayCardAction> played = new List<PlayCardAction>();
            IEnumerator putCoroutine = base.GameController.SelectAndPlayCard(DecisionMaker, (Card c) => c.Location.IsTrash && c.Location.IsHero && base.GameController.IsLocationVisibleToSource(c.Location, GetCardSource()) && c == c.Location.TopCard, isPutIntoPlay: true, cardSource: GetCardSource(), storedResults: played);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(putCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(putCoroutine);
            }
            // "If that card is a One-Shot..."
            if (DidPlayCards(played))
            {
                PlayCardAction successful = played.FirstOrDefault((PlayCardAction pca) => pca.WasCardPlayed);
                if (successful != null && successful.CardToPlay != null && successful.CardToPlay.DoKeywordsContain("one-shot"))
                {
                    // "... deal the associated hero (H) minus 1 psychic damage..."
                    if (IsHero(successful.CardToPlay))
                    {
                        List<Card> targets = new List<Card>();
                        if (base.GameController.FindHeroTurnTakerController(successful.CardToPlay.Owner.ToHero()).HasMultipleCharacterCards)
                        {
                            IEnumerator findCoroutine = FindCharacterCardToTakeDamage(successful.CardToPlay.Owner, targets, base.Card, H - 1, DamageType.Psychic);
                            if (base.UseUnityCoroutines)
                            {
                                yield return base.GameController.StartCoroutine(findCoroutine);
                            }
                            else
                            {
                                base.GameController.ExhaustCoroutine(findCoroutine);
                            }
                        }
                        else
                        {
                            targets.Add(successful.CardToPlay.Owner.CharacterCard);
                        }
                        IEnumerator damageCoroutine = base.GameController.DealDamage(DecisionMaker, base.Card, (Card c) => targets.Contains(c), H - 1, DamageType.Psychic, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(damageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(damageCoroutine);
                        }
                    }
                    // "... and destroy this card."
                    IEnumerator destructCoroutine = base.GameController.DestroyCard(DecisionMaker, base.Card, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
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
            yield break;
        }
    }
}
