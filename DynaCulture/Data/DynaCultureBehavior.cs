using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

using DynaCulture.Util;

namespace DynaCulture.Data
{
    class DynaCultureBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener((object)this, new Action<Settlement>(this.DailyTickSettlementMod));
            CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener((object)this, new Action(this.OnSave));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        public void OnGameLoaded(CampaignGameStarter starter)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            // Reset between sessions to avoid culture cross contamination when loading without restarting the game
            DynaCultureManager.Reset();

            //Initialize settlement culture if there is a savefile if not it will populate the InfluenceMap of the manager with current settlement
            DynaCultureManager.Initialize();
        }

        public void DailyTickSettlementMod(Settlement settlement)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            if (DynaCultureManager.Instance.InfluenceMap.Count == 0)
                DynaCultureManager.Initialize();

            if (!DynaCultureSettings.Instance.PlayerKingdomOnly || (DynaCultureSettings.Instance.PlayerKingdomOnly && settlement.OwnerClan != null  && settlement.OwnerClan.Leader != null && settlement.OwnerClan.Leader.IsHumanPlayerCharacter))
            {
                if (settlement.IsVillage || settlement.IsCastle || settlement.IsTown)
                    DynaCultureManager.Instance.InfluenceMap[settlement.StringId].OnDailyTick();
            }
        }

        /// <summary>
        /// When the game saves, we will also save the culture status
        /// </summary>
        public void OnSave()
        {
            FileUtil.SaveSerializedFile(Hero.MainHero.Name.ToString(), DynaCultureManager.Instance);
        }
    }
}
