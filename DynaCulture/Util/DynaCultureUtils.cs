using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace DynaCulture.Util
{
    class DynaCultureUtils
    {
        public static void ChangeSettlementCulture(Settlement settlement, CultureObject culture)
        {
            // Do not convert the last remaining town of a culture. Companions need a place to spawn or there will be crashes
            if (settlement.IsTown)
            {
                var remainingTowns = Campaign.Current.Settlements.Where(s => s.IsTown && s.Culture == settlement.Culture).Count();
                if (remainingTowns == 1)
                    return;
            }
            
            settlement.Culture = culture;
            ChangeSettlementNotablesCulture(settlement, culture, false);

            // Attempt to set attached villages
            if (settlement.BoundVillages != null)
            {
                foreach (Village attached in settlement.BoundVillages)
                {
                    if (attached.Settlement == null)
                        continue;

                    attached.Settlement.Culture = culture;
                    ChangeSettlementNotablesCulture(attached.Settlement, culture, false);
                }
            }
        }

        public static void ChangeSettlementNotablesCulture(Settlement settlement, CultureObject culture, bool forceChangeCulture)
        {
            //Update Notable culture to change the type of recruit/volunteer
            if (DynaCultureSettings.Instance.ChangeNotablesCulture || forceChangeCulture)
            {
                ChangeNotablesCulture(settlement.Notables.ToList(), culture);

                // Attempt to set attached villages
                if (settlement.BoundVillages != null)
                {
                    foreach (Village attached in settlement.BoundVillages)
                    {
                        if (attached.Settlement == null)
                            continue;

                        ChangeNotablesCulture(attached.Settlement.Notables.ToList(), culture);
                    }
                }
            }
        }

        private static void ChangeNotablesCulture(List<Hero> notables, CultureObject culture)
        {
            foreach(Hero notable in notables)
            {
                if (notable.CanHaveRecruits)
                {
                    notable.Culture = culture;
                }
            }
        }

        public static CultureObject GetOwnerCulture(Settlement settlement)
        {
            if (settlement.OwnerClan.Kingdom != null)
                return settlement.OwnerClan.Kingdom.Culture;
            else if (settlement.OwnerClan.Culture != null)
                return settlement.OwnerClan.Culture;
            else
                return settlement.Culture;
        }
        
        public static CultureObject GetGovernorCulture(Settlement settlement)
        {
            if (settlement.IsTown || settlement.IsCastle)
            {
                if (settlement.Town.Governor != null && settlement.Town.Governor.Culture != null)
                    return settlement.Town.Governor.Culture;
                else
                    return null;
            }
            else
                return null;            
        }

        public static bool IsPlayerOwner(Settlement settlement)
        {
            if (settlement.OwnerClan != null && settlement.OwnerClan.Leader != null && settlement.OwnerClan.Leader.IsHumanPlayerCharacter)
                return settlement.OwnerClan.Leader.IsHumanPlayerCharacter;
            else
                return false;
        }
    }
}
