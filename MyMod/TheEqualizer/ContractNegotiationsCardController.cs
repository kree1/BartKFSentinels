using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.TheEqualizer
{
    public class ContractNegotiationsCardController : EqualizerUtilityCardController
    {
        public ContractNegotiationsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Show hero with second highest HP
            SpecialStringMaker.ShowHeroCharacterCardWithHighestHP(2);
            // Show list of players with >=1 hero character target and 0 Ongoing and/or Equipment cards in play?
            // ...
        }

        public const string ObjectiveIdentifier = "LucrativeContract";

        public override IEnumerator Play()
        {
            // "The hero with the second highest HP may destroy all of their Ongoing and Equipment cards."
            List<Card> results = new List<Card>();
            IEnumerator findCoroutine = GameController.FindTargetWithHighestHitPoints(2, (Card c) => IsHeroCharacterCard(c), results, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(findCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(findCoroutine);
            }
            Card secondHighest = results.FirstOrDefault();
            if (secondHighest != null && secondHighest.Owner.IsPlayer)
            {
                List<Card> toDestroy = GameController.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Owner == secondHighest.Owner && (IsOngoing(c) || IsEquipment(c))).ToList();
                if (toDestroy.Any())
                {
                    YesNoDecision decision = new YesNoDecision(GameController, GameController.FindHeroTurnTakerController(secondHighest.Owner.ToHero()), SelectionType.Custom, associatedCards: toDestroy, cardSource: GetCardSource());
                    IEnumerator decideCoroutine = GameController.MakeDecisionAction(decision);
                    if (base.UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(decideCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(decideCoroutine);
                    }
                    if (decision != null && decision.Answer.HasValue && decision.Answer.Value)
                    {
                        IEnumerator destroyCoroutine = GameController.DestroyCards(GameController.FindHeroTurnTakerController(secondHighest.Owner.ToHero()), new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.Owner == secondHighest.Owner && (IsOngoing(c) || IsEquipment(c)), "belonging to " + secondHighest.Owner.Name, useCardsPrefix: true, useCardsSuffix: false, singular: "Ongoing or Equipment card", plural: "Ongoing or Equipment cards"), cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(destroyCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(destroyCoroutine);
                        }
                    }
                }
            }
            // "One player with no Ongoing or Equipment cards in play may move the Objective card next to one of their hero character targets."
            List<TurnTaker> eligible = GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsPlayer && !GameController.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.Owner == tt && (IsOngoing(c) || IsEquipment(c))).Any() && GameController.FindCardsWhere((Card c) => IsHeroCharacterCard(c) && c.IsTarget && c.Owner == tt).Any()).ToList();
            if (eligible.Any())
            {
                List<Card> options = GameController.FindCardsWhere((Card c) => IsHeroCharacterCard(c) && c.IsTarget && eligible.Contains(c.Owner)).ToList();
                if (options.Any())
                {
                    SelectCardDecision selection = new SelectCardDecision(GameController, DecisionMaker, SelectionType.MoveCardNextToCard, options, isOptional: true, cardSource: GetCardSource(), associatedCards: FindCard(ObjectiveIdentifier).ToEnumerable());
                    IEnumerator selectCoroutine = GameController.MakeDecisionAction(selection);
                    if (base.UseUnityCoroutines)
                    {
                        yield return GameController.StartCoroutine(selectCoroutine);
                    }
                    else
                    {
                        GameController.ExhaustCoroutine(selectCoroutine);
                    }
                    if (selection.Completed)
                    {
                        IEnumerator moveCoroutine = GameController.MoveCard(TurnTakerController, FindCard(ObjectiveIdentifier), selection.SelectedCard.NextToLocation, showMessage: true, responsibleTurnTaker: selection.SelectedCard.Owner, evenIfIndestructible: true, cardSource: GetCardSource());
                        if (base.UseUnityCoroutines)
                        {
                            yield return GameController.StartCoroutine(moveCoroutine);
                        }
                        else
                        {
                            GameController.ExhaustCoroutine(moveCoroutine);
                        }
                    }
                }
            }
            // "{TheEqualizer} deals the [b][i]Marked[/i][/b] target 3 projectile damage."
            IEnumerator shootMarkedCoroutine = DealDamage(CharacterCard, ettc.MarkedTarget(GetCardSource()), 3, DamageType.Projectile, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(shootMarkedCoroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(shootMarkedCoroutine);
            }
        }
    }
}
