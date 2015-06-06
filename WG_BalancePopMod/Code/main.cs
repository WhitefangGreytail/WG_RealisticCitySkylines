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
        public const String OLD_XML_FILE = "WG_BalancePopMod.xml";


        public override void OnLevelUnloading()
        {
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionTwo();
                xml.writeXML(System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + XML_FILE);

                string oldXmlFileName = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + OLD_XML_FILE;
                if (File.Exists(oldXmlFileName))
                {
                    System.IO.File.Move(oldXmlFileName, oldXmlFileName + ".old");
                }
            }
            catch (NullReferenceException e)
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
                doubleCheckHousing();
                
                sw.Stop();
                Debugging.panelMessage("Successfully Loaded WG_RealisticCity in " + sw.ElapsedMilliseconds + " ms.");
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

            for (int i = 1; i < buffer.Length; i++)
            {
                checkResidentialHouseholds(buildings, citizens, i);
            } // end for
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
                try
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
                            c.m_nextUnit = 0u;
                            c.m_building = 0;
                            c.m_flags = CitizenUnit.Flags.None;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugging.panelWarning("Residential building at index " + i + " could not be successfully changed.");
                }
            }
        }


        private void swapAI()
        {
            Dictionary<Type, Type> componentRemap = new Dictionary<Type, Type>
			{
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
				},
            };

            uint buildingcount = (uint) PrefabCollection<BuildingInfo>.PrefabCount();
            for( uint count = 0u; count < buildingcount; count++)
            {
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab(count);
                AdjustBuidingAI(prefab, componentRemap);
            }
        }


        private static void readFromXML()
        {
            string directory = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar;
            string xmlFileName = directory + XML_FILE;
            WG_XMLBaseVersion reader = null;

            // Load in from XML - Designed to be flat file for ease
            XmlDocument doc = new XmlDocument();
            try
            {
                if (File.Exists(xmlFileName))
                {
                    reader = new XML_VersionTwo();
                }
                else
                {
                    xmlFileName = directory + OLD_XML_FILE;
                    reader = new XML_VersionOne();
                }

                if (reader != null)
                {
                    doc.Load(xmlFileName);
                    // TODO: Determine version before obtaining the reader
                    reader.readXML(doc);
                }
            }
            catch (Exception e)
            {
                // Game will now use defaults
                Debugging.panelMessage(e.Message);
            }
        }


        private void AdjustBuidingAI(BuildingInfo buildinginfo, Dictionary<Type, Type> componentRemap)
        {
            if (buildinginfo != null && buildinginfo.GetComponent<BuildingAI>() != null)
            {
                BuildingAI component = buildinginfo.GetComponent<BuildingAI>();
                Type originalAiType = component.GetType();
                Type newAIType;
                if (componentRemap.TryGetValue(originalAiType, out newAIType))
                {
                    BuildingAI buildingAI = buildinginfo.gameObject.AddComponent(newAIType) as BuildingAI;
                    buildingAI.m_info = buildinginfo;
                    buildinginfo.m_buildingAI = buildingAI;
                    buildingAI.InitializePrefab();
                }
                else
                {
                    Debugging.writeDebugToFile("Could not getvalue for " + buildinginfo.name);
                }
            }
        }
    }
}
