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
    public class IdolatryCardController : StrikeCardController
    {
        public IdolatryCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            base.AddAsPowerContributor();
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of the villain turn, if this card has {H * 2} or more HP, put a token on {TheShelledOne} and set this card's HP to 0."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker && base.Card.HitPoints.HasValue && base.Card.HitPoints.Value >= H * 2, AddTokenAndResetResponse, new TriggerType[] { TriggerType.AddTokensToPool, TriggerType.GainHP });
            // "Whenever a player causes another player to draw a card, play a card, or use a power, this card regains 1 HP."

            AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardSource != null && pca.CardSource.Card.Owner.IsHero && pca.DecisionMaker.IsHero && pca.ResponsibleTurnTaker.IsHero && pca.DecisionMaker.TurnTaker != pca.ResponsibleTurnTaker, (PlayCardAction pc) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            AddTrigger<UsePowerAction>((UsePowerAction upa) => upa.CardSource != null && upa.CardSource.Card.Owner.IsHero && upa.CardSource.Card.Owner != upa.HeroUsingPower.TurnTaker, (UsePowerAction p) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            AddTrigger<DrawCardAction>((DrawCardAction dca) => dca.CardSource != null && dca.CardSource.Card.Owner.IsHero && dca.CardSource.Card.Owner != dca.HeroTurnTaker, (DrawCardAction dc) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            AddTrigger((PlayCardAction pc) => base.GameController.ActiveTurnPhase.IsPlayCard && pc.Origin.OwnerTurnTaker.IsHero && pc.Origin.OwnerTurnTaker == base.GameController.ActiveTurnTakerController.TurnTaker && base.GameController.ActiveTurnPhase.PhaseActionCountUsed > 0 && base.GameController.ActiveTurnPhase.PhaseActionCountUsed <= (from cc in base.GameController.GetCardsIncreasingPhaseActionCount()
                                                                                                                                                                                                                                                                                                                                                       where cc.Card.Owner != pc.Origin.OwnerTurnTaker && cc.Card.Owner.IsHero
                                                                                                                                                                                                                                                                                                                                                       select cc).Count(), (PlayCardAction p) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            AddTrigger((UsePowerAction p) => base.GameController.ActiveTurnPhase.IsUsePower && p.Power.TurnTakerController.IsHero && p.Power.TurnTakerController == base.GameController.ActiveTurnTakerController && base.GameController.ActiveTurnPhase.PhaseActionCountUsed > 0 && base.GameController.ActiveTurnPhase.PhaseActionCountUsed <= (from cc in base.GameController.GetCardsIncreasingPhaseActionCount()
                                                                                                                                                                                                                                                                                                                                                  where cc.Card.Owner != p.Power.TurnTakerController.TurnTaker && cc.Card.Owner.IsHero
                                                                                                                                                                                                                                                                                                                                                  select cc).Count(), (UsePowerAction p) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
            AddTrigger((DrawCardAction dc) => base.GameController.ActiveTurnPhase.IsDrawCard && base.GameController.ActiveTurnPhase.PhaseActionCountUsed > 0 && base.GameController.ActiveTurnPhase.PhaseActionCountUsed <= (from cc in base.GameController.GetCardsIncreasingPhaseActionCount()
                                                                                                                                                                                                                             where cc.Card.Owner != dc.HeroTurnTaker && cc.Card.Owner.IsHero
                                                                                                                                                                                                                             select cc).Count(), (DrawCardAction p) => base.GameController.GainHP(base.Card, 1, cardSource: GetCardSource()), TriggerType.GainHP, TriggerTiming.After);
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            // "Heroes gain the following power:"
            if (cardController.HeroTurnTakerController != null && cardController.Card.IsHeroCharacterCard && cardController.Card.Owner.IsHero && !cardController.Card.Owner.ToHero().IsIncapacitatedOrOutOfGame && !cardController.Card.IsFlipped && cardController.Card.IsRealCard)
            {
                // Power: "Destroy 1 of your non-character cards. Another player draws 2 cards."
                Power idolPower = new Power(cardController.HeroTurnTakerController, cardController, "Destroy 1 of your non-character cards. Another player draws 2 cards.", this.UsePowerResponse(cardController), 0, null, GetCardSource());
                return new Power[] { idolPower };
            }
            return null;
        }

        public IEnumerator UsePowerResponse(CardController cardController)
        {
            //  "Destroy 1 of your non-character cards."
            TurnTaker player = cardController.TurnTaker;
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.GameController.FindHeroTurnTakerController(player.ToHero()), new LinqCardCriteria((Card c) => c.Owner == player && !c.IsCharacter && c.IsInPlay, "non-character cards belonging to " + player.Name, false, false, "non-character card belonging to " + player.Name, "non-character cards belonging to " + player.Name), false, responsibleCard: cardController.Card, cardSource: cardController.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Another player draws 2 cards."
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCards(base.GameController.FindHeroTurnTakerController(player.ToHero()), numberOfCards: 2, optionalDrawCards: false, requiredDraws: 2, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && tt != player), cardSource: cardController.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            yield break;
        }
    }
}
