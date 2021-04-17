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
    public class BurstNodeCardController : ClusterCardController
    {
        public BurstNodeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override void AddTriggers()
        {
            // "When this card is destroyed, {TorrentCharacter} deals up to X targets 2 projectile damage each, where X = the number of targets destroyed this turn."
            AddWhenDestroyedTrigger(ExplodeResponse, TriggerType.DealDamage);
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

        public IEnumerator ExplodeResponse(DestroyCardAction dca)
        {
            // "... {TorrentCharacter} deals up to X targets 2 projectile damage each, where X = the number of targets destroyed this turn."
            int x = NumTargetsDestroyedThisTurn();
            IEnumerator damageCoroutine = base.GameController.SelectTargetsAndDealDamage(base.HeroTurnTakerController, new DamageSource(base.GameController, base.CharacterCard), 2, DamageType.Projectile, x, false, 0, cardSource: GetCardSource());
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
