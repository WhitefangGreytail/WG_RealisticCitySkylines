using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;
using System.IO;

namespace WG_BalancedPopMod
{
    class ResidentialBuildingAIMod : ResidentialBuildingAI
    {
        // Might be able to use the same one for all types
        private static Dictionary<int, int>[] availableFloorSpace = { new Dictionary<int, int>(), new Dictionary<int, int>(), new Dictionary<int, int>(), new Dictionary<int, int>(), new Dictionary<int, int>() };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override int CalculateHomeCount(Randomizer r, int width, int length)
        {
            BuildingInfo item = this.m_info;
            int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

            int[] array = DataStore.residentialLow[level];
            if (item.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
            {
                array = DataStore.residentialHigh[level];
            }

            // Check x and z just incase they are 0. Extremely few are. If they are, then base the calculation of 3/4 of the width and length given
            Vector3 v = item.m_size;
            int x = (int)v.x;
            int z = (int)v.z;

            if (x <= 0)
            {
                x = width * 6;
            }
            if (z <= 0) 
            {
                z = length * 6;
            }

            int returnValue = ((x * z * Mathf.CeilToInt(v.y / array[DataStore.LEVEL_HEIGHT])) / array[DataStore.PEOPLE]);
            return Mathf.Max(1, returnValue);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="productionRate"></param>
        /// <param name="electricityConsumption"></param>
        /// <param name="waterConsumption"></param>
        /// <param name="sewageAccumulation"></param>
        /// <param name="garbageAccumulation"></param>
        /// <param name="incomeAccumulation"></param>
        public override void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
        {
            ItemClass @class = this.m_info.m_class;
            int level = (int) (@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = (@class.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];
            electricityConsumption = array[DataStore.POWER];
            waterConsumption = array[DataStore.WATER];
            sewageAccumulation = array[DataStore.SEWAGE];
            garbageAccumulation = array[DataStore.GARBAGE];
            incomeAccumulation = array[DataStore.INCOME];

            if (electricityConsumption != 0)
            {
                electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption + r.Int32(100u)) / 100;
            }
            if (waterConsumption != 0)
            {
                int num = r.Int32(100u);
                waterConsumption = Mathf.Max(100, productionRate * waterConsumption + num) / 100;
                if (sewageAccumulation != 0)
                {
                    sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + num) / 100;
                }
            }
            else if (sewageAccumulation != 0)
            {
                sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation + r.Int32(100u)) / 100;
            }
            if (garbageAccumulation != 0)
            {
                garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation + r.Int32(100u)) / 100;
            }
            if (incomeAccumulation != 0)
            {
                incomeAccumulation = productionRate * incomeAccumulation;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionRate"></param>
        /// <param name="cityPlanningPolicies"></param>
        /// <param name="groundPollution"></param>
        /// <param name="noisePollution"></param>
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