using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

using HarmonyLib;

using DynaCulture.Data;

namespace DynaCulture.View
{
    [HarmonyPatch(typeof(PropertyBasedTooltipVM), "Refresh")]
    public class UpdateTooltipPatch
    {
        [HarmonyPostfix]
        private static void RefreshPostfix(PropertyBasedTooltipVM __instance)
        {
            Type _shownType = (Type)__instance.GetType().GetField("_shownType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            object[] args = (object[])__instance.GetType().GetField("_typeArgs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(__instance);
            
            if (_shownType == typeof(Settlement))
            {
                if (args[0].GetType() == typeof(Settlement) && (Settlement)args[0] != null)
                {
                    try
                    {
                        Settlement settlement = (Settlement)args[0];

                        if (settlement.IsCastle || settlement.IsTown || settlement.IsVillage)
                        {

                            string troopTypesDefinitionLabel = "Troops";

                            List<TooltipProperty> tooltipPropertyMoreInfoList = __instance.TooltipPropertyList.ToList();

                            //Add Influences to tooltip
                            Dictionary<string, decimal> influences = DynaCultureManager.Instance.InfluenceMap[settlement.StringId].CurrentInfluences;

                            if (influences.Count != 0)
                            {
                                int indexForInfluence = __instance.TooltipPropertyList.FindIndex(x => x.DefinitionLabel.Equals(troopTypesDefinitionLabel));
                                if (indexForInfluence == -1)
                                    indexForInfluence = __instance.TooltipPropertyList.Count;

                                if (indexForInfluence >= 0)
                                {
                                    tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty("Influences", " ", 0, true));
                                    indexForInfluence++;
                                    tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty("", "", 0, true, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
                                    indexForInfluence++;

                                    foreach (KeyValuePair<string, decimal> influence in influences.OrderByDescending(i => i.Value))
                                    {
                                        decimal previousInfluenceValue = DynaCultureManager.Instance.InfluenceMap[settlement.StringId].GetPreviousInfluenceForCulture(influence.Key) * 100;
                                        decimal influenceValue = influence.Value * 100;
                                        decimal differenceInfluenceValue = influenceValue - previousInfluenceValue;
                                        string culture = String.Empty;

                                        if (influence.Key.Length > 1)
                                            culture = char.ToUpper(influence.Key[0]) + influence.Key.Substring(1);
                                        else
                                            culture = influence.Key;

                                        tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty($"{culture}", $"{influenceValue.ToString("0.##")} ({differenceInfluenceValue.ToString("0.##")})", 0, true));
                                        indexForInfluence++;
                                    }

                                    tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty("", "", -1, true));
                                    indexForInfluence++;

                                    __instance.TooltipPropertyList = new MBBindingList<TooltipProperty>();
                                    __instance.UpdateTooltip(tooltipPropertyMoreInfoList);
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