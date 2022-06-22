using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public abstract class RenownCardController : MemorialUtilityCardController
    {
        public RenownCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowSpecialString(() => "This card is " + base.Card.Location.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
            SpecialStringMaker.ShowSpecialString(() => "All hero characters in " + base.Card.Location.OwnerTurnTaker.Name + "'s play area are Renowned.", null, () => GameController.FindCardsWhere(new LinqCardCriteria((Card c) => c.IsHeroCharacterCard && c.Location.HighestRecursiveLocation == base.Card.Location.HighestRecursiveLocation), visibleToCard: GetCardSource())).Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        private bool _foundHero;

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            if (base.CharacterCard.IsFlipped)
            {
                // "When a villain Renown enters play, move it next to a non-Renowned hero character target."
                if (NonRenownedHeroCharacterTargets().Count() == 0)
                {
                    storedResults.Add(new MoveCardDestination(base.TurnTaker.Trash));
                    _foundHero = false;
                    yield break;
                }
                IEnumerator selectCoroutine = SelectCardThisCardWillMoveNextTo(IsNonRenownedHeroCharacterTarget(), storedResults, isPutIntoPlay, decisionSources);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(selectCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(selectCoroutine);
                }
            }
            else
            {
                yield return base.DeterminePlayLocation(storedResults, isPutIntoPlay, decisionSources, overridePlayArea, additionalTurnTakerCriteria);
            }
        }

        public override IEnumerator RunIfUnableToEnterPlay()
        {
            if (base.CharacterCard.IsFlipped)
            {
                // "... If there are none, discard it and play the top card of the villain deck."
                if (!_foundHero && NonRenownedHeroCharacterTargets().Count() == 0)
                {
                    if (base.TurnTaker.Deck.Cards.Any((Card c) => !IsRenown(c)) || base.TurnTaker.Trash.Cards.Any((Card c) => !IsRenown(c)))
                    {
                        IEnumerator messageCoroutine = base.GameController.SendMessageAction("All hero character targets are already Renowned! Moving this card to the villain trash and playing the top card of the villain deck...", Priority.High, GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(messageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(messageCoroutine);
                        }
                        IEnumerator playCoroutine = PlayTheTopCardOfTheVillainDeckResponse(null);
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
                        IEnumerator messageCoroutine = base.GameController.SendMessageAction("All hero character targets are already Renowned, and there are no non-Renown cards in the villain deck or trash to play.", Priority.High, GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return base.GameController.StartCoroutine(messageCoroutine);
                        }
                        else
                        {
                            base.GameController.ExhaustCoroutine(messageCoroutine);
                        }
                    }
                }
            }
            else
            {
                yield return base.RunIfUnableToEnterPlay();
            }
        }

        public override IEnumerator Play()
        {
            if (base.CharacterCard.IsFlipped)
            {
                if (!_foundHero && NonRenownedHeroCharacterTargets().Count() == 0)
                {
                    // "... If there are none, discard it and play the top card of the villain deck."
                    IEnumerator runCoroutine = RunIfUnableToEnterPlay();
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(runCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(runCoroutine);
                    }
                }
            }
            // "When this card enters play, {Memorial} deals this hero 2 projectile damage."
            if (_foundHero)
            {
                Card thisHero = GetCardThisCardIsNextTo();
                if (thisHero != null)
                {
                    IEnumerator damageCoroutine = DealDamage(base.CharacterCard, thisHero, 2, DamageType.Projectile, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(damageCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(damageCoroutine);
                    }
                }
            }
            yield break;
        }
    }
}
