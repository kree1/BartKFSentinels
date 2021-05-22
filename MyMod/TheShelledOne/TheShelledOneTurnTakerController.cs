using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace BartKFSentinels.TheShelledOne
{
    public class TheShelledOneTurnTakerController : TurnTakerController
    {
        public TheShelledOneTurnTakerController(TurnTaker turnTaker, GameController gameController) : base(turnTaker, gameController)
        {

        }

        public override IEnumerator StartGame()
        {
            // "Put {TheShelledOne} into play, 'YOU WILL LEARN DISCIPLINE' side up."
            // already listed under InitialCardIdentifiers in decklist
            // "Put all Pods from the villain deck under this card."
            IEnumerator movePodsCoroutine = base.GameController.MoveCards(this, base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Owner == base.TurnTaker && c.DoKeywordsContain("pod"))), base.CharacterCard.UnderLocation, playIfMovingToPlayArea: false, responsibleTurnTaker: base.TurnTaker, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(movePodsCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(movePodsCoroutine);
            }
            // "Put each Strike and a random Umpire from the villain deck into play."
            // Strikes are already listed under InitialCardIdentifiers in decklist
            IEnumerable<Card> umpires = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.Owner == base.TurnTaker && c.DoKeywordsContain("umpire")));
            Card umpireAssigned = umpires.ElementAt(base.GameController.Game.RNG.Next(0, umpires.Count()));
            IEnumerator fieldUmpireCoroutine = base.GameController.PlayCard(this, umpireAssigned, isPutIntoPlay: true, responsibleTurnTaker: base.TurnTaker, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(fieldUmpireCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(fieldUmpireCoroutine);
            }
            // "Set each Strike's HP to 0."
            IEnumerator setHPCoroutine = base.GameController.SetHP(base.FindDecisionMaker(), (Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain("strike"), (Card c) => 0, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(setHPCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(setHPCoroutine);
            }
            // "Shuffle the villain deck."
            IEnumerator shuffleCoroutine = base.GameController.ShuffleLocation(base.TurnTaker.Deck, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(shuffleCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(shuffleCoroutine);
            }

            // "Villain Ongoing cards and environment cards with no printed HP or a printed HP of less than 6 have a maximum HP of 6."
            IEnumerable<Card> getsHP = base.GameController.FindCardsWhere(new LinqCardCriteria((Card c) => (base.CharacterCardController as TheShelledOneCharacterCardController).getsMaxHPSet(c)));
            IEnumerator targetCoroutine = base.GameController.MakeTargettable(base.FindDecisionMaker(), (Card c) => getsHP.Contains(c), (Card c) => 6, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(targetCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(targetCoroutine);
            }
            foreach (Card c in getsHP)
            {
                IEnumerator setCoroutine = base.GameController.SetHP(c, c.MaximumHitPoints.Value, cardSource: base.GameController.FindCardController(base.TurnTaker.CharacterCard).GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(setCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(setCoroutine);
                }
            }
            yield break;
        }
    }
}
