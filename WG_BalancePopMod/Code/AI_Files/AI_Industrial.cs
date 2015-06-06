using System;
using System.Collections.Generic;
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
            int num = 0;
            level0 = 0;
            level1 = 0;
            level2 = 0;
            level3 = 0;
            if (@class.m_subService == ItemClass.SubService.IndustrialGeneric)
            {
                if (@class.m_level == ItemClass.Level.Level1)
                {
                    num = DataStore.industry[0][DataStore.PEOPLE];
                    level0 = 100;
                    level1 = 0;
                    level2 = 0;
                    level3 = 0;
                }
                else if (@class.m_level == ItemClass.Level.Level2)
                {
                    num = DataStore.industry[1][DataStore.PEOPLE];
                    level0 = 20;
                    level1 = 60;
                    level2 = 20;
                    level3 = 0;
                }
                else
                {
                    num = DataStore.industry[2][DataStore.PEOPLE];
                    level0 = 5;
                    level1 = 15;
                    level2 = 30;
                    level3 = 50;
                }
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialFarming)
            {
                num = DataStore.industry_farm[0][DataStore.PEOPLE];
                level0 = 100;
                level1 = 0;
                level2 = 0;
                level3 = 0;
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialForestry)
            {
                num = DataStore.industry_forest[0][DataStore.PEOPLE];
                level0 = 100;
                level1 = 0;
                level2 = 0;
                level3 = 0;
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialOre)
            {
                num = DataStore.industry_ore[0][DataStore.PEOPLE];
                level0 = 20;
                level1 = 60;
                level2 = 20;
                level3 = 0;
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialOil)
            {
                num = DataStore.industry_oil[0][DataStore.PEOPLE];
                level0 = 20;
                level1 = 60;
                level2 = 20;
                level3 = 0;
            }
            if (num != 0)
            {
                num = Mathf.Max(200, width * length * num + r.Int32(100u)) / 100;
                int num2 = level0 + level1 + level2 + level3;
                if (num2 != 0)
                {
                    level0 = (num * level0 + r.Int32((uint)num2)) / num2;
                    num -= level0;
                }
                num2 = level1 + level2 + level3;
                if (num2 != 0)
                {
                    level1 = (num * level1 + r.Int32((uint)num2)) / num2;
                    num -= level1;
                }
                num2 = level2 + level3;
                if (num2 != 0)
                {
                    level2 = (num * level2 + r.Int32((uint)num2)) / num2;
                    num -= level2;
                }
                level3 = num;
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
            electricityConsumption = 0;
            waterConsumption = 0;
            sewageAccumulation = 0;
            garbageAccumulation = 0;
            incomeAccumulation = 0;

            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None

            if (@class.m_subService == ItemClass.SubService.IndustrialGeneric)
            {
                electricityConsumption = DataStore.industry[level][DataStore.POWER];
                waterConsumption = DataStore.industry[level][DataStore.WATER];
                sewageAccumulation = DataStore.industry[level][DataStore.SEWAGE];
                garbageAccumulation = DataStore.industry[level][DataStore.GARBAGE];
                incomeAccumulation = DataStore.industry[level][DataStore.INCOME];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialOre)
            {
                electricityConsumption = DataStore.industry_ore[level][DataStore.POWER];
                waterConsumption = DataStore.industry_ore[level][DataStore.WATER];
                sewageAccumulation = DataStore.industry_ore[level][DataStore.SEWAGE];
                garbageAccumulation = DataStore.industry_ore[level][DataStore.GARBAGE];
                incomeAccumulation = DataStore.industry_ore[level][DataStore.INCOME];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialOil)
            {
                electricityConsumption = DataStore.industry_oil[level][DataStore.POWER];
                waterConsumption = DataStore.industry_oil[level][DataStore.WATER];
                sewageAccumulation = DataStore.industry_oil[level][DataStore.SEWAGE];
                garbageAccumulation = DataStore.industry_oil[level][DataStore.GARBAGE];
                incomeAccumulation = DataStore.industry_oil[level][DataStore.INCOME];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialForestry)
            {
                electricityConsumption = DataStore.industry_forest[level][DataStore.POWER];
                waterConsumption = DataStore.industry_forest[level][DataStore.WATER];
                sewageAccumulation = DataStore.industry_forest[level][DataStore.SEWAGE];
                garbageAccumulation = DataStore.industry_forest[level][DataStore.GARBAGE];
                incomeAccumulation = DataStore.industry_forest[level][DataStore.INCOME];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialFarming)
            {
                electricityConsumption = DataStore.industry_farm[level][DataStore.POWER];
                waterConsumption = DataStore.industry_farm[level][DataStore.WATER];
                sewageAccumulation = DataStore.industry_farm[level][DataStore.SEWAGE];
                garbageAccumulation = DataStore.industry_farm[level][DataStore.GARBAGE];
                incomeAccumulation = DataStore.industry_farm[level][DataStore.INCOME];
            }
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

            if (@class.m_subService == ItemClass.SubService.IndustrialGeneric)
            {
                groundPollution = DataStore.industry[level][DataStore.GROUND_POLLUTION];
                noisePollution = DataStore.industry[level][DataStore.NOISE_POLLUTION];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialOre)
            {
                groundPollution = DataStore.industry_ore[level][DataStore.GROUND_POLLUTION];
                noisePollution = DataStore.industry_ore[level][DataStore.NOISE_POLLUTION];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialOil)
            {
                groundPollution = DataStore.industry_oil[level][DataStore.GROUND_POLLUTION];
                noisePollution = DataStore.industry_oil[level][DataStore.NOISE_POLLUTION];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialForestry)
            {
                groundPollution = DataStore.industry_forest[level][DataStore.GROUND_POLLUTION];
                noisePollution = DataStore.industry_forest[level][DataStore.NOISE_POLLUTION];
            }
            else if (@class.m_subService == ItemClass.SubService.IndustrialFarming)
            {
                groundPollution = DataStore.industry_farm[level][DataStore.GROUND_POLLUTION];
                noisePollution = DataStore.industry_farm[level][DataStore.NOISE_POLLUTION];
            }

            groundPollution = (productionRate * groundPollution) / 100;
            noisePollution = (productionRate * noisePollution) / 100;
        }
    }
}