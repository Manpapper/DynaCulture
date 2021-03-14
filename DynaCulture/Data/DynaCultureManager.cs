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

        public static void Initialize()
        {
            // Add resilience against new settments being added mid-campaign
            foreach (Settlement settlement in Campaign.Current.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
            {
                if (!DynaCultureManager.Instance.InfluenceMap.ContainsKey(settlement.StringId))
                    DynaCultureManager.Instance.InfluenceMap.Add(settlement.StringId, new DynaCultureStatus(settlement));
            }

            // Allow each settlement to initialize itself only after assuring all settments have culture statuses
            foreach (Settlement settlement in Campaign.Current.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
            {
                DynaCultureManager.Instance.InfluenceMap[settlement.StringId].OnCampaignLoad();
            }
        }

        internal static void Reset()
        {
            _instance = null;
        }

        public Dictionary<string, DynaCultureStatus> InfluenceMap = new Dictionary<string, DynaCultureStatus>();
    }
}
