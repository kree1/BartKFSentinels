﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Torrent
{
    public class FocusingNodeCardController : ClusterCardController
    {
        public FocusingNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, {TorrentCharacter} deals 1 target X energy damage, where X = the number of targets destroyed this turn."
            AddWhenDestroyedTrigger(BlastResponse, TriggerType.DealDamage);
            base.AddTriggers();
        }

        public override IEnumerator Play()
        {
            if (Journal.GetCardPropertiesBoolean(base.Card, IgnoreEntersPlay) != true)
            {
                // "When this card enters play, you may draw a card or play a card."
                IEnumerator drawPlayCoroutine = DrawACardOrPlayACard(base.HeroTurnTakerController, true);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(drawPlayCoroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(drawPlayCoroutine);
                }
            }
            yield break;
        }

        public IEnumerator BlastResponse(DestroyCardAction dca)
        {
            // "... {TorrentCharacter} deals 1 target X energy damage, where X = the number of targets destroyed this turn."
            int x = base.NumTargetsDestroyedThisTurn();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), x, DamageType.Energy, 1, false, 1, cardSource: GetCardSource());
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
    }
}
