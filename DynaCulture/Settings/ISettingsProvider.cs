using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynaCulture.Settings
{
    interface ISettingsProvider
    {
        bool GradualAssimilation { get; set; }

        int AssimilationDelay { get; set; }

        int OwnerInfluenceStrength { get; set; }

        int SettlementInfluenceRange { get; set; }

        bool TradeLinkedInfluence { get; set; }

        bool PlayerKingdomOnly { get; set; }

        bool ShowCorruptedTroopMessage { get; set; }

        bool ChangeNotablesCulture { get; set; }
    }
}
