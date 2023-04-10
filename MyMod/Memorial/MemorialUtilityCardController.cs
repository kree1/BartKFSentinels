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
            /*Log.Debug("IsRenownedTarget(" + c.Title + ")");
            Log.Debug(c.Title + ".IsInPlayAndHasGameText: " + c.IsInPlayAndHasGameText.ToString());
            Log.Debug("IsHeroCharacterCard(" + c.Title + "): " + IsHeroCharacterCard(c).ToString());
            Log.Debug("IsHero(" + c.Title + ".Owner): " + IsHero(c.Owner).ToString());
            Log.Debug("!" + c.Title + ".Owner.ToHero().IsIncapacitatedOrOutOfGame: " + (!c.Owner.ToHero().IsIncapacitatedOrOutOfGame).ToString());
            Log.Debug(c.Title + ".IsTarget: " + c.IsTarget.ToString());*/
            return c.IsInPlayAndHasGameText && IsHeroCharacterCard(c) && IsHero(c.Owner) && !c.Owner.ToHero().IsIncapacitatedOrOutOfGame && c.IsTarget && NumRenownsAt(c.Location.HighestRecursiveLocation) > 0;
        }

        public IEnumerable<Card> AllRenownedTargets()
        {
            return GameController.FindCardsWhere(new LinqCardCriteria((Card c) => IsRenownedTarget(c)));
        }

        public LinqCardCriteria IsNonRenownedHeroCharacterTarget()
        {
            return new LinqCardCriteria((Card c) => IsHeroCharacterCard(c) && c.IsTarget && !IsRenownedTarget(c), "non-Renowned hero character", true, false, "target", "targets");
        }

        public IEnumerable<Card> NonRenownedHeroCharacterTargets()
        {
            return GameController.FindCardsWhere((Card c) => IsHeroCharacterCard(c) && c.IsTarget && !IsRenownedTarget(c));
        }
    }
}
