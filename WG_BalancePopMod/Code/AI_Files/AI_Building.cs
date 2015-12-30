using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
using ColossalFramework;
using UnityEngine;


namespace WG_BalancedPopMod
{
    public class AI_Building : BuildingAI
    {
        /// <summary>
        /// BuildingAI replacement
        /// </summary>
        /// <param name="buildingID"></param>
        /// <param name="data"></param>
        /// <param name="homeCount"></param>
        /// <param name="workCount"></param>
        /// <param name="visitCount"></param>
        /// <param name="studentCount"></param>
        protected void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount, int workCount, int visitCount, int studentCount)
        {
            int totalWorkCount = (workCount + 4) / 5;
            int totalHomeCount = homeCount;
            int unitWorkCount = 0;
            int unitHomeCount = 0;
            int unitVisitCount = 0;
            int unitCount = 0;
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenUnit[] citizenUnitArray = instance.m_units.m_buffer;

            if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.BurnedDown)) == Building.Flags.None)
            {
                Citizen.Wealth wealthLevel = Citizen.GetWealthLevel(this.m_info.m_class.m_level);
                uint num = 0u;
                uint num2 = data.m_citizenUnits;
                int num3 = 0;
                while (num2 != 0u)
                {
                    CitizenUnit.Flags flags = citizenUnitArray[(int)((UIntPtr)num2)].m_flags;
                    if ((ushort)(flags & CitizenUnit.Flags.Home) != 0)
                    {
                        citizenUnitArray[(int)((UIntPtr)num2)].SetWealthLevel(wealthLevel);
                        homeCount--;
                        unitHomeCount++;
                    }
                    if ((ushort)(flags & CitizenUnit.Flags.Work) != 0)
                    {
                        workCount -= 5;
                        unitWorkCount++;
                    }
                    if ((ushort)(flags & CitizenUnit.Flags.Visit) != 0)
                    {
                        visitCount -= 5;
                        unitVisitCount++;
                    }
                    if ((ushort)(flags & CitizenUnit.Flags.Student) != 0)
                    {
                        studentCount -= 5;
                    }
                    num = num2;
                    num2 = citizenUnitArray[(int)((UIntPtr)num2)].m_nextUnit;
                    if (++num3 > 524288)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        break;
                    }
                    unitCount++;
                } // end while
/*
                homeCount = Mathf.Max(0, homeCount);
                workCount = Mathf.Max(0, workCount);
 */
                visitCount = Mathf.Max(0, visitCount);
                studentCount = Mathf.Max(0, studentCount);

                if (homeCount > 0 || workCount > 0 || visitCount > 0 || studentCount > 0)
                {
                    uint num4 = 0u;
                    if (instance.CreateUnits(out num4, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, studentCount))
                    {
                        if (num != 0u)
                        {
                            citizenUnitArray[(int)((UIntPtr)num)].m_nextUnit = num4;
                        }
                        else
                        {
                            data.m_citizenUnits = num4;
                        }
                    }
                }

                // This is done to have the count in numbers of citizen units
