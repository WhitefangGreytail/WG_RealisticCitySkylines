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
            try
            {
                WG_XMLBaseVersion xml = new XML_VersionFive();
                xml.writeXML(currentFileLocation);
            }
            catch (Exception e)
            {
                Debugging.panelMessage(e.Message);
            }

            // Clear all the caches
            OfficeBuildingAIMod.clearCache();
            CommercialBuildingAIMod.clearCache();
            IndustrialBuildingAIMod.clearCache();
            IndustrialExtractorAIMod.clearCache();
            ResidentialBuildingAIMod.clearCache();

            DataStore.bonusHousehold.Clear();
            DataStore.allowRemovalOfCitizens = false;
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                Stopwatch sw = Stopwatch.StartNew();
                readFromXML();

                // Add mesh names to dictionary, ensuring uniqueness
                foreach (string data in DataStore.xmlMeshNames)
                {
                    if (!DataStore.bonusHousehold.ContainsKey(data))
                    {
                        DataStore.bonusHousehold.Add(data, data);
                    }
                }

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
                WG_XMLBaseVersion reader = new XML_VersionFive();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(currentFileLocation);
                    try
                    {
                        int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                        if (version == 4)
                        {
                            reader = new XML_VersionFour();

                            // Make a back up copy of the old system to be safe
                            File.Copy(currentFileLocation, currentFileLocation + ".ver4", true);
                        }
                        else if (version <= 3) // Uh oh... version 3 was a while back..
                        {
                            Debugging.panelWarning("Detected an unsupported version of the XML (v3 or less). Backing up for a new configuration as :" + currentFileLocation + ".ver3");
                            File.Copy(currentFileLocation, currentFileLocation + ".ver3", true);
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
    }
}
