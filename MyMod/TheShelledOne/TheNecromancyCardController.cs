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
    public class TheNecromancyCardController : CardController
    {
        public TheNecromancyCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowHeroTargetWithLowestHP(ranking: 2);
        }

        public override IEnumerator Play()
        {
            // "Put the top card of each hero trash on top of its deck."
            foreach (TurnTaker item in FindTurnTakersWhere((TurnTaker tt) => !tt.IsIncapacitatedOrOutOfGame && tt.Trash.HasCards && tt.BattleZone == base.Card.BattleZone))
            {
                IEnumerator recycleCoroutine = base.GameController.MoveCard(base.TurnTakerController, item.Trash.TopCard, item.Deck, toBottom: true, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: true, null, null, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(recycleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(recycleCoroutine);
                }
            }
            // "Each hero target deals the hero character with the second lowest HP 2 fire damage."
            IEnumerator incinerateCoroutine = MultipleDamageSourcesDealDamage(new LinqCardCriteria((Card c) => c.IsHero && c.IsTarget), TargetType.LowestHP, 2, new LinqCardCriteria((Card c) => c.IsHeroCharacterCard), 2, DamageType.Fire);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(incinerateCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(incinerateCoroutine);
            }
            yield break;
        }
    }
}
