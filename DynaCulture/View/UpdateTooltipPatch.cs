﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using DynaCulture.Data;

namespace DynaCulture.View
{
    [HarmonyPatch(typeof(TooltipVM), "OpenTooltip")]
    public class UpdateTooltipPatch
    {
        [HarmonyPostfix]
        private static void UpdateTooltipPostfix(TooltipVM __instance, Type type, object[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine(args[0].GetType());
                    if (args[0].GetType() == typeof(Settlement) && (Settlement)args[0] != null)
                    {
                        Settlement settlement = (Settlement) args[0];

                        if(settlement.IsCastle || settlement.IsTown || settlement.IsVillage)
                        {
                            string factionDefinitionLabel = "Faction";
                            string troopTypesDefinitionLabel = "Troop Types";

                            int indexOfFaction = __instance.TooltipPropertyList.FindIndex(x => x.DefinitionLabel.Equals(factionDefinitionLabel));
                            MBBindingList<TooltipProperty> tooltipProperties = __instance.TooltipPropertyList;

                            if (indexOfFaction != 0)
                            {
                                //Add Culture to tooltip
                                if (settlement.Culture != null)
                                {
                                    tooltipProperties.Insert(indexOfFaction, new TooltipProperty("Culture", settlement.Culture.ToString(), 0, false));
                                }
                            }
                            __instance.TooltipPropertyList = tooltipProperties;

                            List<TooltipProperty> tooltipPropertyMoreInfoList = __instance.TooltipPropertyList.ToList();

                            //Add Influences to tooltip
                            Dictionary<string, decimal> influences = DynaCultureManager.Instance.InfluenceMap[settlement.StringId].getInfluenceForSettlement();

                            if (influences.Count != 0)
                            {
                                int indexForInfluence = __instance.TooltipPropertyList.FindIndex(x => x.DefinitionLabel.Equals(troopTypesDefinitionLabel));
                                if(indexForInfluence != 0)
                                {
                                    tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty("Influences", " ", 0, true));
                                    indexForInfluence++;
                                    tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty("", "", 0, true, TooltipProperty.TooltipPropertyFlags.RundownSeperator));
                                    indexForInfluence++;

                                    foreach (KeyValuePair<string, decimal> influence in influences)
                                    {
                                        decimal targetInfluenceValue = DynaCultureManager.Instance.InfluenceMap[settlement.StringId].getTargetInfluenceValueForSettlement(influence.Key) * 100;
                                        decimal influenceValue = influence.Value * 100;
                                        decimal differenceInfluenceValue = influenceValue - targetInfluenceValue;
                                        string culture = "";

                                        if (influence.Key.Length > 1)
                                            culture = char.ToUpper(influence.Key[0]) + influence.Key.Substring(1);
                                        else
                                            culture = influence.Key;

                                        tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty($"{culture}", $"{influenceValue.ToString("0.##")} ({differenceInfluenceValue.ToString("0.##")})", 0, true));
                                        indexForInfluence++;
                                    }

                                    tooltipPropertyMoreInfoList.Insert(indexForInfluence, new TooltipProperty("", "", -1, true));
                                    indexForInfluence++;
                                }

                            }

                            __instance.TooltipPropertyList = new MBBindingList<TooltipProperty>();
                            __instance.UpdateTooltip(tooltipPropertyMoreInfoList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }

        }
    }
}