// TODO - Remove experimental flag later
                if (DataStore.enableExperimental && DataStore.allowRemovalOfCitizens)
                {
                    if (workCount < 0)
                    {
                        RemoveWorkerBuilding(buildingID, ref data, totalWorkCount);
                    }
                    else if (homeCount < 0)
                    {
                        RemoveHouseHold(buildingID, ref data, totalHomeCount);
                    }
                    // Do nothing for visit only or students
                } // end if
            } // end if good building
        } // end EnsureCitizenUnits

        /// <summary>
        /// Send this unit away to empty to requirements
        /// EmptyBuilding
        /// </summary>
        /// <param name="buildingID"></param>
        /// <param name="data"></param>
        /// <param name="citizenNumber"></param>
        private void RemoveWorkerBuilding(ushort buildingID, ref Building data, int workerUnits)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenUnit[] citizenUnitArray = instance.m_units.m_buffer;
            Citizen[] citizenArray = instance.m_citizens.m_buffer;

            int loopCounter = 0;
            uint previousUnit = data.m_citizenUnits;
            uint currentUnit = data.m_citizenUnits;
            int[] workersRequired = new int[] { 0, 0, 0, 0 };
            ((PrivateBuildingAI)data.Info.GetAI()).CalculateWorkplaceCount(new Randomizer((int)buildingID), data.Width, data.Length,
                out workersRequired[0], out workersRequired[1], out workersRequired[2], out workersRequired[3]);

            while (currentUnit != 0u)
            {
                // If this unit matches what we one, send the citizens away or remove citzens
                uint nextUnit = citizenUnitArray[currentUnit].m_nextUnit;
                bool removeCurrentUnit = false;

                // Only think about removing if it matches the flag
                if ((ushort)(CitizenUnit.Flags.Work & citizenUnitArray[currentUnit].m_flags) != 0)
                {
                    if (workerUnits > 0)
                    {
                        // Don't remove the unit, we'll remove excess afterwards
                        workerUnits--;

                        for (int i = 0; i < 5; i++)
                        {
                            uint citizen = citizenUnitArray[(int)((UIntPtr)currentUnit)].GetCitizen(i);
                            if (citizen != 0u)
                            {
                                // Tick off education
                                workersRequired[(int)citizenArray[(int)((UIntPtr)citizen)].EducationLevel]--;
                            }
                        }
                    }
                    else
                    {
                        // Send unit away like empty building
                        for (int i = 0; i < 5; i++)
                        {
                            uint citizen = citizenUnitArray[(int)((UIntPtr)currentUnit)].GetCitizen(i);
                            if (citizen != 0u)
                            {
                                // Do not shift back where possible. There's enough staff turnover that the spaces aren't worth the intensive checking
                                int citizenIndex = (int)((UIntPtr)citizen);
                                ushort citizenInstanceIndex = citizenArray[citizenIndex].m_instance;
                                CitizenInstance citData = instance.m_instances.m_buffer[(int)citizenInstanceIndex];
                                FireAndTeleportHome(buildingID, citizenArray, citizen, citizenIndex, citizenInstanceIndex, ref citData);
                            }  // end citizen
                        } // end for
                        removeCurrentUnit = true;
                    } // end if
                } // Flag match

                // Don't need to worry about trying to remove the initial citizen unit. 
                // This should always exist and other code will always force at least one.
                if (removeCurrentUnit)
                {
                    // Link previous unit to next unit and release the item
                    citizenUnitArray[previousUnit].m_nextUnit = nextUnit;
                    instance.m_units.ReleaseItem(currentUnit);
                    // Previous unit number has not changed
                }
                else
                {
                    // Current unit is not to be removed, proceed to next
                    previousUnit = currentUnit;
                }
                currentUnit = nextUnit;

                // If list is too long, abort (102400 people is a bit too much)
                if (++loopCounter > 20480)
                {
                    currentUnit = 0u; // Bail out loop
                }
            } // end while
            
