using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.Library;

using DynaCulture.Util;
using TaleWorlds.ObjectSystem;
using System.Xml.Linq;
using TaleWorlds.Core;
using System.Windows.Forms;

namespace DynaCulture.Data
{
    class DynaCultureBehavior : CampaignBehaviorBase
    {
        bool first = true;

        public DynaCultureBehavior()
        {
            // Clean up any existing CultureChangeManagers from current session
            DynaCultureManager.Reset();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener((object)this, new Action<Settlement>(this.DailyTickSettlementMod));
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener((object)this, new Action<Settlement, bool, Hero, Hero, Hero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail>(this.OnSettlementOwnerChangedMod));
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener((object)this, new Action<Clan, Kingdom, Kingdom, bool, bool>(this.OnClanChangedKingdomMod));
            CampaignEvents.OnBeforeSaveEvent.AddNonSerializedListener((object)this, new Action(this.OnSave));
            CampaignEvents.HourlyTickPartyEvent.AddNonSerializedListener((object)this, new Action<MobileParty>(this.RemoveCorruptedTroops));
            CampaignEvents.HourlyTickSettlementEvent.AddNonSerializedListener((object)this, new Action<Settlement>(this.RemoveCorruptedRecruits));

            //if (System.Diagnostics.Debugger.IsAttached)
            //CampaignEvents.SettlementEntered.AddNonSerializedListener((object)this, new Action<MobileParty, Settlement, Hero>(this.DebugCulture));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        public void OnGameLoaded(CampaignGameStarter starter)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            // Reset between sessions to avoid culture cross contamination
            DynaCultureManager.Reset();

            // clean up corrupted troops from previous sessions, if they exist
            foreach (var party in Campaign.Current.MobileParties)
                RemoveCorruptedTroops(party);

            // clean up corrupted recruits from previous sessions, if they exist
            foreach (var settlement in Campaign.Current.Settlements)
                RemoveCorruptedRecruits(settlement);
        }

