using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Breakaway
{
    public class DownhillRunCardController : CardController
    {
        public DownhillRunCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "Whenever a player plays a hero card, if it's a One-Shot, their hero regains 2 HP. If not, 1 non-Terrain villain target with less than its maximum HP regains 2 HP."
            AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardToPlay != null && pca.IsSuccessful && IsHero(pca.ResponsibleTurnTaker) && IsHero(pca.CardToPlay) && !pca.IsPutIntoPlay, GainHPResponse, TriggerType.GainHP, TriggerTiming.After);
            // "When {Momentum} flips to its "Under Pressure" side, destroy this card and play the top card of the villain deck."
            AddTrigger((FlipCardAction fca) => fca.CardToFlip.Card == base.TurnTaker.FindCard("MomentumCharacter") && !fca.ToFaceDown, SelfDestructResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard }, TriggerTiming.After);
        }

        private IEnumerator GainHPResponse(PlayCardAction pca)
        {
            // "Whenever a player plays a hero card, if it's a One-Shot, their hero regains 2 HP. If not, 1 non-Terrain villain target with less than its maximum HP regains 2 HP."
            if (pca.WasCardPlayed)
            {
                if (pca.CardToPlay.IsOneShot)
                {
                    // "... their hero regains 2 HP."
                    HeroTurnTakerController playing = pca.TurnTakerController.ToHero();

                    // The player chooses one of their hero characters to regain HP
                    IEnumerator healHeroCoroutine = base.GameController.SelectAndGainHP(playing, 2, additionalCriteria: (Card c) => IsHeroCharacterCard(c) && c.Owner == playing.TurnTaker, requiredDecisions: 1, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(healHeroCoroutine);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(healHeroCoroutine);
                    }
                }
                else
                {
                    // "... 1 non-Terrain villain target with less than its maximum HP regains 2 HP."
                    IEnumerator healVillainCoroutine = base.GameController.SelectAndGainHP(base.DecisionMaker, 2, additionalCriteria: (Card c) => IsVillainTarget(c) && c.HitPoints < c.MaximumHitPoints && !c.DoKeywordsContain("terrain"), numberOfTargets: 1, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(healVillainCoroutine);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(healVillainCoroutine);
                    }
                }
            }
        }

        private IEnumerator SelfDestructResponse(FlipCardAction fca)
        {
            // "When {Momentum} flips to its "Under Pressure" side, destroy this card and play the top card of the villain deck."
            IEnumerator destroyCoroutine = base.GameController.DestroyCard(this.DecisionMaker, this.Card, optional: false, postDestroyAction: () => base.PlayTheTopCardOfTheVillainDeckResponse(fca), actionSource: fca, responsibleCard: this.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(destroyCoroutine);
            }
        }
    }
}
