﻿using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;

namespace DynaCulture.Patch
{
    [HarmonyPatch]
    class CaravanPartyComponentPatch
    {
        [HarmonyPatch(typeof(CaravanPartyComponent), "InitializeCaravanOnCreation")]
        [HarmonyPrefix]
        private static bool InitializeCaravanOnCreation(CaravanPartyComponent __instance, 
            MobileParty mobileParty,
            Hero caravanLeader,
            ItemRoster caravanItems,
            int troopToBeGiven,
            bool isElite)
        {
            if (mobileParty.Party.Owner.Culture.CaravanPartyTemplate == null || mobileParty.Party.Owner.Culture.EliteCaravanPartyTemplate == null)
                return false;

            //We continue to call InitializeCaravanOnCreation
            return true;

        }    

    }
}
