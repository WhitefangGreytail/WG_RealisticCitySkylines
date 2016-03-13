using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using UnityEngine;
using Boformer.Redirection;
using ColossalFramework;

namespace WG_BalancedPopMod
{
    [TargetType(typeof(OfficeBuildingAI))]
    class OfficeBuildingAIMod : OfficeBuildingAI
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
        [RedirectMethod(true)]
        public override void CalculateWorkplaceCount(Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            ulong seed = r.seed;
            BuildingInfo item = this.m_info;
            int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

            prefabEmployStruct output;
            // If not seen prefab, calculate
            if (!DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out output))
            {
                AI_Utils.calculateprefabWorkerVisit(width, length, ref item, 10, ref DataStore.office[level], out output);
                DataStore.prefabWorkerVisit.Add(item.gameObject.GetHashCode(), output);
            }

            level0 = output.level0;
            level1 = output.level1;
            level2 = output.level2;
            level3 = output.level3;
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
        [RedirectMethod(true)]
        public override void GetConsumptionRates(Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation)
        {
            ItemClass item = this.m_info.m_class;
            int level = (int)(item.m_level >= 0 ? item.m_level : 0); // Force it to 0 if the level was set to None

            electricityConsumption = DataStore.office[level][DataStore.POWER];
            waterConsumption = DataStore.office[level][DataStore.WATER];
            sewageAccumulation = DataStore.office[level][DataStore.SEWAGE];
            garbageAccumulation = DataStore.office[level][DataStore.GARBAGE];

            int landVal = AI_Utils.getLandValueIncomeComponent(r.seed);
            incomeAccumulation = DataStore.office[level][DataStore.INCOME] + landVal;

            electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption) / 100;
            waterConsumption = Mathf.Max(100, productionRate * waterConsumption) / 100;
            sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation) / 100;
            garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation) / 100;
            incomeAccumulation = productionRate * incomeAccumulation;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="productionRate"></param>
        /// <param name="cityPlanningPolicies"></param>
        /// <param name="groundPollution"></param>;
        /// <param name="noisePollution"></param>
        [RedirectMethod(true)]
        public override void GetPollutionRates(int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass @class = this.m_info.m_class;
            int level = (int)(@class.m_level >= 0 ? @class.m_level : 0); // Force it to 0 if the level was set to None

            groundPollution = DataStore.office[level][DataStore.GROUND_POLLUTION];
            noisePollution = DataStore.office[level][DataStore.NOISE_POLLUTION];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="r"></param>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [RedirectMethod(true)]
        public override int CalculateProductionCapacity(Randomizer r, int width, int length)
        {
            ulong seed = r.seed;
            BuildingInfo item = this.m_info;
            prefabEmployStruct worker;

            if (DataStore.prefabWorkerVisit.TryGetValue(item.gameObject.GetHashCode(), out worker))
            {
                // Employment is available
                int workers = worker.level0 + worker.level1 + worker.level2 + worker.level3;
                int level = (int)(this.m_info.m_class.m_level >= 0 ? this.m_info.m_class.m_level : 0); // Force it to 0 if the level was set to None
                return Mathf.Max(1, workers / DataStore.office[level][DataStore.PRODUCTION]);
            }
            else
            {
                // Return minimum to be safe
                return 1;
            }
        }
    }
}