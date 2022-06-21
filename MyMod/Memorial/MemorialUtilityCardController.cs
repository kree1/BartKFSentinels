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
    public abstract class MemorialUtilityCardController : CardController
    {
        public MemorialUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public static readonly string RenownKeyword = "renown";

        public static bool IsRenown(Card c)
        {
            return c.DoKeywordsContain(RenownKeyword);
        }

        public static int NumRenownsAt(Location loc)
        {
            return loc.Cards.Where((Card c) => IsRenown(c)).Count();
        }

        public static bool IsRenownedTarget(Card c)
        {
            return c.IsInPlayAndHasGameText && c.IsHeroCharacterCard && c.Owner.IsHero && !c.Owner.ToHero().IsIncapacitatedOrOutOfGame && c.IsTarget && NumRenownsAt(c.Location.HighestRecursiveLocation) > 0;
        }

        public IEnumerable<Card> AllRenownedTargets()
        {
            return GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsRenownedTarget(c)));
        }
    }
}
