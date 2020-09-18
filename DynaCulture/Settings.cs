using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using ModLib.Definitions;
using ModLib.Definitions.Attributes;

namespace DynaCulture
{
    public class Settings : SettingsBase
    {
        public override string ModName => "DynaCulture";
        public override string ModuleFolderName => SubModule.ModuleFolderName;
        public const string SettingsInstanceID = "DynaCultureSettings";

        public static Settings Instance
        {
            get
            {
                return (Settings)SettingsDatabase.GetSettings<Settings>();
            }
        }

        [XmlElement]
        public override string ID { get; set; } = SettingsInstanceID;

        [XmlElement]
        [SettingProperty("Gradual Assimilation", "(Default true) The culture will gradually switch to the new culture")]
        [SettingPropertyGroup("Gradual Assimilation", true)]
        public bool GradualAssimilation { get; set; } = true;

        [XmlElement]
        [SettingProperty("Assimilation Resistance Factor", 15, 90, "(Default 45) Resistance factor for change in culture. Higher resistance slows down the rate of change")]
        [SettingPropertyGroup("Gradual Assimilation")]
        public int AssimilationDelay { get; set; } = 45;

        [XmlElement]
        [SettingProperty("Owner Kingdom Influence Strength", 0, 20, "(Default 5) Extra influence for the owner of the settlement")]
        public int OwnerInfluenceStrength { get; set; } = 5;

        [XmlElement]
        [SettingProperty("Settlement Influence Range", 1, 60, "(Default 30) Geographical range to check for influence from surrounding settlements")]
        public int SettlementInfluenceRange { get; set; } = 30;

        [XmlElement]
        [SettingProperty("Linked Settlements Cause Influence", "(Default true) Settlements linked by peasant trade will influence each other")]
        public bool TradeLinkedInfluence { get; set; } = true;

        [XmlElement]
        [SettingProperty("Assimilate Player Kingdom Only", "(Default false) Only the Player's kingdom's settlements will change culture")]
        public bool PlayerKingdomOnly { get; set; } = false;

        [XmlElement]
        [SettingProperty("Show Corrupted Troop Message", "(Default true) Toggle the corrupted troops removal message")]
        public bool ShowCorruptedTroopMessage { get; set; } = true;

        //[XmlElement]
        //[SettingProperty("Debug Mode", "(Default false) Output Debug Information to desktop. Warning: probably causes problems")]
        //public bool DebugMode { get; set; } = false;

        //[XmlElement]
        //[SettingProperty("Allow Forced Assimilation", "(Default false) Enable a settlement action to forcibly assimilate a settlement")]
        //public bool AllowForcedAssimilation { get; set; } = false;

        //[XmlElement]
        //[SettingProperty("Change Settlement Visuals (Experimental)", "(Default false) Match settlement model to the changed culture")]
        //public bool ChangeSettlementVisuals { get; set; } = false;
    }
}
