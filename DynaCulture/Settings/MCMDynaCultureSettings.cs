using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;

namespace DynaCulture.Settings
{
    public class MCMDynaCultureSettings : AttributeGlobalSettings<MCMDynaCultureSettings>, ISettingsProvider
    {
        public override string Id => nameof(MCMDynaCultureSettings);

        public override string DisplayName => "DynaCulture Settings";

        public override string FolderName => nameof(MCMDynaCultureSettings);

        public override string FormatType => "json2";

        [SettingPropertyBool("Gradual Assimilation", HintText = "(Default true) The culture will gradually switch to the new culture", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Gradual Assimilation")]
        public bool GradualAssimilation { get; set; } = true;

        [SettingPropertyInteger("Assimilation Resistance Factor", 15, 90, HintText = "(Default 45) Resistance factor for change in culture. Higher resistance slows down the rate of change", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Gradual Assimilation")]
        public int AssimilationDelay { get; set; } = 45;

        [SettingPropertyInteger("Owner Kingdom Influence Strength", 0, 20, HintText = "(Default 5) Extra influence for the owner of the settlement", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Culture Influence")]
        public int OwnerInfluenceStrength { get; set; } = 5;

        [SettingPropertyInteger("Settlement Influence Range", 1, 60, HintText = "(Default 30) Geographical range to check for influence from surrounding settlements", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Culture Influence")]
        public int SettlementInfluenceRange { get; set; } = 30;

        [SettingPropertyBool("Linked Settlements Cause Influence", HintText = "(Default true) Settlements linked by peasant trade will influence each other", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Culture Influence")]
        public bool TradeLinkedInfluence { get; set; } = true;

        [SettingPropertyBool("Assimilate Player Kingdom Only", HintText = "(Default false) Only the Player's kingdom's settlements will change culture", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Settings")]
        public bool PlayerKingdomOnly { get; set; } = false;

        [SettingPropertyBool("Change Notables Culture", HintText = "(Default true) When a settlement change culture activating this will change the culture of Notable (It will affect recruit/volunteer type), notables who already changed culture will keep it.", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Settings")]
        public bool ChangeNotablesCulture { get; set; } = true;

        [SettingPropertyBool("Show Corrupted Troop Message", HintText = "(Default true) Toggle the corrupted troops removal message", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("Logs")]
        public bool ShowCorruptedTroopMessage { get; set; } = true;

    }
}
