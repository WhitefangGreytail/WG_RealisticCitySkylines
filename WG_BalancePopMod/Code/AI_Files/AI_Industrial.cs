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
    class IndustrialBuildingAIMod : IndustrialBuildingAI
    {
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
            ItemClass @class = this.m_info.m_class;
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = getArray(@class, ref level);
            int num = array[DataStore.PEOPLE];
            level0 = array[DataStore.WORK_LVL0];
            level1 = array[DataStore.WORK_LVL1];
            level2 = array[DataStore.WORK_LVL2];
            level3 = array[DataStore.WORK_LVL3];
            int num2 = level0 + level1 + level2 + level3;

            if (num > 0 && num2 > 0)
            {
                num = Mathf.Max(200, width * length * num + r.Int32(100u)) / 100;  // Minimum of two
                level3 = (num * level3 + r.Int32((uint)num2)) / num2;
                level2 = (num * level2 + r.Int32((uint)num2)) / num2;
                level1 = (num * level1 + r.Int32((uint)num2)) / num2;
                level0 = (num * level0 + r.Int32((uint)num2)) / num2;
            }
            else
            {
                level0 = level1 = level2 = level3 = 0;
            }
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
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = getArray(@class, ref level);

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
        /// <param name="groundPollution"></param>
        /// <param name="noisePollution"></param>
        public override void GetPollutionRates(int productionRate, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = this.m_info.m_class;
            groundPollution = 0;
            noisePollution = 0;
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None
            int[] array = getArray(@class, ref level);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private int[] getArray(ItemClass item, ref int level)
        {
            switch (item.m_subService)
            {
                case ItemClass.SubService.IndustrialOre:
                    return DataStore.industry_ore[level + 1];

                case ItemClass.SubService.IndustrialForestry:
                    return DataStore.industry_forest[level + 1];

                case ItemClass.SubService.IndustrialFarming:
                    return DataStore.industry_farm[level + 1];

                case ItemClass.SubService.IndustrialOil:
                    return DataStore.industry_oil[level + 1];

                case ItemClass.SubService.IndustrialGeneric:  // Deliberate fall through
                default:
                    return DataStore.industry[level];
            }
        }
    }
}