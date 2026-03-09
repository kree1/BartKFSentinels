using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Planetfall
{
    public class NeuralDisruptorCardController : DestructiveChipCardController
    {
        public NeuralDisruptorCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToDeal = DamageType.Psychic;
            Relevant = new LinqCardCriteria((Card c) => IsOngoing(c), "Ongoing");
            SpecialStringMaker.ShowHeroWithMostCards(false, Relevant);
        }

        protected override LinqCardCriteria ToDestroy()
        {
            return new LinqCardCriteria((Card c) => IsHero(c) && IsOngoing(c), "hero Ongoing");
        }
    }
}
