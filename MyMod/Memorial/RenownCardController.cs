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
            SpecialStringMaker.ShowSpecialString(() => "This card is in " + base.Card.Location.GetFriendlyName() + ".").Condition = () => base.Card.IsInPlayAndHasGameText;
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            if (base.CharacterCard.IsFlipped)
            {
                // "When a villain Renown enters play, move it next to a non-Renowned hero character target."
                // ...
                return base.DeterminePlayLocation(storedResults, isPutIntoPlay, decisionSources, overridePlayArea, additionalTurnTakerCriteria);
            }
            else
            {
                return base.DeterminePlayLocation(storedResults, isPutIntoPlay, decisionSources, overridePlayArea, additionalTurnTakerCriteria);
            }
        }

        public override IEnumerator RunIfUnableToEnterPlay()
        {
            if (base.CharacterCard.IsFlipped)
            {
                // "... If there are none, discard it and play the top card of the villain deck."
                // ...
                return base.RunIfUnableToEnterPlay();
            }
            else
            {
                return base.RunIfUnableToEnterPlay();
            }
        }

        public override IEnumerator Play()
        {
            // "When this card enters play, {Memorial} deals this hero 2 projectile damage."
            // ...
            yield break;
        }
    }
}
