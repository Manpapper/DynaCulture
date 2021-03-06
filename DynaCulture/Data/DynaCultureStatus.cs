﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem;

using DynaCulture.Util;

namespace DynaCulture.Data
{
    [Serializable]
    class DynaCultureStatus
    {
        [field:NonSerialized]
        const int BASE_INFLUENCE = 1;

        [field: NonSerialized]
        static Dictionary<string, CultureObject> _cachedCultures;

        static Dictionary<string, CultureObject> CachedCultures
        {
            get
            {
                if (_cachedCultures == null || _cachedCultures.Count == 0)
                    initializeCultures();

                return _cachedCultures;
            }
        }

        // Serialized fields
        internal string homelandCultureId;
        internal string settlementId;
        internal int expectedIDKey = 0;
        internal int TargetInfluencesIDKey = 0;
        internal int daysSinceTargetsChanged = 0;
        internal Dictionary<string, decimal> CurrentInfluences;
        internal Dictionary<string, decimal> TargetInfluences;

        public DynaCultureStatus(Settlement settlement)
        {
            this.settlementId = settlement.StringId;
            this.homelandCultureId = settlement.Culture.StringId;
        }

        /// <summary>
        /// On campaign start, if there was no CultureConfig.dat, lookups need to be initialized
        /// </summary>
        public void OnCampaignLoad()
        {
            _cachedCultures = new Dictionary<string, CultureObject>();

            // There was no current influence stored in the save file
            if (CurrentInfluences == null)
            {
                Settlement settlement = Settlement.Find(settlementId);

                CurrentInfluences = new Dictionary<string, decimal>();
                CurrentInfluences.Add(settlement.Culture.StringId, 1m);
            }

            // there was no target influence stored in the save file
            if (TargetInfluences == null)
            {
                TargetInfluences = new Dictionary<string, decimal>();
                RecalculateInfluencers(true);
            }
        }

        /// <summary>
        /// Every day this settlement will progress its CurrentInfluence towards its TargetInfluence
        /// </summary>
        public void OnDailyTick()
        {
            int influenceSum = RecalculateInfluencers(false);

            if (Settings.Instance.GradualAssimilation)
            {
                // detect if there were ownership changes around us
                if (expectedIDKey != TargetInfluencesIDKey)
                {
                    daysSinceTargetsChanged = 0;
                    expectedIDKey = TargetInfluencesIDKey;
                }
                else
                {
                    daysSinceTargetsChanged++;
                }

                // each day we will move 1/[setting] of the way towards the target
                decimal assimilationRate = Math.Min((decimal)influenceSum / Settings.Instance.AssimilationDelay, 1);

                // Iterate each pressuring influence
                foreach (var targetInfluence in TargetInfluences)
                {
                    // If we already have some of that culture
                    if (CurrentInfluences.ContainsKey(targetInfluence.Key))
                    {
                        // today we will shift this culture by (difference) * (rate)
                        decimal assimilationDelta = (targetInfluence.Value - CurrentInfluences[targetInfluence.Key]) * assimilationRate;

                        CurrentInfluences[targetInfluence.Key] += assimilationDelta;
                    }
                    else
                    {
                        // Today we will shift this culture by (target) * (rate)
                        decimal assimilationDelta = targetInfluence.Value * assimilationRate;

                        // Always add a previously nonexistant culture
                        CurrentInfluences.Add(targetInfluence.Key, assimilationDelta);
                    }
                }

                // Clear out no-influence cultures
                for (int x = 0; x < CurrentInfluences.Count; x++)
                {
                    var currentInfluence = CurrentInfluences.ElementAt(x);
                    if (currentInfluence.Value < 0)
                    {
                        CurrentInfluences.Remove(CurrentInfluences.ElementAt(x).Key);
                        x--;
                    }
                }
            }
            else
                CurrentInfluences = TargetInfluences;

            applyCulture();
        }

