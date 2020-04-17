using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;

namespace DynaCulture.Util
{
    class DynaCultureUtils
    {
        public static void ChangeSettlementCulture(Settlement settlement, CultureObject culture)
        {
            var editableSettlement = Campaign.Current.Settlements.Where(x => x.StringId == settlement.StringId).FirstOrDefault();
            editableSettlement.Culture = culture;

            // Attempt to set attached villages
            if (editableSettlement.BoundVillages != null)
            {
                foreach (Village attached in editableSettlement.BoundVillages)
                {
                    if (attached.Settlement == null)
                        continue;

                    var editableBound = Campaign.Current.Settlements.Where(x => x.StringId == attached.Settlement.StringId).FirstOrDefault();
                    editableBound.Culture = culture;
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
    }
}
