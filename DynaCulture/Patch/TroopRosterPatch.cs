using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;

namespace DynaCulture.Patch
{
    [HarmonyPatch]
    class TroopRosterPatch
    {
        [HarmonyPatch(typeof(TroopRoster), "ClampXp")]
        [HarmonyPrefix]
        private static bool ClampXp(TroopRoster __instance, int index)
        {
            CharacterObject troop = __instance.GetCharacterAtIndex(index);

            //We skip the ClampXp call since it will crash with corrupted troop
            if (troop.Age == 0)
                return false;

            //We continue to call ClampXp
            return true;

        }

    }
}