        /// <summary>
        /// Recalculate influence based on campaign circumstances and save it as "TargetInfluence"
        /// </summary>
        /// <param name="firstTimeSetup"></param>
        /// <returns></returns>
        public int RecalculateInfluencers(bool firstTimeSetup)
        {
            Settlement settlement = Settlement.Find(settlementId);

            // foreach nearby settlement, sum up the influence of each culture type
            List<Settlement> influencingSettlements = new List<Settlement>();

            // Geographically close settlements
            influencingSettlements.AddRange(Settlement.FindSettlementsAroundPosition(settlement.Position2D, Settings.Instance.SettlementInfluenceRange).Where(x => x.IsVillage || x.IsCastle || x.IsTown));

            if (Settings.Instance.TradeLinkedInfluence)
            {
                // Trade-linked settlements
                if (settlement.IsCastle || settlement.IsTown)
                    influencingSettlements.AddRange(settlement.BoundVillages.Select(v => v.Settlement));

                // Parent's Trade-linked settlements
                if (settlement.IsVillage)
                    influencingSettlements.AddRange(Settlement.FindFirst(s => s.BoundVillages.Contains(settlement.Village)).BoundVillages.Select(v => v.Settlement));
            }

            // calculate the influence caused by our neighbors
            int sum = 0;
            Dictionary<CultureObject, int> influencers = new Dictionary<CultureObject, int>();
            foreach (var otherSettlement in influencingSettlements)
            {
                int influence = getInfluenceFromSettlement(otherSettlement, settlement, firstTimeSetup);
                sum += influence;

                if (!influencers.ContainsKey(otherSettlement.Culture))
                    influencers.Add(otherSettlement.Culture, influence);
                else
                    influencers[otherSettlement.Culture] += influence;
            }

            // calculate the influence caused by our owners
            int ownerInfluence = BASE_INFLUENCE * Settings.Instance.OwnerInfluenceStrength;
            sum += ownerInfluence;

            if (ownerInfluence > 0)
            {
                var ownerCulture = DynaCultureUtils.GetOwnerCulture(settlement);

                if (!influencers.ContainsKey(ownerCulture))
                    influencers.Add(ownerCulture, ownerInfluence);
                else
                    influencers[ownerCulture] += ownerInfluence;
            }

            // foreach influencing culture, calculate the percent of total influence
            bool didTargetsChange = false;
            foreach (var pair in influencers)
            {
                CultureObject culture = pair.Key;
                decimal percent = (decimal)pair.Value / sum;

                if (!TargetInfluences.ContainsKey(culture.StringId))
                {
                    TargetInfluences.Add(culture.StringId, percent);
                    didTargetsChange = true;
                }
                else if (percent != TargetInfluences[culture.StringId])
                {
                    TargetInfluences[culture.StringId] = percent;
                    didTargetsChange = true;
                }
            }

            // clear out no longer influencing cultures
            for (int x = 0; x < TargetInfluences.Count; x++)
            {
                if (!influencers.Select(c => c.Key.StringId).Contains(TargetInfluences.ElementAt(x).Key))
                    TargetInfluences[TargetInfluences.ElementAt(x).Key] = 0m;
            }

            if (didTargetsChange)
                TargetInfluencesIDKey++;

            return sum;
        }

        /// <summary>
        /// Get the influence ratio of the top culture for this settlement
        /// </summary>
        /// <returns></returns>
        public decimal GetPercentOfTopCulture()
        {
            return CurrentInfluences[getTopCulture().StringId];
        }

        /// <summary>
        /// Initializes the culture cache
        /// </summary>
        static void initializeCultures()
        {
            foreach (var culture in Campaign.Current.Kingdoms.Where(x => x.IsKingdomFaction && x.Culture != null).Select(x => x.Culture).Distinct())
            {
                if (!_cachedCultures.ContainsKey(culture.StringId))
                    _cachedCultures.Add(culture.StringId, culture);
            }
        }

        /// <summary>
        /// Returns the most dominant culture for this settlement
        /// </summary>
        /// <returns></returns>
        CultureObject getTopCulture()
        {
            var top = CurrentInfluences.OrderByDescending(x => x.Value).First();

            // Settle ties...
            var tied = CurrentInfluences.Where(x => x.Value == top.Value);
            if (tied.Count() > 1)
            {
                // Take the homeland if it exists
                if (tied.Any(x => x.Key == homelandCultureId))
                    top = tied.Where(x => x.Key == homelandCultureId).First();
                // Otherwise pick the top alphabetically
                else
                    top = tied.OrderBy(x => x.Key).First();
            }

            var topCulture = CachedCultures[top.Key];

            return topCulture;
        }

        /// <summary>
        /// If the top culture is different than the current culture, it will be changed here
        /// </summary>
        void applyCulture()
        {
            Settlement settlement = Settlement.Find(settlementId);
            CultureObject topCulture = getTopCulture();
            if (settlement.Culture.StringId != topCulture.StringId)
            {
                DynaCultureUtils.ChangeSettlementCulture(settlement, topCulture);
            }
        }

        /// <summary>
        /// Gets the influence strength from otherSettlement on thisSettlement
        /// </summary>
        /// <param name="otherSettlement">Influencer</param>
        /// <param name="thisSettlement">Influencee</param>
        /// <param name="firstTimeSetup">First time setup will ignore the Ratio calculation, because the InfluenceMap has not been fully populated</param>
        /// <returns></returns>
        int getInfluenceFromSettlement(Settlement otherSettlement, Settlement thisSettlement, bool firstTimeSetup)
        {
            var otherCulture = DynaCultureUtils.GetOwnerCulture(otherSettlement);

            int influenceValue = BASE_INFLUENCE;

            if (otherSettlement.IsTown)
                influenceValue *= 3;
            if (otherSettlement.IsCastle)
                influenceValue *= 2;
            if (otherSettlement.IsVillage)
                influenceValue *= 1;

            // Self influence bonus
            if (otherSettlement.StringId == this.settlementId)
                influenceValue *= 2;

            // Homeland bonus only applied for self influence
            if (otherCulture.StringId == homelandCultureId && otherSettlement.StringId == thisSettlement.StringId)
                influenceValue *= 2;

            // Scale the influence based on the influence ratios of the other settlement. Skip on first time setup.
            if (!firstTimeSetup)
                influenceValue = (int)decimal.Round((decimal)influenceValue * DynaCultureManager.Instance.InfluenceMap[otherSettlement.StringId].GetPercentOfTopCulture());

            return influenceValue;
        }

        public Dictionary<string, decimal> getInfluenceForSettlement()
        {
            return CurrentInfluences.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value); ;
        }

        public decimal getTargetInfluenceValueForSettlement(string culture)
        {
            return TargetInfluences.Where(x => x.Key == culture).OrderByDescending(x => x.Value).First().Value;
        }
    }
}
