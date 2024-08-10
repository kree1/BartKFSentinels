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
    public class FloodingCardController : ExpansionWeatherCardController
    {
        public FloodingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            // "When this card enters play during a player's turn, ..."
            if (base.Game.ActiveTurnTaker.IsPlayer)
            {
                // "... that player may destroy 1 of their Ongoing or Equipment cards."
                List<DestroyCardAction> results = new List<DestroyCardAction>();
                IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.GameController.FindHeroTurnTakerController(base.Game.ActiveTurnTaker.ToHero()), new LinqCardCriteria((Card c) => (IsOngoing(c) || IsEquipment(c)) && c.Owner == base.Game.ActiveTurnTaker, "belonging to " + base.Game.ActiveTurnTaker.Name, useCardsSuffix: false, useCardsPrefix: true, singular: "Ongoing or Equipment card", plural: "Ongoing or Equipment cards"), true, results, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
                // "If they don't, {OwnershipCharacter} deals their hero 2 cold damage."
                if (!DidDestroyCards(results))
                {
                    List<Card> choices = new List<Card>();
                    IEnumerator findCoroutine = FindCharacterCardToTakeDamage(base.Game.ActiveTurnTaker, choices, FindCard(OwnershipIdentifier), 2, DamageType.Cold);
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(findCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(findCoroutine);
                    }
                    Card selected = choices.FirstOrDefault();
                    if (selected != null)
                    {
                        IEnumerator meleeCoroutine = DealDamage(FindCard(OwnershipIdentifier), selected, 2, DamageType.Cold, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(meleeCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(meleeCoroutine);
                        }
                    }
                }
            }
        }
    }
}