        void initializeAllSettlementCultures()
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
                    DynaCultureManager.Instance.InfluenceMap[settlement.StringId].OnDailyTick();
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
                    DynaCultureManager.Instance.InfluenceMap[settlement.StringId].OnNewOwner();
            }
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
                    DynaCultureManager.Instance.InfluenceMap[settlement.StringId].OnNewOwner();
            }
        }

        bool cleanseRoster(TroopRoster roster)
        {
            bool removed = false;

            // search for corrupted troops in the internal member "data" of TroopRoster
            TroopRosterElement[] troops = (TroopRosterElement[])roster.GetType().GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(roster);
            var characterObjects = troops.Where(t => t.Number > 0 && t.Character?.Age == 0f).Select(t => t.Character).ToList();

            if (characterObjects.Any())
            {
                // delete them
                foreach (CharacterObject troop in characterObjects)
                {
                    roster.RemoveTroop(troop, roster.GetTroopCount(troop));
                    removed = true;
                }
            }

            return removed;
        }

        public void RemoveCorruptedTroops(MobileParty mobileParty)
        {
            try
            {
                if (mobileParty.MemberRoster.Count != 0)
                {
                    var removed = cleanseRoster(mobileParty.MemberRoster);

                    if (removed && Settings.Instance.ShowCorruptedTroopMessage)
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Corrupted troops were cleansed from {mobileParty.Name} party.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
                }

                if (mobileParty.PrisonRoster.Count != 0)
                {
                    var removed = cleanseRoster(mobileParty.PrisonRoster);

                    if (removed && Settings.Instance.ShowCorruptedTroopMessage)
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Corrupted prisoners were cleansed from {mobileParty.Name} party.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
                }
            }
            catch
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Error attempting to cleanse corrupted troops or prisoners from {mobileParty.Name} party.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
            }
        }

        public void RemoveCorruptedRecruits(Settlement settlement)
        {
            try
            {
                bool removed = false;
                foreach (var notable in settlement.Notables.Where(n => n.CanHaveRecruits))
                {
                    for (int x = 0; x < Hero.MaximumNumberOfVolunteers; x++)
                    {
                        var recruit = notable.VolunteerTypes[x];
                        if (recruit != null && recruit.Age == 0f)
                        {
                            notable.VolunteerTypes[x] = null;
                            removed = true;
                        }
                    }
                }

                if (removed && Settings.Instance.ShowCorruptedTroopMessage)
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Corrupted recruits were removed from {settlement.Name}.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
            }
            catch
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Error attempting to remove corrupted recruits from {settlement.Name}.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
            }
        }

        //public void DebugCulture(MobileParty party, Settlement settlement, Hero hero)
        //{
        //    try
        //    {
        //        //if (!Settings.Instance.DebugMode)
        //        //    return;

        //        if (party == null || settlement == null || hero == null || !party.IsMainParty)
        //            return;

        //        var pair = DynaCultureManager.Instance.InfluenceMap[settlement.StringId];

        //        string msg = $"Settlement: {settlement.Name} \nCulture: {settlement.Culture.Name}";
        //        string msg2 = "";

        //        foreach (var pair2 in DynaCultureManager.Instance.InfluenceMap)
        //        {
        //            var smnt = Settlement.Find(pair2.Key);
        //            msg2 += pair2.Key.ToString() + " (" + smnt.Name + "), " + pair2.Value.TargetInfluences.Count
        //                + "(Currently: " + smnt.Culture.Name + ", Originally: " + pair2.Value.homelandCultureId + ", Owned by: " + (smnt.OwnerClan.Kingdom == null ? smnt.OwnerClan.Culture.Name.ToString() : smnt.OwnerClan.Kingdom.Culture.Name.ToString()) + ")" + "\n";
        //            msg2 += "Target Influences:\n";
        //            foreach (var inf in pair2.Value.TargetInfluences)
        //            {
        //                decimal currentinf;
        //                if (!pair2.Value.CurrentInfluences.TryGetValue(inf.Key, out currentinf))
        //                    currentinf = 0m;
        //                msg2 += inf.Key + ", " + currentinf + " => " + inf.Value.ToString() + " | Days Since change: " + pair2.Value.daysSinceTargetsChanged + " | CurrentID: " + pair2.Value.expectedIDKey + " | TargetID: " + pair2.Value.TargetInfluencesIDKey + "\n";
        //            }
        //            msg2 += "Current Influences:\n";
        //            foreach (var inf in pair2.Value.CurrentInfluences)
        //            {
        //                decimal targetinf;
        //                if (!pair2.Value.TargetInfluences.TryGetValue(inf.Key, out targetinf))
        //                    targetinf = 0m;
        //                msg2 += inf.Key + ", " + inf.Value.ToString() + " => " + targetinf + " | Days Since change: " + pair2.Value.daysSinceTargetsChanged + " | CurrentID: " + pair2.Value.expectedIDKey + " | TargetID: " + pair2.Value.TargetInfluencesIDKey + "\n";
        //            }
        //            msg2 += "\n";
        //        }
        //        string file = $@"C:\Users\{Environment.UserName}\Desktop\debug.txt";
        //        string file2 = $@"C:\Users\{Environment.UserName}\Desktop\debugPrevious.txt";
        //        if (System.IO.File.Exists(file))
        //        {
        //            if (System.IO.File.Exists(file2))
        //                System.IO.File.Delete(file2);

        //            System.IO.File.Copy(file, file2);
        //            System.IO.File.Delete(file);
        //        }

        //        System.IO.File.WriteAllText(file, msg2);

        //        //System.Windows.Forms.MessageBox.Show(msg);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        /// <summary>
        /// When the game saves, we will also save the culture status
        /// </summary>
        public void OnSave()
        {
            FileUtil.SaveSerializedFile(Hero.MainHero.Name.ToString()/*, DynaCultureManager.Instance.InitializeTime*/, DynaCultureManager.Instance);
        }
    }
}
