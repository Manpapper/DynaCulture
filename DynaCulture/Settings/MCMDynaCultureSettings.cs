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

        [SettingPropertyBool("{=ieJye6ml8N}Gradual Assimilation", HintText = "{=ieJye6ml12N}(Default true) The culture will gradually switch to the new culture", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml8N}Gradual Assimilation")]
        public bool GradualAssimilation { get; set; } = true;

        [SettingPropertyInteger("{=ieJye6ml11N}Assimilation Resistance Factor", 15, 90, HintText = "{=ieJye6ml13N}(Default 45) Resistance factor for change in culture. Higher resistance slows down the rate of change", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml8N}Gradual Assimilation")]
        public int AssimilationDelay { get; set; } = 45;

        [SettingPropertyInteger("{=ieJye6ml19N}Owner Kingdom Influence Strength", 0, 50, HintText = "{=ieJye6ml14N}(Default 5) Extra influence for the owner of the settlement", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml20N}Culture Influence")]
        public int OwnerInfluenceStrength { get; set; } = 5;

        [SettingPropertyBool("{=ieJye6ml29N}Governor Influence Player Settlement Only", HintText = "{=ieJye6ml28N}(Default true) Governor culture will only affect player settlement", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml27N}Governor Influence")]
        public bool GovernorCultureInfluencePlayerSettlementOnly { get; set; } = true;

        [SettingPropertyInteger("{=ieJye6ml25N}Governor Influence Strength", 0, 50, HintText = "{=ieJye6ml26N}(Default 0 (The governor culture doesn't influence the settlement culture)) Allow the governor culture to influence the culture of the settlement it's assigned to. If you want to change city culture to the one of your governor you probably want a high value (30 to max) if the settlement is surround by settlements with the same culture (if you have high owner influence it might conflict)", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml27N}Governor Influence")]
        public int GovernorInfluenceStrength { get; set; } = 0;

        [SettingPropertyInteger("{=ieJye6ml21N}Settlement Influence Range", 1, 60, HintText = "{=ieJye6ml15N}(Default 30) Geographical range to check for influence from surrounding settlements", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml20N}Culture Influence")]
        public int SettlementInfluenceRange { get; set; } = 30;

        [SettingPropertyBool("{=ieJye6ml22N}Linked Settlements Cause Influence", HintText = "{=ieJye6ml16N}(Default true) Settlements linked by peasant trade will influence each other", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml20N}Culture Influence")]
        public bool TradeLinkedInfluence { get; set; } = true;

        [SettingPropertyBool("{=ieJye6ml23N}Assimilate Player Kingdom Only", HintText = "{=ieJye6ml17N}(Default false) Only the Player's kingdom's settlements will change culture", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml10N}Settings")]
        public bool PlayerKingdomOnly { get; set; } = false;

        [SettingPropertyBool("{=ieJye6ml24N}Change Notables Culture", HintText = "{=ieJye6ml18N}(Default true) When a settlement change culture activating this will change the culture of Notable (It will affect recruit/volunteer type).", Order = 1, RequireRestart = false)]
        [SettingPropertyGroup("{=ieJye6ml10N}Settings")]
        public bool ChangeNotablesCulture { get; set; } = true;

    }
}
