using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;

using ChangeSettlementCulture.Util;

namespace ChangeSettlementCulture.Data
{
    [Serializable]
    class ChangeCultureManager
    {
        [field:NonSerialized]
        static ChangeCultureManager _instance;

        public static ChangeCultureManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FileUtil.TryLoadSerializedFile<ChangeCultureManager>(Hero.MainHero.Name.ToString());
                    if (_instance == null)
                    {
                        _instance = new ChangeCultureManager();
                    }
                }

                return _instance;
            }
        }

        internal static void Reset()
        {
            _instance = null;
        }

        public Dictionary<string, SettlementCultureStatus> InfluenceMap = new Dictionary<string, SettlementCultureStatus>();
    }
}
