using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using UnityEngine;
using ColossalFramework.Math;
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
        private static byte[][] segments = new byte[2][];
        private static bool isModEnabled = false;

        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                // Replace the one method call which is called when the city is loaded and EnsureCitizenUnits is used
                // ResidentialAI -> Game_ResidentialAI. This stops the buildings from going to game defaults on load.
                // This has no further effects on buildings as the templates are replaced by ResidentialAIMod
                var oldMethod = typeof(ResidentialBuildingAI).GetMethod("CalculateHomeCount");
                var newMethod = typeof(TrickResidentialAI).GetMethod("CalculateHomeCount");

                // This is to disable all household checks. No need to load a thing
                segments[0] = RedirectionHelper.RedirectCalls(oldMethod, newMethod);

                oldMethod = typeof(BuildingAI).GetMethod("EnsureCitizenUnits", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                newMethod = typeof(AI_Building).GetMethod("EnsureCitizenUnits", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                segments[1] = RedirectionHelper.RedirectCalls(oldMethod, newMethod);
/*
                Dictionary<ulong, uint> seedToId = new Dictionary<ulong, uint>(131071);
                for (uint i = 0; i <= (64*1024); i++)  // Up to 256k buildings apparently is ok
                {
                    // This creates a unique number
                    Randomizer number = new Randomizer((int)i);
                    try
                    {
                        seedToId.Add(number.seed, i);
                    }
                    catch (System.ArgumentException)
                    {
                        Debugging.writeDebugToFile("Seed collision at number: "+ i);
                    }
                }
*/
                isModEnabled = true;
            }
        }

        public override void OnReleased()
        {
            if (isModEnabled)
            {
                var oldMethod = typeof(ResidentialBuildingAI).GetMethod("CalculateHomeCount");
                RedirectionHelper.RestoreCalls(oldMethod, segments[0]);

                oldMethod = typeof(BuildingAI).GetMethod("EnsureCitizenUnits", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                RedirectionHelper.RestoreCalls(oldMethod, segments[1]);

                isModEnabled = false;
            }
        }


        public override void OnLevelUnloading()
        {
            DataStore.allowRemovalOfCitizens = false;
            // Clear all the caches
            OfficeBuildingAIMod.clearCache();
            CommercialBuildingAIMod.clearCache();
            IndustrialBuildingAIMod.clearCache();
            IndustrialExtractorAIMod.clearCache();
            ResidentialBuildingAIMod.clearCache();

            try
            {
                WG_XMLBaseVersion xml = new XML_VersionFour();
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
                    // Nothing here for now
                }
                DataStore.allowRemovalOfCitizens = true;

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
                WG_XMLBaseVersion reader = new XML_VersionFour();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    try
                    {
                        if (Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText) < 4)
                        {
                            reader = new XML_VersionThreeToFour();

                            // Make a back up copy of the old system
                            File.Copy(currentFileLocation, currentFileLocation + ".ver2", true);
                        }
                    }
                    catch
                    {
                        // Default to new XML structure
                    }

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

                if (componentRemap.TryGetValue(originalAiType, out newAIType))
                {
                    BuildingAI buildingAI = buildinginfo.gameObject.AddComponent(newAIType) as BuildingAI;
                    buildingAI.m_info = buildinginfo;
                    buildinginfo.m_buildingAI = buildingAI;
                    buildingAI.InitializePrefab();
                }
            }
        }

//---------------------------------------------------------
//        Spare code
//---------------------------------------------------------

        /// <summary>
        /// Check the household numbers
        /// </summary>
        /// <param name="buildings"></param>
        /// <param name="citizens"></param>
        /// <param name="i"></param>
        private void checkResidentialHouseholds(BuildingManager buildings, int i)
        {
            CitizenManager citizens = ColossalFramework.Singleton<CitizenManager>.instance;

            Building building = buildings.m_buildings.m_buffer[i];
            BuildingInfo info = building.Info;
            int width = building.Width;
            int length = building.Length;

            if ((info != null) && (info.m_buildingAI is ResidentialBuildingAI) && (info.m_class.m_subService == ItemClass.SubService.ResidentialLow))  // Only do something for low density
            {
                int modHomeCount = ((ResidentialBuildingAI)info.m_buildingAI).CalculateHomeCount(new ColossalFramework.Math.Randomizer(i), width, length);

                // If the modded home count is meant to be less than the original
                if (modHomeCount < TrickResidentialAI.CalculateHomeCount(new ColossalFramework.Math.Randomizer(i), width, length, info.m_class.m_subService, info.m_class.m_level))
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
                        // TODO - Need to get the building to find the citizen AI
                        // This is essentially the release building code after bulldoze. I don't see why it causes an issue.
                        // Is this thread related? I hope not as it would be unsolvable :(

                        CitizenUnit c = citizens.m_units.m_buffer[(int)((UIntPtr)citizenIndex)];
                        citizens.ReleaseUnits(citizenIndex);
                        citizenIndex = c.m_nextUnit;

                        // Reset the flags which could make the game think this group has connections to a home
                        c.m_citizen0 = 0u;
                        c.m_citizen1 = 0u;
                        c.m_citizen2 = 0u;
                        c.m_citizen3 = 0u;
                        c.m_citizen4 = 0u;
                        c.m_nextUnit = 0u;
                        c.m_vehicle = (ushort)0;
                        c.m_building = 0;
                        c.m_flags = CitizenUnit.Flags.None;
                    }
                    
                }
            }
        } // end checkResidentialHouseholds
    }
}
