using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DynaCulture.Settings;

namespace DynaCulture
{
    class DynaCultureSettings
    {
        ISettingsProvider _provider;
        public bool GradualAssimilation { get => _provider.GradualAssimilation; set => _provider.GradualAssimilation = value; }
        public int AssimilationDelay { get => _provider.AssimilationDelay; set => _provider.AssimilationDelay = value; }
        public int OwnerInfluenceStrength { get => _provider.OwnerInfluenceStrength; set => _provider.OwnerInfluenceStrength = value; }
        public int SettlementInfluenceRange { get => _provider.SettlementInfluenceRange; set => _provider.SettlementInfluenceRange = value; }
        public bool TradeLinkedInfluence { get => _provider.TradeLinkedInfluence; set => _provider.TradeLinkedInfluence = value; }
        public bool PlayerKingdomOnly { get => _provider.PlayerKingdomOnly; set => _provider.PlayerKingdomOnly = value; }
        public bool ShowCorruptedTroopMessage { get => _provider.ShowCorruptedTroopMessage; set => _provider.ShowCorruptedTroopMessage = value; }
        public bool ChangeNotablesCulture { get => _provider.ChangeNotablesCulture; set => _provider.ChangeNotablesCulture = value; }

        static DynaCultureSettings _instance;
        public static DynaCultureSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DynaCultureSettings();

                return _instance;
            }
        }

        public DynaCultureSettings()
        {
            ISettingsProvider mcm = null;
            // MCM as a soft dependency
            try
            {
                mcm = MCMDynaCultureSettings.Instance;
            }
            catch { }
            if (mcm != null)
                _provider = mcm;
            else
                _provider = new DefaultDynaCultureSettings();
        }
    }
}
