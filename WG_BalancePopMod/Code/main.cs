using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using UnityEngine;
using ColossalFramework.Plugins;
using System.Diagnostics;


namespace WG_BalancedPopMod
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public const String XML_FILE = "WG_RealisticCity.xml";

        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = ""; 

        public override void OnLevelUnloading()
        {
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.writeXML(currentFileLocation);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                Stopwatch sw = Stopwatch.StartNew();
                readFromXML();
                swapAI();
                if (DataStore.enableExperimental)
                {
                    doubleCheckHousing();
                }
                sw.Stop();

                Debugging.panelMessage("Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
            }
        }


        /// <summary>
        ///
        /// </summary>
        private void readFromXML()
        {
            // Check the exe directory first
            currentFileLocation = ColossalFramework.IO.DataLocation.executableDirectory + Path.DirectorySeparatorChar + XML_FILE;
            bool fileAvailable = File.Exists(currentFileLocation);

            if (!fileAvailable)
            {
                // Switch to default which is the cities skylines in the application data area.
                currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;
                fileAvailable = File.Exists(currentFileLocation);
            }

            if (fileAvailable)
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = null;
                XmlDocument doc = new XmlDocument();
                try
                {
                    // TODO: Determine version before obtaining the reader
                    doc.Load(currentFileLocation);

                    reader = new XML_VersionTwo();
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.panelMessage(e.Message);
                }
            }
            else
            {
                Debugging.panelMessage("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }


        /// <summary>
        /// Swap the AI for buildings
        /// </summary>
        private void swapAI()
        {
            Dictionary<Type, Type> componentRemap = new Dictionary<Type, Type>
            {
                {
                    typeof(IndustrialExtractorAI),
                    typeof(IndustrialExtractorAIMod)
                },
                {
                    typeof(IndustrialBuildingAI),
                    typeof(IndustrialBuildingAIMod)
                },
                {
                    typeof(ResidentialBuildingAI),
                    typeof(ResidentialBuildingAIMod)
                },
                {
                    typeof(OfficeBuildingAI),
                    typeof(OfficeBuildingAIMod)
                },
                {
                    typeof(CommercialBuildingAI),
                    typeof(CommercialBuildingAIMod)
                }
            };

            uint buildingcount = (uint)PrefabCollection<BuildingInfo>.PrefabCount();
            for (uint count = 0u; count < buildingcount; count++)
            {
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab(count);
                AdjustBuidingAI(prefab, componentRemap);
            }
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="buildinginfo"></param>
        /// <param name="componentRemap"></param>
        private void AdjustBuidingAI(BuildingInfo buildinginfo, Dictionary<Type, Type> componentRemap)
        {
            if (buildinginfo != null && buildinginfo.GetComponent<BuildingAI>() != null)
            {
                BuildingAI component = buildinginfo.GetComponent<BuildingAI>();
                Type originalAiType = component.GetType();
                Type newAIType;
                // TODO ? - Somewhere in here we could grab the asset number to be able to set duplex houses with +1 household modifier?
                if (componentRemap.TryGetValue(originalAiType, out newAIType))
                {
                    BuildingAI buildingAI = buildinginfo.gameObject.AddComponent(newAIType) as BuildingAI;
                    buildingAI.m_info = buildinginfo;
                    buildinginfo.m_buildingAI = buildingAI;
                    buildingAI.InitializePrefab();
                }
            }
        }


        /// <summary>
        /// This is here because the mods are only loaded in and activated once the city is loaded in. The loading sequence is too late to prevent citizenIndex units being allocated to the building
        /// </summary>
        public void doubleCheckHousing()
        {
            // Grab the list of residential buildings that are in the city
            BuildingManager buildings = (BuildingManager)((object)UnityEngine.Object.FindObjectOfType(typeof(BuildingManager)));
            // CitizenUnit is the data store for the family, citizen0-4 in a struct which can't be changed. No way to change family size beyond 5 :(
            CitizenManager citizens = (CitizenManager)((object)UnityEngine.Object.FindObjectOfType(typeof(CitizenManager)));
            Building[] buffer = buildings.m_buildings.m_buffer;
            int failedCount = 0;

            for (int i = 1; i < buffer.Length; i++)
            {
                try
                {
                    checkResidentialHouseholds(buildings, citizens, i);
                }
#pragma warning disable  // Stop complaining compiler!
                catch (Exception e)
#pragma warning enable
                {
                    failedCount++;
                }
            } // end for

            if (failedCount > 0)
            {
                Debugging.panelWarning("Number of failed residential changes : " + failedCount);
            }
        }


        /// <summary>
        /// Check the household numbers
        /// </summary>
        /// <param name="buildings"></param>
        /// <param name="citizens"></param>
        /// <param name="i"></param>
        private void checkResidentialHouseholds(BuildingManager buildings, CitizenManager citizens, int i)
        {
            Building building = buildings.m_buildings.m_buffer[i];
            BuildingInfo info = building.Info;
            int width = building.Width;
            int length = building.Length;

            if ((info != null) && (info.m_buildingAI is ResidentialBuildingAI))
            {
                int modHomeCount = ((ResidentialBuildingAI)info.m_buildingAI).CalculateHomeCount(new ColossalFramework.Math.Randomizer(i), width, length);

                // If the modded home count is meant to be less than the original
                if (modHomeCount < Game_ResidentialAI.CalculateHomeCount(new ColossalFramework.Math.Randomizer(i), width, length, info.m_class.m_subService, info.m_class.m_level))
                {
                    int houseHoldCount = 0;
                    uint citizenIndex = building.m_citizenUnits;
                    while (houseHoldCount < modHomeCount)  // Fast forward
                    {
                        citizenIndex = citizens.m_units.m_buffer[(int)((UIntPtr)citizenIndex)].m_nextUnit;
                        houseHoldCount++;
                    }

                    // Disconnect the rest
                    while (citizenIndex != 0u)
                    {
                        CitizenUnit c = citizens.m_units.m_buffer[(int)((UIntPtr)citizenIndex)];

                        citizens.ReleaseUnits(citizenIndex);
                        citizenIndex = c.m_nextUnit;

                        // Reset the flags which could make the game think this group has connections to a home
                        c.m_building = 0;
                        c.m_nextUnit = 0u;
                        c.m_flags = CitizenUnit.Flags.None;
                    }
                }
            }
        }
    }
}
