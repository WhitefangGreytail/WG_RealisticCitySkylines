using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;
using System.IO;
using WG_BalancedPopMod;
using Boformer.Redirection;

namespace WG_BalancedPopMod
{
    [TargetType(typeof(ResidentialBuildingAI))]
    class ResidentialBuildingAIMod : ResidentialBuildingAI
    {
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [RedirectMethod(true)]
        public override int CalculateHomeCount(Randomizer r, int width, int length)
        {
            BuildingInfo item = this.m_info;
            int returnValue = 1;

            if (!DataStore.prefabHouseHolds.TryGetValue(item.gameObject.GetHashCode(), out returnValue))
            {
                returnValue = AI_Utils.calculatePrefabHousehold(width, length, ref item);
                DataStore.prefabHouseHolds.Add(item.gameObject.GetHashCode(), returnValue);
            }
            return returnValue;
        }


        /// <param name="r"></param>
        /// <param name="productionRate"></param>
        /// <param name="electricityConsumption"></param>
        /// <param name="waterConsumption"></param>
        /// <param name="sewageAccumulation"></param>
        /// <param name="garbageAccumulation"></param>
        /// <param name="incomeAccumulation"></param>
        [RedirectMethod(true)]
        public override void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
        {
            ItemClass item = this.m_info.m_class;
            
            int level = (int)(item.m_level >= 0 ? item.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = (item.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];
            electricityConsumption = array[DataStore.POWER];
            waterConsumption = array[DataStore.WATER];
            sewageAccumulation = array[DataStore.SEWAGE];
            garbageAccumulation = array[DataStore.GARBAGE];
            incomeAccumulation = array[DataStore.INCOME];

            electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption) / 100;
            waterConsumption = Mathf.Max(100, productionRate * waterConsumption) / 100;
            sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation) / 100;
            garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation) / 100;
            incomeAccumulation = productionRate * incomeAccumulation;
        }


        /// <param name="productionRate"></param>
        /// <param name="cityPlanningPolicies"></param>
        /// <param name="groundPollution"></param>
        /// <param name="noisePollution"></param>
        [RedirectMethod(true)]
        public override void GetPollutionRates(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = this.m_info.m_class;
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = (@class.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];
        }
    }
}