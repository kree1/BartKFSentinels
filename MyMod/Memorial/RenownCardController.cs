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
            //Log.Debug(Card.Title + ".DeterminePlayLocation() started");
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
                _foundHero = true;
            }
            else
            {
                yield return base.DeterminePlayLocation(storedResults, isPutIntoPlay, decisionSources, overridePlayArea, additionalTurnTakerCriteria);
            }
            //Log.Debug(Card.Title + ".DeterminePlayLocation() finished");
        }

        public override IEnumerator RunIfUnableToEnterPlay()
        {
            //Log.Debug(Card.Title + ".RunIfUnableToEnterPlay() started");
            if (FindCardController(CharacterCard) is MemorialUtilityCharacterCardController)
            {
                IEnumerator runCoroutine = ((MemorialUtilityCharacterCardController)FindCardController(base.CharacterCard)).ExtraRenownResponse(Card);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(runCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(runCoroutine);
                }
                //Log.Debug(Card.Title + ".RunIfUnableToEnterPlay() finished");
            }
            else
            {
                //Log.Debug(Card.Title + ".RunIfUnableToEnterPlay() handing off to base class");
                yield return base.RunIfUnableToEnterPlay();
            }
        }

        public override IEnumerator Play()
        {
            //Log.Debug(Card.Title + ".Play() started");
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

            // "When this card enters play, {Memorial} deals this hero 2 projectile damage."
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
            //Log.Debug(Card.Title + ".Play() finished");
            yield break;
        }
    }
}
