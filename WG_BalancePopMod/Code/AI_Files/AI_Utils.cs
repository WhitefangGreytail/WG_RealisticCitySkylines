using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WG_BalancedPopMod
{
    public class AI_Utils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="item"></param>
        /// <param name="minWorkers"></param>
        /// <param name="array"></param>
        /// <param name="output"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void calculateprefabWorkerVisit(int width, int length, ref BuildingInfo item, int minWorkers, ref int[] array, out prefabEmployStruct output)
        {
            int num = array[DataStore.PEOPLE];
            int level0 = array[DataStore.WORK_LVL0];
            int level1 = array[DataStore.WORK_LVL1];
            int level2 = array[DataStore.WORK_LVL2];
            int level3 = array[DataStore.WORK_LVL3];
            int num2 = level0 + level1 + level2 + level3;

            if (num > 0 && num2 > 0)
            {
                Vector3 v = item.m_size;
                int floorSpace = calcBase(width, length, ref array, v);
                int floorCount = Mathf.Max(1, Mathf.FloorToInt(v.y / array[DataStore.LEVEL_HEIGHT])) + array[DataStore.DENSIFICATION];
                int value = (floorSpace * floorCount) / array[DataStore.PEOPLE];
                num = Mathf.Max(minWorkers, value);

                output.level3 = (num * level3) / num2;
                output.level2 = (num * level2) / num2;
                output.level1 = (num * level1) / num2;
                output.level0 = Mathf.Max(0, num - output.level3 - output.level2 - output.level1);  // Whatever is left
            }
            else
            {
                output.level0 = output.level1 = output.level2 = output.level3 = 1;  // Allocate 1 for every level, to stop div by 0
            }

            // Set the visitors here since we're calculating
            if (DataStore.enableVisitMultiplier)
            {
                if (array[DataStore.VISIT_MULT] > 1)
                {
                    int visit = (int) ((output.level0 + output.level1 + output.level2 + output.level3) * (array[DataStore.VISIT_MULT] / 10.0));
                    output.visitors = (int) (Math.Ceiling(visit / 5.0) * 5.0);  // Round to nearest 5 to use up the space
                }
                else
                {
                    output.visitors = 0;
                }
            }
            else
            {
	            int value = 0;
	            ItemClass.SubService subService = item.m_class.m_subService;
	            if (subService != ItemClass.SubService.CommercialLow)
	            {
		            if (subService != ItemClass.SubService.CommercialHigh)
		            {
			            if (subService != ItemClass.SubService.CommercialLeisure)
			            {
				            if (subService == ItemClass.SubService.CommercialTourist)
				            {
                                value = 250;
				            }
			            }
			            else
			            {
                            value = 250;
			            }
		            }
                    else if (item.m_class.m_level == ItemClass.Level.Level1)
		            {
                        value = 200;
		            }
                    else if (item.m_class.m_level == ItemClass.Level.Level2)
		            {
                        value = 300;
		            }
		            else
		            {
                        value = 400;
		            }
	            }
                else if (item.m_class.m_level == ItemClass.Level.Level1)
	            {
                    value = 90;
	            }
                else if (item.m_class.m_level == ItemClass.Level.Level2)
	            {
                    value = 100;
	            }
	            else
	            {
                    value = 110;
	            }

	            if (num != 0)
	            {
                    value = Mathf.Max(200, width * length * value) / 100;
	            }
                output.visitors = value;
            } // end visitMult
        } // end calculateprefabWorkerVisit


        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="item"></param>
        /// <param name="returnValue"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int calculatePrefabHousehold(int width, int length, ref BuildingInfo item)
        {
            int level = (int)(item.m_class.m_level >= 0 ? item.m_class.m_level : 0); // Force it to 0 if the level was set to None

            int[] array = DataStore.residentialLow[level];
            if (item.m_class.m_subService == ItemClass.SubService.ResidentialHigh)
            {
                array = DataStore.residentialHigh[level];
            }

            Vector3 v = item.m_size;
            int floorCount = Mathf.Max(1, Mathf.FloorToInt(v.y / array[DataStore.LEVEL_HEIGHT]));
            int returnValue = (calcBase(width, length, ref array, v) * floorCount) / array[DataStore.PEOPLE];

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

            return returnValue;
        }  // end calculatePrefabHousehold


        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="array"></param>
        /// <param name="v"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int calcBase(int width, int length, ref int[] array, Vector3 v)
        {
            if (array[DataStore.CALC_METHOD] == 0)
            {
                // Check x and z just incase they are 0. A few user created assets are.
                // If they are, then base the calculation of 3/4 of the width and length given
                if (v.x <= 1)
                {
                    width *= 6;
                }
                else
                {
                    width = (int)v.x;
                }

                if (v.z <= 1)
                {
                    length *= 6;
                }
                else
                {
                    length = (int)v.z;
                }
            }
            else
            {
                width *= 64; // Combine the eights
            }

            return width * length;
        }
    }
}
