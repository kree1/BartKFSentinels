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
            AddTrigger<PlayCardAction>((PlayCardAction pca) => pca.CardToPlay != null && pca.IsSuccessful && pca.CardToPlay.Owner.IsHero && pca.CardToPlay.IsHero, GainHPResponse, TriggerType.GainHP, TriggerTiming.After);
            // "When {Momentum} flips to its "Under Pressure" side, destroy this card and play the top card of the villain deck."
            AddTrigger((FlipCardAction fca) => fca.CardToFlip.Card == base.TurnTaker.FindCard("MomentumCharacter") && !fca.ToFaceDown, SelfDestructResponse, new TriggerType[] { TriggerType.DestroySelf, TriggerType.PlayCard }, TriggerTiming.After);
        }

        public override IEnumerator Play()
        {
            yield break;
        }

        private IEnumerator GainHPResponse(PlayCardAction pca)
        {
            // "Whenever a player plays a hero card, if it's a One-Shot, their hero regains 2 HP. If not, 1 non-Terrain villain target with less than its maximum HP regains 2 HP."
            if (pca.WasCardPlayed)
            {
                if (pca.CardToPlay.DoKeywordsContain("one-shot"))
                {
                    // "... their hero regains 2 HP."
                    HeroTurnTakerController playing = pca.TurnTakerController.ToHero();
                    TurnTaker player = pca.TurnTakerController.TurnTaker;
                    List<Card> resultsHeroCharacter = new List<Card>();

                    // The player chooses one of their hero characters to regain HP (stored in resultsHeroCharacter)
                    IEnumerator chooseHeroCoroutine = base.FindCharacterCard(player, SelectionType.GainHP, resultsHeroCharacter);
                    if (base.UseUnityCoroutines)
                    {
                        yield return this.GameController.StartCoroutine(chooseHeroCoroutine);
                    }
                    else
                    {
                        this.GameController.ExhaustCoroutine(chooseHeroCoroutine);
                    }

                    // That hero regains 2 HP
                    IEnumerator healHeroCoroutine = base.GameController.GainHP(resultsHeroCharacter.First(), 2, cardSource: GetCardSource());
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
                    List<Card> resultsVillainTarget = new List<Card>();
                    LinqCardCriteria req = new LinqCardCriteria((Card c) => c.IsVillainTarget && c.HitPoints < c.MaximumHitPoints);
                    IEnumerator healVillainCoroutine = base.GameController.SelectAndGainHP(base.DecisionMaker, 2, additionalCriteria: (Card c) => c.IsVillainTarget && c.HitPoints < c.MaximumHitPoints, numberOfTargets: 1, cardSource: GetCardSource());
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
            yield break;
        }

        private IEnumerator SelfDestructResponse(FlipCardAction fca)
        {
            // "When {Momentum} flips to its "Under Pressure" side, destroy this card and play the top card of the villain deck."
            IEnumerator destroyCoroutine = base.DestroyThisCardResponse(fca);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(destroyCoroutine);
            }

            IEnumerator playCoroutine = base.PlayTheTopCardOfTheVillainDeckResponse(fca);
            if (base.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(playCoroutine);
            }

            yield break;
        }
    }
}
