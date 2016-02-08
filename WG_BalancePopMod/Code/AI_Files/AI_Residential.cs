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
        // CalculateHomeCount is only called once in construction or upgrading. Only store consumption
        private static Dictionary<ulong, consumeStruct> consumeCache = new Dictionary<ulong, consumeStruct>(DataStore.CACHE_SIZE);

        // Stores the number of households for a prefab
        private static Dictionary<int, int> prefabHouseHolds = new Dictionary<int, int>(256);  // Shouldn't need huge amounts

        public static void clearCache()
        {
            consumeCache.Clear();
            prefabHouseHolds.Clear();
        }

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
            consumeCache.Remove(r.seed);  // Clean out the consumption cache on upgrade
            int returnValue = 1;

            if (!prefabHouseHolds.TryGetValue(item.gameObject.GetHashCode(), out returnValue))
            {
                int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

                int[] array = DataStore.residentialLow[level];
                if (item.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
                {
                    array = DataStore.residentialHigh[level];
                }

                // Check x and z just incase they are 0. A few user created assets are. If they are, then base the calculation of 3/4 of the width and length given
                Vector3 v = item.m_size;
                int x = (int)v.x;
                int z = (int)v.z;

                if (x <= 1)
                {
                    x = width * 6;
                }
                if (z <= 1) 
                {
                    z = length * 6;
                }

                int floorCount = Mathf.Max(1, Mathf.FloorToInt(v.y / array[DataStore.LEVEL_HEIGHT]));
                returnValue = (x * z * floorCount) / array[DataStore.PEOPLE];

                if (item.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
                {
                    // Minimum of 2, or ceiling of 90% number of floors, which ever is greater. This helps the 1x1 high density
                    returnValue = Mathf.Max(Mathf.Max(2, Mathf.CeilToInt(0.9f * floorCount)), returnValue);
                }
                else
                {
                    returnValue = Mathf.Max(1, returnValue);
                }

                int bonus = 0;
                if (DataStore.bonusHouseholdCache.TryGetValue(item.gameObject.name, out bonus))
                {
                    returnValue = returnValue + bonus;
                }

                if (DataStore.printResidentialNames)
                {
                    Debugging.writeDebugToFile("Requested asset name -->" + item.gameObject.name + "<--, level: " + (level + 1));
                }

                prefabHouseHolds.Add(item.gameObject.GetHashCode(), returnValue);
            }
            return returnValue;
        }


        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
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
                int level = (int)(item.m_level >= 0 ? item.m_level : 0); // Force it to 0 if the level was set to None
                int[] array = (item.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];
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
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = (@class.m_subService == ItemClass.SubService.ResidentialHigh) ? DataStore.residentialHigh[level] : DataStore.residentialLow[level];

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];
        }
    }
}