//Debugging.writeDebugToFile(buildingID + ". Workers needed: " + workersRequired[0] + ", " + workersRequired[1] + ", " + workersRequired[2] + ", " + workersRequired[3]);
            // Now, see if we can update the workers to fit the education bill better.
            // Remove excess 0, 1, 2. However, give 20 - 50 % change to go up an education level. Don't touch lvl 3 educated (they'll disappear fast if possible)
            // Turn off transfer offer
            loopCounter = 0;
            previousUnit = data.m_citizenUnits;
            currentUnit = data.m_citizenUnits;
            while (currentUnit != 0u)
            {
                // If this unit matches what we one, send the citizens away or remove citzens
                uint nextUnit = citizenUnitArray[currentUnit].m_nextUnit;

                // Only think about removing if it matches the flag
                if ((ushort)(CitizenUnit.Flags.Work & citizenUnitArray[currentUnit].m_flags) != 0)
                {
                    // Send unit away like empty building
                    for (int i = 0; i < 5; i++)
                    {
                        uint citizen = citizenUnitArray[(int)((UIntPtr)currentUnit)].GetCitizen(i);
                        if (citizen != 0u)
                        {
                            // Do not shift back where possible. There's enough staff turnover that the spaces aren't worth the intensive checking
                            int citizenIndex = (int)((UIntPtr)citizen);
                            ushort citizenInstanceIndex = citizenArray[citizenIndex].m_instance;
                            CitizenInstance citData = instance.m_instances.m_buffer[(int)citizenInstanceIndex];

                            // Get education level. Perform checks
                            Citizen  cit = citizenArray[(int)((UIntPtr)citizen)];
                            int education = (int)cit.EducationLevel;
                            // -ve workersRequired means excess workers. Ignoring three schools
                            // Checks if the citizen should be promoted or fire
                            if ((cit.EducationLevel != Citizen.Education.ThreeSchools) && workersRequired[education] < 0 && workersRequired[education + 1] > 0)
                            {
                                // Need to be above 50 to be promoted. However, each level is harder to get to, effectively (50, 65, 80)
                                int number = Singleton<SimulationManager>.instance.m_randomizer.Int32(0, 100) - (education * 15);
                                if (number > 50)
                                {
                                    if (cit.EducationLevel == Citizen.Education.Uneducated)
                                    {
                                        cit.Education1 = true;
                                        workersRequired[0]++;
                                        workersRequired[1]--;
                                    }
                                    else if (cit.EducationLevel == Citizen.Education.OneSchool)
                                    {
                                        cit.Education2 = true;
                                        workersRequired[1]++;
                                        workersRequired[2]--;
                                    }
                                    else if (cit.EducationLevel == Citizen.Education.TwoSchools)
                                    {
                                        cit.Education3 = true;
                                        workersRequired[2]++;
                                        workersRequired[3]--;
                                    }
                                }
                                else
                                {
                                    workersRequired[education]++;
                                    FireAndTeleportHome(buildingID, citizenArray, citizen, citizenIndex, citizenInstanceIndex, ref citData);
                                    RemoveFromCitizenUnit(citizenUnitArray, currentUnit, i);
                                }
                            }
                            else if (workersRequired[education] < 0)
                            {
                                workersRequired[education]++;
                                FireAndTeleportHome(buildingID, citizenArray, citizen, citizenIndex, citizenInstanceIndex, ref citData);
                                RemoveFromCitizenUnit(citizenUnitArray, currentUnit, i);
                            } // end if
                        }  // end citizen
                    } // end for
                } // Flag match

                previousUnit = currentUnit;
                currentUnit = nextUnit;

                // If list is too long, abort (102400 people is a bit too much)
                if (++loopCounter > 20480)
                {
                    currentUnit = 0u; // Bail out loop
                }
            } // end while

            // Force reset incoming offers to get HandleWorkers() to start fresh
            TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
            offer.Building = buildingID;
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker0, offer);
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker1, offer);
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker2, offer);
            Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Worker3, offer);
        } // end CheckWorkerBuilding


        /// <summary>
        /// 
        /// </summary>
        /// <param name="citizenUnitArray"></param>
        /// <param name="currentUnit"></param>
        /// <param name="i"></param>
        private static void RemoveFromCitizenUnit(CitizenUnit[] citizenUnitArray, uint currentUnit, int i)
        {
            switch (i)
            {
                case 0:
                    citizenUnitArray[(int)((UIntPtr)currentUnit)].m_citizen0 = 0u;
                    break;
                case 1:
                    citizenUnitArray[(int)((UIntPtr)currentUnit)].m_citizen1 = 0u;
                    break;
                case 2:
                    citizenUnitArray[(int)((UIntPtr)currentUnit)].m_citizen2 = 0u;
                    break;
                case 3:
                    citizenUnitArray[(int)((UIntPtr)currentUnit)].m_citizen3 = 0u;
                    break;
                case 4:
                    citizenUnitArray[(int)((UIntPtr)currentUnit)].m_citizen4 = 0u;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildingID"></param>
        /// <param name="citizenArray"></param>
        /// <param name="citizen"></param>
        /// <param name="citizenIndex"></param>
        /// <param name="citizenInstanceIndex"></param>
        /// <param name="citData"></param>
        private static void FireAndTeleportHome(ushort buildingID, Citizen[] citizenArray, uint citizen, int citizenIndex, ushort citizenInstanceIndex, ref CitizenInstance citData)
        {
            if (citizenArray[citizenIndex].GetBuildingByLocation() == buildingID ||
                (citizenInstanceIndex != 0 && citData.m_targetBuilding == buildingID))
            {
                // Stop citizen and teleport citizen home to be simple
                if (citData.m_path != 0u)
                {
                    Singleton<PathManager>.instance.ReleasePath(citData.m_path);
                    citData.m_path = 0u;
                }
                citizenArray[citizenIndex].m_workBuilding = 0;
                citizenArray[citizenIndex].m_flags = Citizen.Flags.Unemployed;
                citizenArray[citizenIndex].CurrentLocation = Citizen.Location.Home;
            }
        }


        /// <summary>
        /// Send this unit away to empty to requirements
        /// EmptyBuilding
        /// </summary>
        /// <param name="buildingID"></param>
        /// <param name="data"></param>
        /// <param name="citizenNumber"></param>
        private void RemoveHouseHold(ushort buildingID, ref Building data, int maxHomes)
        {
            CitizenManager instance = Singleton<CitizenManager>.instance;
            CitizenUnit[] citizenUnitArray = instance.m_units.m_buffer;
            Citizen[] citizenArray = instance.m_citizens.m_buffer;

            int loopCounter = 0;
            uint previousUnit = data.m_citizenUnits;
            uint currentUnit = data.m_citizenUnits;

            while (currentUnit != 0u)
            {
                // If this unit matches what we one, send the citizens away or remove citzens
                uint nextUnit = citizenUnitArray[currentUnit].m_nextUnit;
                bool removeCurrentUnit = false;

                // Only think about removing if it matches the flag
                if ((ushort)(CitizenUnit.Flags.Home & citizenUnitArray[currentUnit].m_flags) != 0)
                {
                    if (maxHomes > 0)
                    {
                        maxHomes--;
                    }
                    else
                    {
                        // Remove excess citizens
                        for (int i = 0; i < 5; i++)
                        {
                            uint citizen = citizenUnitArray[(int)((UIntPtr)currentUnit)].GetCitizen(i);

                            if (citizen != 0u)
                            {
                                instance.ReleaseCitizen(citizen);
                            }  // end citizen
                        } // end for
                        removeCurrentUnit = true;
                    } // end if - above count
                } // Flag match

                // Don't need to worry about trying to remove the initial citizen unit. 
                // This should always exist and other code will always force at least one.
                if (removeCurrentUnit)
                {
                    // Link previous unit to next unit and release the item
                    citizenUnitArray[previousUnit].m_nextUnit = nextUnit;
                    instance.m_units.ReleaseItem(currentUnit);
                    // Previous unit number has not changed
                }
                else
                {
                    // Current unit is not to be removed, proceed to next
                    previousUnit = currentUnit;
                }
                currentUnit = nextUnit;

                // If list is too long, abort (102400 people is a bit too much)
                if (++loopCounter > 20480)
                {
                    currentUnit = 0u; // Bail out loop
                }
            } // end while
        } // end SendHouseHoldAway


        /// <summary>
        /// This gets the next citizen unit which is not full. The result can be the input
        /// </summary>
        /// <param name="currentUnit"></param>
        /// <param name="array"></param>
        /// <param name="lastAllowedUnit"></param>
        /// <returns></returns>
        private uint GetNextCitizenUnitWithSpace(uint currentUnit, CitizenUnit[] array, uint lastAllowedUnit)
        {
            while (currentUnit != 0u)
            {
                CitizenUnit unit = array[currentUnit];
                if (!unit.Full())
                {
                    return currentUnit;
                }
                currentUnit = array[currentUnit].m_nextUnit;

                if (currentUnit == lastAllowedUnit)
                {
                    // Halt search and future searches
                    currentUnit = 0u;
                }
            }

            return 0u;
        } // end GetNextCitizenUnitWithSpace
    }
}
