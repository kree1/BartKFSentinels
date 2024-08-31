using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BartKFSentinels.Alaalu
{
    public class AlaaluUtilityCardController : CardController
    {
        public AlaaluUtilityCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {

        }

        public readonly string AlaalidKeyword = "alaalid";
        public readonly string ChoiceKeyword = "choice";
        public readonly string LivestockKeyword = "livestock";
        public readonly string LandmarkKeyword = "landmark";
        public readonly string MythKeyword = "myth";
        public readonly string WizardKeyword = "wizard";

        public LinqCardCriteria AlaalidCriteria()
        {
            return new LinqCardCriteria((Card c) => c.DoKeywordsContain(AlaalidKeyword), "Alaalid");
        }

        public LinqCardCriteria AlaalidInPlayCriteria()
        {
            return new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(AlaalidKeyword), "Alaalid", singular: "card in play", plural: "cards in play");
        }

        public LinqCardCriteria LivestockCriteria()
        {
            return new LinqCardCriteria((Card c) => c.DoKeywordsContain(LivestockKeyword), LivestockKeyword, false);
        }

        public LinqCardCriteria LivestockInPlayCriteria()
        {
            return new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(LivestockKeyword), LivestockKeyword + " in play", false);
        }

        public LinqCardCriteria NonMythLandmarkCriteria()
        {
            return new LinqCardCriteria((Card c) => c.DoKeywordsContain(LandmarkKeyword) && !c.DoKeywordsContain(MythKeyword), "non-Myth Landmark");
        }

        public LinqCardCriteria NonMythLandmarkInPlayCriteria()
        {
            return new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.DoKeywordsContain(LandmarkKeyword) && !c.DoKeywordsContain(MythKeyword), "non-Myth Landmark", singular: "card in play", plural: "cards in play");
        }

        public LinqCardCriteria MythCriteria()
        {
            return new LinqCardCriteria((Card c) => c.DoKeywordsContain(MythKeyword), MythKeyword);
        }
    }
}
