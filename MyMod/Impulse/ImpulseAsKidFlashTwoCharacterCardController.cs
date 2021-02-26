using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Impulse
{
    public class ImpulseAsKidFlashTwoCharacterCardController : HeroCharacterCardController
    {
        public ImpulseAsKidFlashTwoCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override IEnumerator UsePower(int index = 0)
        {
            // "The next time another hero would be dealt damage, redirect it to {ImpulseCharacter}. If {ImpulseCharacter} takes damage this way, he regains 2 HP."
            RedirectDamageStatusEffect redirect = new RedirectDamageStatusEffect();
            redirect.CardFlippedExpiryCriteria.Card = base.Card;
            redirect.TargetCriteria.IsHeroCharacterCard = true;
            redirect.TargetCriteria.IsNotSpecificCard = base.Card;
            redirect.RedirectTarget = base.Card;
            redirect.NumberOfUses = 1;
            redirect.FinalTargetRegainsHP = 2;
            IEnumerator redirectCoroutine = AddStatusEffect(redirect);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(redirectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(redirectCoroutine);
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
            IEnumerator playCoroutine = SelectHeroToPlayCard(base.HeroTurnTakerController, heroCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && base.GameController.CanPlayCards(FindTurnTakerController(tt), GetCardSource())));
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
            // "One player draws 2 cards, then discards a card."
            List<SelectTurnTakerDecision> playerChoice = new List<SelectTurnTakerDecision>();
            IEnumerator drawCoroutine = base.GameController.SelectHeroToDrawCards(base.HeroTurnTakerController, 2, optionalSelectHero: false, optionalDrawCards: false, 2, storedResults: playerChoice, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame, "active heroes"), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
            TurnTaker playerChosen = (from d in playerChoice where d.Completed select d.SelectedTurnTaker).FirstOrDefault();
            if (playerChosen != null)
            {
                IEnumerator discardCoroutine = base.GameController.SelectAndDiscardCard(base.GameController.FindTurnTakerController(playerChosen).ToHero(), optional: false, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(discardCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(discardCoroutine);
                }
            }
            yield break;
        }

        private IEnumerator UseIncapOption3()
        {
            // "Destroy a target with 1 HP."
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsTarget && c.HitPoints.Value == 1, "targets with 1 HP", useCardsSuffix: false), optional: false, cardSource: GetCardSource());
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
    }
}
