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
            base.SpecialStringMaker.ShowHeroCharacterCardWithHighestHP();
            base.SpecialStringMaker.ShowHeroCharacterCardWithLowestHP(ranking: 2);
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, the hero with the highest HP deals the hero with the second lowest HP {H - 1} fire damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, UnstableResponse, TriggerType.DealDamage);
        }

        public override IEnumerator Play()
        {
            // "... put the top card of each hero trash on top of its deck."
            foreach (TurnTaker item in FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && tt.Trash.HasCards && tt.BattleZone == base.Card.BattleZone))
            {
                IEnumerator recycleCoroutine = base.GameController.MoveCard(base.TurnTakerController, item.Trash.TopCard, item.Deck, toBottom: false, isPutIntoPlay: false, playCardIfMovingToPlayArea: true, null, showMessage: true, null, null, null, evenIfIndestructible: false, flipFaceDown: false, null, isDiscard: false, evenIfPretendGameOver: false, shuffledTrashIntoDeck: false, doesNotEnterPlay: false, GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(recycleCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(recycleCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator UnstableResponse(GameAction ga)
        {
            // "... the hero with the highest HP deals the hero with the second lowest HP {H - 1} fire damage."
            IEnumerator damageCoroutine = DealDamageToLowestHP(null, 2, (Card c) => c.IsHeroCharacterCard, (Card c) => H - 1, DamageType.Fire, damageSourceInfo: new TargetInfo(HighestLowestHP.HighestHP, 1, 1, new LinqCardCriteria((Card c) => c.IsHeroCharacterCard, "The hero with the highest HP")));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(damageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(damageCoroutine);
            }
            yield break;
        }
    }
}
