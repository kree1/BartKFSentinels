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
    public class NewMeganItoIVCardController : CardController
    {
        public NewMeganItoIVCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the start of each hero turn, discard the top card of that player's deck. If the discarded card is a One-Shot, play it. Otherwise, this card deals a hero character in that play area 2 projectile damage."
            AddStartOfTurnTrigger((TurnTaker tt) => IsHero(tt), PitchResponse, new TriggerType[] { TriggerType.DiscardCard, TriggerType.PlayCard, TriggerType.DealDamage });
        }

        public IEnumerator PitchResponse(PhaseChangeAction pca)
        {
            // "... discard the top card of that player's deck."
            List<MoveCardAction> results = new List<MoveCardAction>();
            IEnumerator discardCoroutine = base.GameController.DiscardTopCard(base.Game.ActiveTurnTaker.Deck, results, (Card c) => true, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(discardCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(discardCoroutine);
            }
            MoveCardAction important = results.FirstOrDefault((MoveCardAction mca) => mca.WasCardMoved);
            if (important != null)
            {
                // "If the discarded card is a One-Shot, play it."
                Card discarded = important.CardToMove;
                if (discarded.IsOneShot)
                {
                    IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, discarded, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(playCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(playCoroutine);
                    }
                }
                else
                {
                    // "Otherwise, this card deals a hero character in that play area 2 projectile damage."
                    IEnumerator projectileCoroutine = base.GameController.SelectTargetsAndDealDamage(DecisionMaker, new DamageSource(base.GameController, base.Card), 2, DamageType.Projectile, 1, false, 1, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Location.IsPlayAreaOf(base.Game.ActiveTurnTaker), cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(projectileCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(projectileCoroutine);
                    }
                }
            }
        }
    }
}
