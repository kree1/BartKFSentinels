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
    public class EarlyWarningSystemCardController : TorrentUtilityCardController
    {
        public EarlyWarningSystemCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public override void AddTriggers()
        {
            // "Whenever a Cluster would be dealt damage, you may redirect that damage to {TorrentCharacter}."
            //AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.DoKeywordsContain("cluster"), (DealDamageAction dda) => base.GameController.RedirectDamage(dda, base.CharacterCard, isOptional: true, cardSource: GetCardSource()), new TriggerType[] { TriggerType.WouldBeDealtDamage, TriggerType.RedirectDamage }, TriggerTiming.Before, isActionOptional: true);

            // "Whenever a Cluster would be dealt damage, you may prevent that daamge. If you do, the source of that damage deals {TorrentCharacter} 1 damage of that type."
            AddTrigger<DealDamageAction>((DealDamageAction dda) => dda.Target.DoKeywordsContain("cluster"), HitTorrentInsteadResponse, new TriggerType[] { TriggerType.CancelAction, TriggerType.WouldBeDealtDamage, TriggerType.DealDamage }, TriggerTiming.Before);
            // "At the end of your turn, you may play a Cluster from your trash, ignoring its 'when this card enters play' effects, then destroy it."
            AddEndOfTurnTrigger((TurnTaker tt) => tt == base.TurnTaker, PlayThenDestroyResponse, new TriggerType[] { TriggerType.PlayCard, TriggerType.DestroyCard }, additionalCriteria: (PhaseChangeAction pca) => base.TurnTaker.Trash.Cards.Any((Card c) => c.DoKeywordsContain("cluster")));
            base.AddTriggers();
        }

        public IEnumerator HitTorrentInsteadResponse(DealDamageAction dda)
        {
            // "... you may prevent that daamge. If you do, the source of that damage deals {TorrentCharacter} 1 damage of that type."
            if (dda.IsPretend)
            {
                IEnumerator cancelCoroutine = base.GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(cancelCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(cancelCoroutine);
                }
                yield break;
            }
            List<Card> associated = new List<Card>();
            associated.Add(dda.Target);
            List<YesNoCardDecision> choice = new List<YesNoCardDecision>();
            IEnumerator chooseCoroutine = base.GameController.MakeYesNoCardDecision(base.HeroTurnTakerController, SelectionType.PreventDamage, base.Card, action: dda, storedResults: choice, associatedCards: associated, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(chooseCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(chooseCoroutine);
            }
            YesNoCardDecision answer = choice.FirstOrDefault();
            if (answer.Answer.HasValue && answer.Answer.Value)
            {
                IEnumerator preventCoroutine = base.GameController.CancelAction(dda, isPreventEffect: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(preventCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(preventCoroutine);
                }
                DealDamageAction ddaPrime = new DealDamageAction(dda);
                ddaPrime.Target = base.CharacterCard;
                ddaPrime.Amount = 1;
                Card source = null;
                if (ddaPrime.DamageSource != null && ddaPrime.DamageSource.Card != null)
                {
                    source = ddaPrime.DamageSource.Card;
                }
                IEnumerator damageCoroutine = DealDamage(source, base.CharacterCard, 1, ddaPrime.DamageType, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(damageCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(damageCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator PlayThenDestroyResponse(PhaseChangeAction pca)
        {
            // "... you may play a Cluster from your trash, ignoring its 'when this card enters play' effects, then destroy it."
            List<SelectCardDecision> choice = new List<SelectCardDecision>();
            IEnumerator selectCoroutine = base.GameController.SelectCardAndStoreResults(base.HeroTurnTakerController, SelectionType.PlayCard, new LinqCardCriteria((Card c) => c.Location == base.TurnTaker.Trash && c.DoKeywordsContain("cluster"), "cluster card in " + base.TurnTaker.Name + "'s trash", false, false, "cluster card in " + base.TurnTaker.Name + "'s trash", "cluster cards in " + base.TurnTaker.Name + "'s trash"), choice, true, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(selectCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(selectCoroutine);
            }
            Card selectedCluster = GetSelectedCard(choice);
            if (selectedCluster != null)
            {
                CardController selectedController = FindCardController(selectedCluster);
                selectedController.SetCardPropertyToTrueIfRealAction(ClusterCardController.IgnoreEntersPlay);
                IEnumerator playCoroutine = base.GameController.PlayCard(base.TurnTakerController, selectedCluster, responsibleTurnTaker: base.TurnTaker, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(playCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(playCoroutine);
                }
                if (IsRealAction())
                {
                    selectedController.SetCardProperty(ClusterCardController.IgnoreEntersPlay, false);
                }
                IEnumerator destroyCoroutine = base.GameController.DestroyCard(base.HeroTurnTakerController, selectedCluster, showOutput: true, responsibleCard: base.Card, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(destroyCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(destroyCoroutine);
                }
            }
            yield break;
        }
    }
}
