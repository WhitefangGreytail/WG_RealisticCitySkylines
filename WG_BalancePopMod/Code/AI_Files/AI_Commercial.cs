using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;
using System.Runtime.CompilerServices; 

namespace WG_BalancedPopMod
{
    public class CommercialBuildingAIMod : CommercialBuildingAI
    {
        private static Dictionary<ulong, buildingWorkVisitorStruct> buildingEmployCache = new Dictionary<ulong, buildingWorkVisitorStruct>(DataStore.CACHE_SIZE);
        private static Dictionary<ulong, consumeStruct> consumeCache = new Dictionary<ulong, consumeStruct>(DataStore.CACHE_SIZE);

        public static void clearCache()
        {
            buildingEmployCache.Clear();
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

            bool needRefresh = true;
            buildingWorkVisitorStruct cachedLevel;
            if (buildingEmployCache.TryGetValue(seed, out cachedLevel))
            {
                needRefresh = cachedLevel.level != level;
            }

            if (needRefresh)
            {
                buildingEmployCache.Remove(seed);
                consumeCache.Remove(seed);

                prefabEmployStruct output;
                // If not seen prefab, calculate
                if (!DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out output))
                {
                    int[] array = getArray(item.m_class, ref level);
                    AI_Utils.calculateprefabWorkerVisit(width, length, ref item, 3, ref array, out output);
                    DataStore.prefabWorkerVisit.Add(item.gameObject.GetHashCode(), output);
                }

                cachedLevel.level = level;
                cachedLevel.level0 = output.level0;
                cachedLevel.level1 = output.level1;
                cachedLevel.level2 = output.level2;
                cachedLevel.level3 = output.level3;

                // Update the level for the new building
                buildingEmployCache.Add(seed, cachedLevel);
            }

            level0 = cachedLevel.level0;
            level1 = cachedLevel.level1;
            level2 = cachedLevel.level2;
            level3 = cachedLevel.level3;
        }


        public override void ReleaseBuilding(ushort buildingID, ref Building data)
        {
            buildingEmployCache.Remove(new Randomizer((int)buildingID).seed);
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
                int[] array = getArray(item, ref level);

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
                    waterConsumption = Mathf.Max(100, productionRate * waterConsumption + r.Int32(100u)) / 100;
                }
                if (sewageAccumulation != 0)
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
            ItemClass item = this.m_info.m_class;
            int level = (int)(item.m_level >= 0 ? item.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = getArray(item, ref level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;
            if (item.m_subService == ItemClass.SubService.CommercialLeisure)
            {
              if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None)
              {
                  noisePollution /= 2;
              }
            }
        }


        public override int CalculateVisitplaceCount(Randomizer r, int width, int length)
        {
            prefabEmployStruct visitors;
            if (!DataStore.prefabWorkerVisit.TryGetValue(this.m_info.gameObject.GetHashCode(), out visitors))
            {
                // All commercial places will need visitors. CalcWorkplaces is called first. But just return 0.
            }
            return visitors.visitors;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int[] getArray(ItemClass item, ref int level)
        {
            switch (item.m_subService)
            {
                case ItemClass.SubService.CommercialLeisure:
                    return DataStore.commercialLeisure[0];
                
                case ItemClass.SubService.CommercialTourist:
                    return DataStore.commercialTourist[0];
                
                case ItemClass.SubService.CommercialHigh:
                    return DataStore.commercialHigh[level];

                case ItemClass.SubService.CommercialLow:
                default:
                    return DataStore.commercialLow[level];
            }
        }
    }
}