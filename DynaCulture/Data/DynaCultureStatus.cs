using System;
using System.Collections.Generic;
using System.Linq;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

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
        internal Dictionary<string, decimal> PreviousInfluences;
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
            // There was no current influence stored in the save file
            if (CurrentInfluences == null)
            {
                Settlement settlement = Settlement.Find(settlementId);

                CurrentInfluences = new Dictionary<string, decimal>();
                CurrentInfluences.Add(settlement.Culture.StringId, 1m);
            }

            // There was no previous influence stored in the save file
            if (PreviousInfluences == null)
            {
                PreviousInfluences = new Dictionary<string, decimal>();
            }

            // there was no target influence stored in the save file
            if (TargetInfluences == null)
            {
                TargetInfluences = new Dictionary<string, decimal>();
                recalculateInfluencers(true);
            }
        }

        /// <summary>
        /// Every day this settlement will progress its CurrentInfluence towards its TargetInfluence
        /// </summary>
        public void OnDailyTick()
        {
            int influenceSum = recalculateInfluencers(false);

            RemoveNonRevelentCulture(CurrentInfluences);

            PreviousInfluences = new Dictionary<string, decimal>(CurrentInfluences);

            if (DynaCultureSettings.Instance.GradualAssimilation)
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
                decimal assimilationRate = Math.Min((decimal)influenceSum / DynaCultureSettings.Instance.AssimilationDelay, 1);

                // Iterate each pressuring influence
                foreach (KeyValuePair<string, decimal> targetInfluence in TargetInfluences.ToList())
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

            }
            else
                CurrentInfluences = TargetInfluences;

            applyCulture();
        }

        public decimal GetPreviousInfluenceForCulture(string culture)
        {
            decimal val;
            if (PreviousInfluences != null && PreviousInfluences.TryGetValue(culture, out val))
                return val;
            if (CurrentInfluences.TryGetValue(culture, out val))
                return val;
            return 0m;
        }

        /// <summary>
        /// Recalculate influence based on campaign circumstances and save it as "TargetInfluence"
        /// </summary>
        /// <param name="firstTimeSetup"></param>
        /// <returns></returns>
        int recalculateInfluencers(bool firstTimeSetup)
        {
            Settlement settlement = Settlement.Find(settlementId);

            // foreach nearby settlement, sum up the influence of each culture type
            List<Settlement> influencingSettlements = new List<Settlement>();

            // Geographically close settlements
            influencingSettlements.AddRange(Settlement.FindSettlementsAroundPosition(settlement.Position2D, DynaCultureSettings.Instance.SettlementInfluenceRange).Where(x => x.IsVillage || x.IsCastle || x.IsTown));

            if (DynaCultureSettings.Instance.TradeLinkedInfluence)
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
            int ownerInfluence = BASE_INFLUENCE * DynaCultureSettings.Instance.OwnerInfluenceStrength;
            sum += ownerInfluence;

            if (ownerInfluence > 0)
            {
                var ownerCulture = DynaCultureUtils.GetOwnerCulture(settlement);

                if (!influencers.ContainsKey(ownerCulture))
                    influencers.Add(ownerCulture, ownerInfluence);
                else
                    influencers[ownerCulture] += ownerInfluence;
            }

            //calculate the influence cause by governors
            if (DynaCultureSettings.Instance.GovernorCultureInfluencePlayerSettlementOnly && DynaCultureUtils.IsPlayerOwner(settlement))
            {
                sum += CalculateGovernorInfluence(settlement, influencers);
            }
            else if (DynaCultureSettings.Instance.GovernorCultureInfluencePlayerSettlementOnly == false)
            {
                sum += CalculateGovernorInfluence(settlement, influencers);
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
        decimal getTopCultureValue()
        {
            return CurrentInfluences[getTopCulture().StringId];
        }

        /// <summary>
        /// Initializes the culture cache
        /// </summary>
        public static void initializeCultures()
        {
            _cachedCultures = new Dictionary<string, CultureObject>();
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
            if(CurrentInfluences.Count == 0)
            {
                recalculateInfluencers(true);
                CurrentInfluences = TargetInfluences;

                if (CurrentInfluences.Count == 0)
                {
                    CurrentInfluences.Add(Settlement.Find(settlementId).OwnerClan.Kingdom.Culture.StringId, 1);
                }
            }

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
        public void applyCulture()
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
                influenceValue = (int)decimal.Round((decimal)influenceValue * DynaCultureManager.Instance.InfluenceMap[otherSettlement.StringId].getTopCultureValue());

            return influenceValue;
        }

        private void RemoveNonRevelentCulture(Dictionary<string, decimal> currentInfluences)
        {
            //Remove current influence where influence is less than 0.001
            for (int x = 0; x < currentInfluences.Count; x++)
            {                
                if (currentInfluences.ElementAt(x).Value < 1m / (decimal)Math.Pow(10, 3))
                {
                    if(PreviousInfluences.ContainsKey(currentInfluences.ElementAt(x).Key))
                        PreviousInfluences.Remove(currentInfluences.ElementAt(x).Key);

                    if(TargetInfluences.ContainsKey(currentInfluences.ElementAt(x).Key))
                        TargetInfluences.Remove(currentInfluences.ElementAt(x).Key);

                    currentInfluences.Remove(currentInfluences.ElementAt(x).Key);
                    x--;
                }
            }

            //Remove current influence which are no more in target
            for (int x = 0; x < currentInfluences.Count; x++)
            {                
                if (!TargetInfluences.ContainsKey(currentInfluences.ElementAt(x).Key))
                {
                    if (PreviousInfluences.ContainsKey(currentInfluences.ElementAt(x).Key))
                        PreviousInfluences.Remove(currentInfluences.ElementAt(x).Key);

                    currentInfluences.Remove(currentInfluences.ElementAt(x).Key);
                    x--;
                }
            }
        }

        private int CalculateGovernorInfluence(Settlement settlement, Dictionary<CultureObject, int> influencers)
        {
            int governorInfluence = 0;

            //calculate the influence cause by governors
            if (DynaCultureSettings.Instance.GovernorInfluenceStrength != 0)
            {
                var governorCulture = DynaCultureUtils.GetGovernorCulture(settlement);
                if (governorCulture != null)
                {
                    governorInfluence = BASE_INFLUENCE * DynaCultureSettings.Instance.GovernorInfluenceStrength;

                    if (!influencers.ContainsKey(governorCulture))
                        influencers.Add(governorCulture, governorInfluence);
                    else
                        influencers[governorCulture] += governorInfluence;
                }
            }

            return governorInfluence;
        }
    }
}
