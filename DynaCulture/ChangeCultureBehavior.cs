using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

using ChangeSettlementCulture.Util;

namespace ChangeSettlementCulture.Data
{
    class ChangeCultureBehavior : CampaignBehaviorBase
    {
        bool first = true;

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener((object)this, new Action<Settlement>(this.DailyTickSettlementMod));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(this.OnSettlementOwnerChangedMod));
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener((object)this, new Action<Clan, Kingdom, Kingdom, bool, bool>(this.OnClanChangedKingdomMod));
            CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener((object)this, new Action(this.OnSave));

            //if (System.Diagnostics.Debugger.IsAttached)
            CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, new Action<MobileParty, Settlement, Hero>(this.DebugCulture));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        public void OnGameLoaded(CampaignGameStarter starter)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            ChangeCultureManager.Reset();

            //ChangeCultureManager.Instance.InfluenceMap.Clear();

            foreach (Settlement settlement in Campaign.Current.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
            {
                //ChangeCultureManager.Instance.InfluenceMap.Add(settlement.StringId, new SettlementCultureStatus(settlement));
                /*
                if (settlement.OwnerClan.Kingdom != null)
                {
                    if (settlement.Culture.StringId != settlement.OwnerClan.Kingdom.Culture.StringId)
                        ChangeCultureUtils.ChangeSettlementCulture(settlement, settlement.OwnerClan.Kingdom.Culture);
                }
                else if (settlement.OwnerClan != null)
                {
                    if (settlement.Culture.StringId != settlement.OwnerClan.Culture.StringId)
                        ChangeCultureUtils.ChangeSettlementCulture(settlement, settlement.OwnerClan.Culture);
                }
                */
            }
        }

        void initializeAllSettlementCultures()
        {
            if (!ChangeCultureManager.Instance.InfluenceMap.Any())
            {
                foreach (Settlement settlement in Campaign.Current.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
                    ChangeCultureManager.Instance.InfluenceMap.Add(settlement.StringId, new SettlementCultureStatus(settlement));
            }

            foreach (Settlement settlement in Campaign.Current.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
                ChangeCultureManager.Instance.InfluenceMap[settlement.StringId].OnCampaignLoad();

            first = false;
        }

        public void DailyTickSettlementMod(Settlement settlement)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            if (first)
                initializeAllSettlementCultures();

            if (!Settings.Instance.PlayerKingdomOnly || (Settings.Instance.PlayerKingdomOnly && settlement.OwnerClan.Leader.IsHumanPlayerCharacter))
            {
                if (settlement.IsVillage || settlement.IsCastle || settlement.IsTown)
                    ChangeCultureManager.Instance.InfluenceMap[settlement.StringId].OnDailyTick();
            }
        }

        public void OnSettlementOwnerChangedMod(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            if (first)
                initializeAllSettlementCultures();

            if (!Settings.Instance.PlayerKingdomOnly || (Settings.Instance.PlayerKingdomOnly && settlement.OwnerClan.Leader.IsHumanPlayerCharacter))
            {
                if (settlement.IsVillage || settlement.IsCastle || settlement.IsTown)
                    ChangeCultureManager.Instance.InfluenceMap[settlement.StringId].OnNewOwner();
            }

            /*
            // Attempt to set culture based on kingdoms
            if (oldOwner.Clan.Kingdom != null && newOwner.Clan.Kingdom != null)
            {
                if (oldOwner.Clan.Kingdom.Culture.StringId != newOwner.Clan.Kingdom.Culture.StringId)
                {
                    ChangeCultureUtils.ChangeSettlementCulture(settlement, newOwner.Clan.Kingdom.Culture);
                }
            }
            // Fallback to setting culture on clan
            else if (oldOwner.Clan != null && newOwner.Clan != null)
            {
                if (oldOwner.Clan.Culture != newOwner.Clan.Culture)
                {
                    ChangeCultureUtils.ChangeSettlementCulture(settlement, newOwner.Clan.Culture);
                }
            }
            */
        }

        public void OnClanChangedKingdomMod(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, bool byRebellion, bool showNotification)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            if (clan.Settlements == null || !clan.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown).Any())
                return;

            if (first)
                initializeAllSettlementCultures();

            foreach (Settlement settlement in clan.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
            {
                if (!Settings.Instance.PlayerKingdomOnly || (Settings.Instance.PlayerKingdomOnly && settlement.OwnerClan.Leader.IsHumanPlayerCharacter))
                    ChangeCultureManager.Instance.InfluenceMap[settlement.StringId].OnNewOwner();
            }

            /*
            // Attempt to set culture based on kingdom
            if (oldKingdom != null && newKingdom != null)
            {
                if (oldKingdom.Culture.StringId != newKingdom.Culture.StringId)
                {
                    foreach (var settlement in clan.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
                    {
                        ChangeCultureUtils.ChangeSettlementCulture(settlement, newKingdom.Culture);
                    }
                }
            }
            // Fallback to setting culture based on clan
            else if (clan.Settlements != null)
            {
                foreach (var settlement in clan.Settlements.Where(x => x.IsVillage || x.IsCastle || x.IsTown))
                {
                    ChangeCultureUtils.ChangeSettlementCulture(settlement, clan.Culture);
                }
            }
            */
        }

        public void DebugCulture(MobileParty party, Settlement settlement, Hero hero)
        {
            try
            {
                if (party == null || settlement == null || hero == null || !party.IsMainParty)
                    return;

                var pair = ChangeCultureManager.Instance.InfluenceMap[settlement.StringId];

                string msg = $"Settlement: {settlement.Name} \nCulture: {settlement.Culture.Name}";
                string msg2 = "";

                foreach (var pair2 in ChangeCultureManager.Instance.InfluenceMap)
                {
                    var smnt = Settlement.Find(pair2.Key);
                    msg2 += pair2.Key.ToString() + " (" + smnt.Name + "), " + pair2.Value.TargetInfluences.Count
                        + "(Currently: " + smnt.Culture.Name + ", Originally: " + pair2.Value.homelandCultureId + ", Owned by: " + (smnt.OwnerClan.Kingdom == null ? smnt.OwnerClan.Culture.Name.ToString() : smnt.OwnerClan.Kingdom.Culture.Name.ToString()) + ")" + "\n";
                    msg2 += "Target Influences:\n";
                    foreach (var inf in pair2.Value.TargetInfluences)
                    {
                        decimal currentinf;
                        if (!pair2.Value.CurrentInfluences.TryGetValue(inf.Key, out currentinf))
                            currentinf = 0m;
                        msg2 += inf.Key + ", " + currentinf + " => " + inf.Value.ToString() + " | Days Since change: " + pair2.Value.daysSinceTargetsChanged + " | CurrentID: " + pair2.Value.expectedIDKey + " | TargetID: " + pair2.Value.TargetInfluencesIDKey + "\n";
                    }
                    msg2 += "Current Influences:\n";
                    foreach (var inf in pair2.Value.CurrentInfluences)
                    {
                        decimal targetinf;
                        if (!pair2.Value.TargetInfluences.TryGetValue(inf.Key, out targetinf))
                            targetinf = 0m;
                        msg2 += inf.Key + ", " + inf.Value.ToString() + " => " + targetinf + " | Days Since change: " + pair2.Value.daysSinceTargetsChanged + " | CurrentID: " + pair2.Value.expectedIDKey + " | TargetID: " + pair2.Value.TargetInfluencesIDKey + "\n";
                    }
                    msg2 += "\n";
                }
                string file = $@"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\debug.txt";
                string file2 = $@"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\debugPrevious.txt";
                if (System.IO.File.Exists(file))
                {
                    if (System.IO.File.Exists(file2))
                        System.IO.File.Delete(file2);

                    System.IO.File.Copy(file, file2);
                    System.IO.File.Delete(file);
                }

                System.IO.File.WriteAllText(file, msg2);

                //System.Windows.Forms.MessageBox.Show(msg);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// When the game saves, we will also save the culture status
        /// </summary>
        public void OnSave()
        {
            FileUtil.SaveSerializedFile(Hero.MainHero.Name.ToString(), ChangeCultureManager.Instance);
        }
    }
}
