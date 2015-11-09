using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;

namespace WG_BalancedPopMod
{
    class IndustrialExtractorAIMod : IndustrialExtractorAI
    {
        private const int EXTRACT_LEVEL = 0; // Extracting is always level 1 (To make it easier to code)
        private static Dictionary<ulong, employStruct> employCache = new Dictionary<ulong, employStruct>(DataStore.CACHE_SIZE);
        private static Dictionary<ulong, consumeStruct> consumeCache = new Dictionary<ulong, consumeStruct>(DataStore.CACHE_SIZE);

        public static void clearCache()
        {
            employCache.Clear();
            consumeCache.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="level0"></param>
        /// <param name="level1"></param>
        /// <param name="level2"></param>
        /// <param name="level3"></param>
        public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            ulong seed = r.seed;
            BuildingInfo item = this.m_info;
            int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

            employStruct output;
            bool needRefresh = true;

            if (employCache.TryGetValue(seed, out output))
            {
                needRefresh = output.level != level;
            }

            if (needRefresh)
            {
                employCache.Remove(seed);
                consumeCache.Remove(seed);
                int[] array = getArray(item.m_class, EXTRACT_LEVEL);

                int num = array[DataStore.PEOPLE];
                level0 = array[DataStore.WORK_LVL0];
                level1 = array[DataStore.WORK_LVL1];
                level2 = array[DataStore.WORK_LVL2];
                level3 = array[DataStore.WORK_LVL3];
                int num2 = level0 + level1 + level2 + level3;

                if (num > 0 && num2 > 0)
                {
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

                    int value = ((x * z * Mathf.CeilToInt(v.y / array[DataStore.LEVEL_HEIGHT])) / array[DataStore.PEOPLE]);
                    num = Mathf.Max(2, value);  // Minimum of two

                    level3 = (num * level3) / num2;
                    level2 = (num * level2) / num2;
                    level1 = (num * level1) / num2;
                    level0 = Mathf.Max(0, num - level3 - level2 - level1);  // Whatever is left
                }
                else
                {
                    level0 = level1 = level2 = level3 = 1;  // Allocate 1 for every level, to stop div by 0
                }

                output.level = level;
                output.level0 = level0;
                output.level1 = level1;
                output.level2 = level2;
                output.level3 = level3;

                employCache.Add(seed, output);
            }
            else
            {
                level0 = output.level0;
                level1 = output.level1;
                level2 = output.level2;
                level3 = output.level3;
            }
        }


        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            employCache.Remove(new Randomizer((int)buildingID).seed);
            consumeCache.Remove(new Randomizer((int)buildingID).seed);
            base.ReleaseBuilding(buildingID, ref data);
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
            ulong seed = r.seed;
            ItemClass item = this.m_info.m_class;
            consumeStruct output;
            bool needRefresh = true;

            if (consumeCache.TryGetValue(seed, out output))
            {
                needRefresh = output.productionRate != productionRate;
            }

            if (needRefresh)
            {
                consumeCache.Remove(seed);
                int[] array = getArray(item, EXTRACT_LEVEL);

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

                output.productionRate = productionRate;
                output.electricity = electricityConsumption;
                output.water = waterConsumption;
                output.sewage = sewageAccumulation;
                output.garbage = garbageAccumulation;
                output.income = incomeAccumulation;

                consumeCache.Add(seed, output);
            }
            else
            {
                productionRate = output.productionRate;
                electricityConsumption = output.electricity;
                waterConsumption = output.water;
                sewageAccumulation = output.sewage;
                garbageAccumulation = output.garbage;
                incomeAccumulation = output.income;
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
            groundPollution = 0;
            noisePollution = 0;
            int[] array = getArray(@class, EXTRACT_LEVEL);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int[] getArray(ItemClass item, int level)
        {
            switch (item.m_subService)
            {
                case ItemClass.SubService.IndustrialOre:
                    return DataStore.industry_ore[level];

                case ItemClass.SubService.IndustrialForestry:
                    return DataStore.industry_forest[level];

                case ItemClass.SubService.IndustrialFarming:
                    return DataStore.industry_farm[level];

                case ItemClass.SubService.IndustrialOil:
                    return DataStore.industry_oil[level];

                case ItemClass.SubService.IndustrialGeneric:  // Deliberate fall through
                default:
                    return DataStore.industry[level];
            }
        }
    }
}