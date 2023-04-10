using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Memorial
{
    public abstract class MemorialUtilityCharacterCardController : VillainCharacterCardController
    {
        public MemorialUtilityCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public static readonly string RenownKeyword = "renown";

        public static bool IsRenown(Card c)
        {
            return c.DoKeywordsContain(RenownKeyword);
        }

        public int NumRenownsAt(Location loc)
        {
            if (loc.HighestRecursiveLocation == loc)
            {
                return GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsRenown(c) && c.Location.HighestRecursiveLocation == loc)).Count();
            }
            return GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsRenown(c) && c.Location == loc)).Count();
        }

        public bool IsRenownedTarget(Card c)
        {
            return c.IsInPlayAndHasGameText && IsHeroCharacterCard(c) && IsHero(c.Owner) && !c.Owner.ToHero().IsIncapacitatedOrOutOfGame && c.IsTarget && NumRenownsAt(c.Location.HighestRecursiveLocation) > 0;
        }

        public virtual IEnumerator ExtraRenownResponse(Card entering)
        {
            yield return null;
        }
    }
}
