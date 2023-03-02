using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaCulture.Settings
{
    class DefaultDynaCultureSettings : ISettingsProvider
    {
        public bool GradualAssimilation { get; set; } = true;
        public int AssimilationDelay { get; set; } = 45;
        public int OwnerInfluenceStrength { get; set; } = 6;
        public int SettlementInfluenceRange { get; set; } = 30;
        public bool TradeLinkedInfluence { get; set; } = true;
        public bool PlayerKingdomOnly { get; set; } = false;
        public bool ChangeNotablesCulture { get; set; } = true;
        public int GovernorInfluenceStrength { get; set; } = 0;
        public bool GovernorCultureInfluencePlayerSettlementOnly { get; set; } = true;
        public bool ShowInfluencesInDetails { get; set; } = true;
    }
}
