using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class TorrentCharacterCardController : HeroCharacterCardController
    {
        public TorrentCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowListOfCardsInPlay(new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1));
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int maxTargets = GetPowerNumeral(0, 3);
            int hpValue = GetPowerNumeral(1, 1);
            // "Destroy up to 3 targets with 1 HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == hpValue, "targets with " + hpValue.ToString() + " HP", false, false, "target with " + hpValue.ToString() + " HP", "targets with " + hpValue.ToString() + " HP"), 3, false, 0, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "Draw a card."
            IEnumerator drawCoroutine = DrawCard(base.HeroTurnTaker);
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

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator incapCoroutine;
            switch (index)
            {
                case 0:
                    incapCoroutine = UseIncapOption1();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 1:
                    incapCoroutine = UseIncapOption2();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
                case 2:
                    incapCoroutine = UseIncapOption3();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(incapCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(incapCoroutine);
                    }
                    break;
            }
            yield break;
        }

        private IEnumerator UseIncapOption1()
        {
            // "One player may play a card now."
            IEnumerator playCoroutine = SelectHeroToPlayCard(base.HeroTurnTakerController);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(playCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(playCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption2()
        {
            // "Destroy a target with 1 HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1), false, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "The next time a target is destroyed, 1 hero target regains 3 HP."
            WhenCardIsDestroyedStatusEffect healTrigger = new WhenCardIsDestroyedStatusEffect(base.CardWithoutReplacements, "HealResponse", "The next time a target is destroyed, 1 hero target regains 3 HP.", new TriggerType[] { TriggerType.GainHP }, base.HeroTurnTaker, base.CardWithoutReplacements);
            healTrigger.CardDestroyedCriteria.IsTarget = true;
            healTrigger.NumberOfUses = 1;
            healTrigger.CanEffectStack = false;
            IEnumerator statusCoroutine = AddStatusEffect(healTrigger);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(statusCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(statusCoroutine);
            }
            yield break;
        }

        public IEnumerator HealResponse()
        {
            // "... 1 hero target regains 3 HP."
            IEnumerator healCoroutine = base.GameController.SelectAndGainHP(base.HeroTurnTakerController, 3, false, (Card c) => c.IsHero, 1, 1, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(healCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(healCoroutine);
            }
            yield break;
        }
    }
}
