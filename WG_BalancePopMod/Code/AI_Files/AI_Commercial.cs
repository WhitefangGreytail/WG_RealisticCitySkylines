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
        private static Dictionary<int, employStruct> employCache = new Dictionary<int, employStruct>(DataStore.CACHE_SIZE);
        private static Dictionary<int, consumeStruct> consumeCache = new Dictionary<int, consumeStruct>(DataStore.CACHE_SIZE);

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
            BuildingInfo item = this.m_info;
            int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

            employStruct output;
            bool needRefresh = true;

            if (employCache.TryGetValue(item.GetInstanceID(), out output))
            {
                needRefresh = output.level != level;
            }

            if (needRefresh)
            {
                employCache.Remove(item.GetInstanceID());
                consumeCache.Remove(item.GetInstanceID());
                int[] array = getArray(item.m_class, ref level);

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

    /*                string file = "ComLow.txt";
                    if (item.m_class.m_subService == ItemClass.SubService.CommercialHigh)
                        file = "ComHigh.txt";
                    else if (item.m_class.m_subService == ItemClass.SubService.CommercialTourist)
                        file = "ComTourist.txt";
                    else if (item.m_class.m_subService == ItemClass.SubService.CommercialLeisure)
                        file = "ComLeisure.txt";
                    Debugging.writeDebugToFile("level: " + level + ", w/l/h/f: " + (int)item.m_size.x + " * " + (int)item.m_size.z + " * " + (int)item.m_size.y + " / " + Mathf.CeilToInt(item.m_size.y / array[DataStore.LEVEL_HEIGHT]) + ", workers: " + value, file);
     */
                    num = Mathf.Max(3, value);  // Minimum of three

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

                employCache.Add(item.GetInstanceID(), output);
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
            employCache.Remove(this.m_info.GetInstanceID());
            consumeCache.Remove(this.m_info.GetInstanceID());
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
            ItemClass item = this.m_info.m_class;
            consumeStruct output;
            bool needRefresh = true;

            if (consumeCache.TryGetValue(item.GetInstanceID(), out output))
            {
                needRefresh = output.productionRate != productionRate;
            }

            if (needRefresh)
            {
                consumeCache.Remove(item.GetInstanceID());

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

                consumeCache.Add(item.GetInstanceID(), output);
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