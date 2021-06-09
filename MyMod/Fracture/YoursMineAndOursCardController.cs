using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Fracture
{
    public class YoursMineAndOursCardController : FractureUtilityCardController
    {
        public YoursMineAndOursCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public Guid? DestroyToAct { get; set; }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // "At the end of your turn, you may destroy this card. If you do, one other player may draw a card, play a card, and use a power in any order, then their hero deals themself 3 psychic damage."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, (PhaseChangeAction pca) => DestructChoiceResponse(pca), new TriggerType[] { TriggerType.DestroySelf });
            //AddTrigger((DestroyCardAction dca) => dca.CardToDestroy.Card == base.Card && dca.CardSource != null && dca.CardSource.Card == base.Card, (DestroyCardAction dca) => ChoosePlayerToActResponse(), new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard, TriggerType.UsePower, TriggerType.DealDamage }, TriggerTiming.After);
            AddWhenDestroyedTrigger((DestroyCardAction dca) => ChoosePlayerToActResponse(dca), new TriggerType[] { TriggerType.DrawCard, TriggerType.PlayCard, TriggerType.UsePower, TriggerType.DealDamage });
        }

        public IEnumerator DestructChoiceResponse(GameAction ga)
        {
            // "... you may destroy this card. If you do, ..."
            if (!DestroyToAct.HasValue || DestroyToAct.Value != ga.InstanceIdentifier)
            {
                List<YesNoCardDecision> choice = new List<YesNoCardDecision>();
                IEnumerator decideCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.DestroyCard, base.Card, storedResults: choice, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(decideCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(decideCoroutine);
                }
                if (DidPlayerAnswerYes(choice))
                {
                    DestroyToAct = ga.InstanceIdentifier;
                }
            }
            if (DestroyToAct.HasValue && DestroyToAct.Value == ga.InstanceIdentifier)
            {
                IEnumerator destructCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, base.Card, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destructCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destructCoroutine);
                }
            }
            if (IsRealAction(ga))
            {
                DestroyToAct = null;
            }
            yield break;
        }

        public IEnumerator ChoosePlayerToActResponse(DestroyCardAction dca)
        {
            if (dca.CardSource.Card == base.Card)
            {
                IEnumerator chooseCoroutine = base.GameController.SelectTurnTakerAndDoAction(new SelectTurnTakerDecision(base.GameController, base.HeroTurnTakerController, base.GameController.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero && !tt.IsIncapacitatedOrOutOfGame && tt != base.TurnTaker, battleZone: base.TurnTaker.BattleZone), SelectionType.None, cardSource: GetCardSource()), ActNowResponse);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(chooseCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(chooseCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator ActNowResponse(TurnTaker tt)
        {
            // "... may draw a card, play a card, and use a power in any order..."
            bool drewCard = false;
            bool playedCard = false;
            bool usedPower = false;
            List<SelectFunctionDecision> storedFunction = new List<SelectFunctionDecision>();
            Func<string> getNoFunctionString = delegate
            {
                List<string> list2 = new List<string>();
                if (!usedPower && (!base.GameController.CanUsePowers(base.GameController.FindHeroTurnTakerController(tt.ToHero()), GetCardSource()) || base.GameController.GetUsablePowersThisTurn(base.GameController.FindHeroTurnTakerController(tt.ToHero())).Count() < 1))
                {
                    list2.Add("use a power");
                }
                if (!playedCard && !CanPlayCardsFromHand(base.GameController.FindHeroTurnTakerController(tt.ToHero())))
                {
                    list2.Add("play a card");
                }
                if (!drewCard && !CanDrawCards(base.GameController.FindHeroTurnTakerController(tt.ToHero())))
                {
                    list2.Add("draw a card");
                }
                return "Stuntman cannot " + list2.ToCommaList(useWordAnd: false, useWordOr: true) + ".";
            };
            for (int i = 0; i < 3; i++)
            {
                List<Function> list = new List<Function>();
                list.Add(new Function(DecisionMaker, "Draw a Card", SelectionType.DrawCard, () => DrawCard(tt.ToHero()), !drewCard && CanDrawCards(base.GameController.FindHeroTurnTakerController(tt.ToHero()))));
                list.Add(new Function(DecisionMaker, "Play a Card", SelectionType.PlayCard, () => SelectAndPlayCardFromHand(base.GameController.FindHeroTurnTakerController(tt.ToHero()), optional: false), !playedCard && CanPlayCardsFromHand(base.GameController.FindHeroTurnTakerController(tt.ToHero()))));
                list.Add(new Function(DecisionMaker, "Use a Power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(base.GameController.FindHeroTurnTakerController(tt.ToHero()), optional: false, null, 1, eliminateUsedPowers: true, null, showMessage: false, allowAnyHeroPower: false, allowReplacements: true, canBeCancelled: true, null, forceDecision: false, allowOutOfPlayPower: false, GetCardSource()), !usedPower && base.GameController.CanUsePowers(base.GameController.FindHeroTurnTakerController(tt.ToHero()), GetCardSource()) && base.GameController.GetUsablePowersThisTurn(base.GameController.FindHeroTurnTakerController(tt.ToHero())).Count() > 0));
                SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.GameController.FindHeroTurnTakerController(tt.ToHero()), list, optional: true, null, getNoFunctionString(), null, GetCardSource());
                IEnumerator coroutine = base.GameController.SelectAndPerformFunction(selectFunction, storedFunction);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
                Function selectedFunction = GetSelectedFunction(storedFunction);
                if (selectedFunction != null)
                {
                    switch (selectedFunction.SelectionType)
                    {
                        case SelectionType.DrawCard:
                            drewCard = true;
                            break;
                        case SelectionType.PlayCard:
                            playedCard = true;
                            break;
                        case SelectionType.UsePower:
                            usedPower = true;
                            break;
                    }
                    continue;
                }
                break;
            }
            // "... then their hero deals themself 3 psychic damage."
            List<Card> damagedHero = new List<Card>();
            IEnumerator chooseCoroutine = base.FindCharacterCardToTakeDamage(tt, damagedHero, null, 3, DamageType.Psychic);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            IEnumerator damageCoroutine = base.GameController.DealDamageToSelf(base.GameController.FindHeroTurnTakerController(tt.ToHero()), (Card c) => damagedHero.Contains(c), 2, DamageType.Psychic, cardSource: GetCardSource());
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

        public override IEnumerator UsePower(int index = 0)
        {
            // "Draw a card."
            IEnumerator selfDrawCoroutine = base.GameController.DrawCard(base.HeroTurnTaker, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selfDrawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selfDrawCoroutine);
            }
            // "Another player draws a card."
            IEnumerator allyDrawCoroutine = base.GameController.SelectHeroToDrawCard(base.HeroTurnTakerController, optionalDrawCard: false, additionalCriteria: new LinqTurnTakerCriteria((TurnTaker tt) => tt != base.TurnTaker), cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(allyDrawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(allyDrawCoroutine);
            }
            yield break;
        }
    }
}
