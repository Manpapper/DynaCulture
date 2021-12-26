using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Localization;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Core;

using DynaCulture.Util;
using DynaCulture.Settings;

namespace DynaCulture.Data
{
    class DynaCultureBehavior : CampaignBehaviorBase
    {
        public Dictionary<string, CharacterObject> troopMap = new Dictionary<string, CharacterObject>();
        public DynaCultureBehavior()
        {
            // Clean up any existing CultureChangeManagers from current session
            DynaCultureManager.Reset();
        }

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

            // Reset between sessions to avoid culture cross contamination
            DynaCultureManager.Reset();

            SaveAllTypeOfTroops();

            // clean up corrupted troops from previous sessions, if they exist
            foreach (var party in Campaign.Current.MobileParties)
                RemoveCorruptedTroops(party);

            // clean up corrupted recruits from previous sessions, if they exist
            foreach (var settlement in Campaign.Current.Settlements)
                RemoveCorruptedRecruits(settlement);
        }

        public void DailyTickSettlementMod(Settlement settlement)
        {
            if (!Campaign.Current.GameStarted || Campaign.Current.Settlements == null)
                return;

            if (DynaCultureManager.Instance.InfluenceMap.Count == 0)
                DynaCultureManager.Initialize();

            if (!DynaCultureSettings.Instance.PlayerKingdomOnly || (DynaCultureSettings.Instance.PlayerKingdomOnly && settlement.OwnerClan.Leader.IsHumanPlayerCharacter))
            {
                if (settlement.IsVillage || settlement.IsCastle || settlement.IsTown)
                    DynaCultureManager.Instance.InfluenceMap[settlement.StringId].OnDailyTick();
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
                    int number = roster.GetTroopCount(troop);
                    roster.RemoveTroop(troop, number);
                    roster.AddToCounts(troopMap[troop.StringId], number);
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

                    if (removed && DynaCultureSettings.Instance.ShowCorruptedTroopMessage)
                        InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Corrupted troops were cleansed from {mobileParty.Name} party.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
                }

                if (mobileParty.PrisonRoster.Count != 0)
                {
                    var removed = cleanseRoster(mobileParty.PrisonRoster);

                    if (removed && DynaCultureSettings.Instance.ShowCorruptedTroopMessage)
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

                if (removed && DynaCultureSettings.Instance.ShowCorruptedTroopMessage)
                    InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Corrupted recruits were removed from {settlement.Name}.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
            }
            catch
            {
                InformationManager.DisplayMessage(new InformationMessage(new TextObject($"(DynaCulture) Error attempting to remove corrupted recruits from {settlement.Name}.", (Dictionary<string, object>)null).ToString(), Colors.Yellow));
            }
        }

        /// <summary>
        /// When the game saves, we will also save the culture status
        /// </summary>
        public void OnSave()
        {
            FileUtil.SaveSerializedFile(Hero.MainHero.Name.ToString(), DynaCultureManager.Instance);
        }

        private void SaveAllTypeOfTroops()
        {
            foreach (MobileParty mobileParty in Campaign.Current.MobileParties)
            {

                if (mobileParty.MemberRoster.Count != 0)
                    AddTroopRosterToTroopMap(mobileParty.MemberRoster);

                if (mobileParty.PrisonRoster.Count != 0)
                    AddTroopRosterToTroopMap(mobileParty.PrisonRoster);
            }
        }

        private void AddTroopRosterToTroopMap(TroopRoster roster)
        {
            //We get only non corrupted troops
            TroopRosterElement[] troops = (TroopRosterElement[])roster.GetType().GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(roster);
            var characterObjects = troops.Where(t => t.Number > 0 && t.Character?.Age != 0f).Select(t => t.Character).ToList();

            if (characterObjects.Any())
            {
                foreach (CharacterObject troop in characterObjects)
                {
                    if (!troopMap.ContainsKey(troop.StringId))
                        troopMap.Add(troop.StringId, troop);
                }
            }
        }
    }
}
