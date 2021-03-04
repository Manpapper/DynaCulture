using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;

using DynaCulture.Util;

namespace DynaCulture.Data
{
    [Serializable]
    class DynaCultureManager
    {
        [field:NonSerialized]
        static DynaCultureManager _instance;

        public static DynaCultureManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FileUtil.TryLoadSerializedFile<DynaCultureManager>(Hero.MainHero.Name.ToString());
                    if (_instance == null)
                    {
                        _instance = new DynaCultureManager();
                    }
                }

                return _instance;
            }
        }

        internal static void Reset()
        {
            _instance = null;
        }

        public Dictionary<string, DynaCultureStatus> InfluenceMap = new Dictionary<string, DynaCultureStatus>();
    }
}
