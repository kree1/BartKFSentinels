using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BartKFSentinels.Ownership
{
    public class StatCharacterCardController : OwnershipBaseCharacterCardController
    {
        public StatCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            // Front side: Show token pool
            SpecialStringMaker.ShowTokenPool(base.Card.FindTokenPool(WeightPoolIdentifier), this).Condition = () => base.Card.IsInPlayAndHasGameText && !base.Card.IsFlipped;
            // Both sides: Show location of this hero's marker
            SpecialStringMaker.ShowSpecialString(() => DisplayMarkerLocation(RelevantHeroIndex())).Condition = () => base.Card.Location.IsPlayArea && base.Card.Location.IsHero;
        }

        public int RelevantHeroIndex()
        {
            return IndexOfHero(base.GameController.FindHeroTurnTakerController(base.Card.Location.OwnerTurnTaker.ToHero()));
        }

        public override void AddSideTriggers()
        {
            base.AddSideTriggers();
            if (!base.Card.IsFlipped)
            {
                // Front side:
                // "Whenever tokens are added to or removed from this card, move your marker on the Map to row Y, where Y = 4 minus (the number of tokens on this card divided by 10, rounded down)."
                AddSideTrigger(AddTrigger((AddTokensToPoolAction tpa) => tpa.TokenPool.CardWithTokenPool == base.Card, UpdateMarkerResponse, TriggerType.AddTokensToPool, TriggerTiming.After));
                AddSideTrigger(AddTrigger((RemoveTokensFromPoolAction tpa) => tpa.TokenPool.CardWithTokenPool == base.Card, UpdateMarkerResponse, TriggerType.AddTokensToPool, TriggerTiming.After));
                // "At the end of your turn, if your marker is in row 0 or 1, you may destroy 1 of your Ongoing or Equipment cards. If your marker is in row 0 or 1 and no card was destroyed this way, {OwnershipCharacter} deals your hero 4 melee damage."
                AddSideTrigger(AddEndOfTurnTrigger((TurnTaker tt) => tt == base.Card.Location.OwnerTurnTaker && HeroMarkerLocation(RelevantHeroIndex())[0] < 2, ConsumersAttackResponse, new TriggerType[] { TriggerType.DestroyCard, TriggerType.DealDamage }));
            }
            else
            {
                // Back side:
                // "Hero targets in this play area are immune to fire damage."
                AddSideTrigger(AddImmuneToDamageTrigger((DealDamageAction dda) => IsHeroTarget(dda.Target) && dda.Target.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation) && dda.DamageType == DamageType.Fire));
                // "Increase damage dealt by hero targets in this play area to {OwnershipCharacter} by 5."
                AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == FindCard(OwnershipIdentifier) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation), (DealDamageAction dda) => 5));
                // "If your marker is in row 1, 2, or 3 and in column 1, 2, or 3, increase damage dealt by hero targets in this play area to {OwnershipCharacter} by an additional 20."
                AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == FindCard(OwnershipIdentifier) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation) && HeroMarkerLocation(RelevantHeroIndex())[0] > 0 && HeroMarkerLocation(RelevantHeroIndex())[0] < 4 && HeroMarkerLocation(RelevantHeroIndex())[1] > 0 && HeroMarkerLocation(RelevantHeroIndex())[1] < 4, (DealDamageAction dda) => 20));
                // "If your marker is at row 2, column 2, increase damage dealt by hero targets in this play area to {OwnershipCharacter} by an additional 100."
                AddSideTrigger(AddIncreaseDamageTrigger((DealDamageAction dda) => dda.Target == FindCard(OwnershipIdentifier) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card) && dda.DamageSource.Card.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation) && HeroMarkerLocation(RelevantHeroIndex())[0] == 2 && HeroMarkerLocation(RelevantHeroIndex())[1] == 2, (DealDamageAction dda) => 100));
                AddSideTrigger(AddTrigger((DealDamageAction dda) => dda.Target == FindCard(OwnershipIdentifier) && dda.DamageSource != null && dda.DamageSource.IsCard && IsHeroTarget(dda.DamageSource.Card), LogDamageResponse, TriggerType.Hidden, TriggerTiming.Before));
            }
        }

        public IEnumerator LogDamageResponse(DealDamageAction dda)
        {
            Log.Debug("StatCharacterCardController.LogDamageResponse called from " + base.Card.Location.GetFriendlyName() + " for damage dealt to Ownership by " + dda.DamageSource.Card.Title);
            Log.Debug("StatCharacterCardController.LogDamageResponse: marker at " + MapLocationDescription(HeroMarkerLocation(RelevantHeroIndex())));
            Log.Debug("StatCharacterCardController.LogDamageResponse: location match? " + (dda.DamageSource.Card.IsAtLocationRecursive(base.Card.Location.HighestRecursiveLocation)).ToString());
            yield break;
        }

        public override IEnumerator Play()
        {
            IEnumerator setupCoroutine;
            if (!base.Card.IsFlipped)
            {
                // Setup (when moved into a hero play area face-up): "Put your marker on the Map at row 1, column X, where X = your place in the hero turn order minus 1."
                setupCoroutine = MoveHeroMarker(RelevantHeroIndex(), 1, RelevantHeroIndex() - 1, showMessage: true, cardSource: GetCardSource());
            }
            else
            {
                // When moved into a hero play area face-down: remove all tokens, the Weight Pool is no longer relevant
                setupCoroutine = base.GameController.RemoveTokensFromPool(base.Card.FindTokenPool(WeightPoolIdentifier), 50, cardSource: GetCardSource());
            }
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(setupCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(setupCoroutine);
            }
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            if (base.Card.IsInPlayAndNotUnderCard)
            {
                IEnumerator flipCoroutine = base.AfterFlipCardImmediateResponse();
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(flipCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(flipCoroutine);
                }
            }
            else
            {
                yield break;
            }
        }

        public IEnumerator UpdateMarkerResponse(GameAction ga)
        {
            // "... move your marker on the Map to row Y, where Y = 4 minus (the number of tokens on this card divided by 10, rounded down)."
            int[] currentLocation = HeroMarkerLocation(RelevantHeroIndex());
            int targetRow = 4 - (int)(base.Card.FindTokenPool(WeightPoolIdentifier).CurrentValue / 10);
            int rowChange = targetRow - currentLocation[0];
            if (rowChange != 0)
            {
                IEnumerator moveCoroutine = MoveHeroMarker(RelevantHeroIndex(), rowChange, 0, showMessage: true, cardSource: GetCardSource());
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(moveCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(moveCoroutine);
                }
            }
        }

        public IEnumerator ConsumersAttackResponse(PhaseChangeAction pca)
        {
            IEnumerator messageCoroutine = base.GameController.SendMessageAction("CONSUMERS ATTACK\n" + base.Card.Location.OwnerName.ToUpper(), Priority.Medium, GetCardSource(), associatedCards: base.Card.Location.OwnerTurnTaker.CharacterCards);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(messageCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(messageCoroutine);
            }
            // "... you may destroy 1 of your Ongoing or Equipment cards."
            List<DestroyCardAction> results = new List<DestroyCardAction>();
            IEnumerator destroyCoroutine = base.GameController.SelectAndDestroyCard(base.GameController.FindHeroTurnTakerController(base.Card.Location.OwnerTurnTaker.ToHero()), new LinqCardCriteria((Card c) => (IsOngoing(c) || IsEquipment(c)) && c.Owner == base.Card.Location.OwnerTurnTaker, "belonging to " + base.Card.Location.OwnerTurnTaker.Name, useCardsSuffix: false, useCardsPrefix: true, singular: "Ongoing or Equipment card", plural: "Ongoing or Equipment cards"), true, results, responsibleCard: base.Card, cardSource: GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(destroyCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(destroyCoroutine);
            }
            // "If your marker is in row 0 or 1 and no card was destroyed this way, {OwnershipCharacter} deals your hero 4 melee damage."
            if (HeroMarkerLocation(RelevantHeroIndex())[0] < 2 && !DidDestroyCards(results))
            {
                List<Card> choices = new List<Card>();
                IEnumerator findCoroutine = FindCharacterCardToTakeDamage(base.Card.Location.OwnerTurnTaker, choices, FindCard(OwnershipIdentifier), 4, DamageType.Melee);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(findCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(findCoroutine);
                }
                Card selected = choices.FirstOrDefault();
                if (selected != null)
                {
                    IEnumerator meleeCoroutine = DealDamage(FindCard(OwnershipIdentifier), selected, 4, DamageType.Melee, cardSource: GetCardSource());
                    if (base.UseUnityCoroutines)
                    {
                        yield return base.GameController.StartCoroutine(meleeCoroutine);
                    }
                    else
                    {
                        base.GameController.ExhaustCoroutine(meleeCoroutine);
                    }
                }
            }
        }
    }
}
