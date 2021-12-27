using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

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
