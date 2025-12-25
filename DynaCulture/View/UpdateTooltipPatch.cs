using System;
using System.Collections.Generic;
using System.Linq;
using DynaCulture.Data;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace DynaCulture.View
{
    [HarmonyPatch(typeof(PropertyBasedTooltipVM), "Refresh")]
    public class UpdateTooltipPatch
    {
        [HarmonyPostfix]
        private static void RefreshPostfix(PropertyBasedTooltipVM __instance)
        {
            Type _shownType = (Type)__instance.GetType().GetField("_invokedType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            object[] args = (object[])__instance.GetType().GetField("_invokedArgs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            
            if (_shownType == typeof(Settlement))
            {
                if (args[0].GetType() == typeof(Settlement) && (Settlement)args[0] != null)
                {
                    try
                    {
                        if (!(_shownType == typeof(Settlement)) || (args[0] == null || !(args[0].GetType() == typeof(Settlement))))
                            return;
                        Settlement settlement = (Settlement)args[0];
                        if (settlement.IsCastle || settlement.IsTown || settlement.IsVillage)
                        {
                            bool influencesInDetails = DynaCultureSettings.Instance.ShowInfluencesInDetails;
                            __instance.TooltipPropertyList.ToList<TooltipProperty>();
                            Dictionary<string, Decimal> currentInfluences = DynaCultureManager.Instance.InfluenceMap[settlement.StringId].CurrentInfluences;
                            if (currentInfluences.Count > 0)
                            {
                                int num1 = 0;
                                if (__instance.TooltipPropertyList.Count != 0 && __instance.TooltipPropertyList.Count >= 2)
                                    num1 = __instance.TooltipPropertyList.Count - 2;

                                if (num1 >= 0)
                                {
                                    __instance.TooltipPropertyList.Insert(num1, new TooltipProperty("", "", -1, influencesInDetails));
                                    num1++;
                                    TextObject influenceText = new TextObject("{=ieJye6ml32N}Influences");
                                    __instance.TooltipPropertyList.Insert(num1,new TooltipProperty(influenceText.ToString(), " ", 0, influencesInDetails));
                                    num1++;
                                    __instance.TooltipPropertyList.Insert(num1, new TooltipProperty("", "", 0, influencesInDetails, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
                                    foreach(KeyValuePair<string, Decimal>  currentInfluence in currentInfluences)
                                    {
                                        Decimal num2 = DynaCultureManager.Instance.InfluenceMap[settlement.StringId].GetPreviousInfluenceForCulture(currentInfluence.Key) * 100M;
                                        Decimal num3 = currentInfluence.Value * 100M;
                                        Decimal num4 = num3 - num2;
                                        string empty = string.Empty;
                                        string str = currentInfluence.Key.Length <= 1 ? currentInfluence.Key : DynaCultureStatus.getCultureNameById(currentInfluence.Key).ToString();
                                        num1++;
                                        __instance.TooltipPropertyList.Insert(num1, new TooltipProperty(str ?? "", num3.ToString("0.##") + " (" + num4.ToString("0.##") + ")", 0, influencesInDetails));
                                    }
                                    num1++;
                                    __instance.TooltipPropertyList.Insert(num1, new TooltipProperty("", "", -1, influencesInDetails));
                                }
                            }
                        }
                    }
                    catch (Exception e) { }                    
                }
            }
        }
    }
}