﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using System.Diagnostics;
using Boformer.Redirection;
using ColossalFramework.Math;
using ColossalFramework;

namespace WG_BalancedPopMod
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private const int RES = 0;
        private const int COM = 0;
        private const int IND = 0;
        private const int INDEX = 0;
        private const int OFFICE = 0;
        public const String XML_FILE = "WG_RealisticCity.xml";

        private readonly Dictionary<MethodInfo, Redirector> redirectsOnLoaded = new Dictionary<MethodInfo, Redirector>();
        private readonly Dictionary<MethodInfo, Redirector> redirectsOnCreated = new Dictionary<MethodInfo, Redirector>();


        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;
        private static Stopwatch sw;

        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                isModEnabled = true;
                sw = Stopwatch.StartNew();
                Redirect(true);
                DataStore.ClearCache();

                ReadFromXML();
                MergeDefaultBonus();

                // Remove bonus names from over rides
                foreach (string name in DataStore.bonusHouseholdCache.Keys)
                {
                    DataStore.householdCache.Remove(name);
                }

                foreach (string name in DataStore.bonusWorkerCache.Keys)
                {
                    DataStore.workerCache.Remove(name);
                }

                DataStore.seedToId.Clear();
                for (int i = 0; i <= ushort.MaxValue; ++i)  // Up to 1M buildings apparently is ok
                {
                    // This creates a unique number
                    try
                    {
                        Randomizer number = new Randomizer(i);
                        DataStore.seedToId.Add(number.seed, (ushort) i);
                    }
                    catch (Exception)
                    {
                        //Debugging.writeDebugToFile("Seed collision at number: " + i);
                    }
                }

                sw.Stop();
                UnityEngine.Debug.Log("WG_RealisticCity: Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
            }
        }

        private void MergeDefaultBonus()
        {
            if (DataStore.mergeResidentialNames)
            {
                foreach(KeyValuePair<string, int> entry in DataStore.defaultHousehold)
                {
                    try
                    {
                        DataStore.householdCache.Add(entry.Key, entry.Value);
                    }
                    catch (Exception)
                    {
                        // Don't care
                    }
                }
            }

            if (DataStore.mergeEmploymentNames)
            {
                foreach (KeyValuePair<string, int> entry in DataStore.defaultWorker)
                {
                    try
                    {
                        DataStore.workerCache.Add(entry.Key, entry.Value);
                    }
                    catch (Exception)
                    {
                        // Don't care
                    }
                }
            }
        }

        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionSix();
                    xml.writeXML(DataStore.currentFileLocation);
                }
                catch (Exception e)
                {
                    Debugging.panelMessage(e.Message);
                }

                RevertRedirect(true);
            }
        }


        public override void OnLevelUnloading()
        {
            if (isLevelLoaded)
            {
                isLevelLoaded = false;
                DataStore.allowRemovalOfCitizens = false;
            }
        }


        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
            {
                if (!isLevelLoaded)
                {
                    isLevelLoaded = true;
                    // Now we can remove people
                    DataStore.allowRemovalOfCitizens = true;
                    Debugging.releaseBuffer();
                    Debugging.panelMessage("Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
                }
            }
        }


        private void Redirect(bool onCreated)
        {
            var redirects = onCreated ? redirectsOnCreated : redirectsOnLoaded;
            redirects.Clear();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                try
                {
                    var r = RedirectionUtil.RedirectType(type, onCreated);
                    if (r != null)
                    {
                        foreach (var pair in r)
                        {
                            redirects.Add(pair.Key, pair.Value);
                        }
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"An error occured while applying {type.Name} redirects!");
                    UnityEngine.Debug.Log(e.StackTrace);
                }
            }
        }

        private void RevertRedirect(bool onCreated)
        {
            var redirects = onCreated ? redirectsOnCreated : redirectsOnLoaded;
            foreach (var kvp in redirects)
            {
                try
                {
                    kvp.Value.Revert();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.Log($"An error occured while reverting {kvp.Key.Name} redirect!");
                    UnityEngine.Debug.Log(e.StackTrace);
                }
            }
            redirects.Clear();
        }

        /// <summary>
        ///
        /// </summary>
        private void ReadFromXML()
        {
            // Check the exe directory first
            DataStore.currentFileLocation = ColossalFramework.IO.DataLocation.executableDirectory + Path.DirectorySeparatorChar + XML_FILE;
            bool fileAvailable = File.Exists(DataStore.currentFileLocation);

            if (!fileAvailable)
            {
                // Switch to default which is the cities skylines in the application data area.
                DataStore.currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XML_FILE;
                fileAvailable = File.Exists(DataStore.currentFileLocation);
            }

            if (fileAvailable)
            {
                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionSix();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(DataStore.currentFileLocation);

                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version > 3 && version <= 5)
                    {
                        // Use version 5
                        reader = new XML_VersionFive();

                        // Make a back up copy of the old system to be safe
                        File.Copy(DataStore.currentFileLocation, DataStore.currentFileLocation + ".ver5", true);
                        string error = "Detected an old version of the XML (v5). " + DataStore.currentFileLocation + ".ver5 has been created for future reference and will be upgraded to the new version.";
                        Debugging.bufferWarning(error);
                        UnityEngine.Debug.Log(error);
                    }
                    else if (version <= 3) // Uh oh... version 4 was a while back..
                    {
                        string error = "Detected an unsupported version of the XML (v4 or less). Backing up for a new configuration as :" + DataStore.currentFileLocation + ".ver4";
                        Debugging.bufferWarning(error);
                        UnityEngine.Debug.Log(error);
                        File.Copy(DataStore.currentFileLocation, DataStore.currentFileLocation + ".ver4", true);
                        return;
                    }
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.bufferWarning("The following exception(s) were detected while loading the XML file. Some (or all) values may not be loaded.");
                    Debugging.bufferWarning(e.Message);
                    UnityEngine.Debug.LogException(e);
                }
            }
            else
            {
                UnityEngine.Debug.Log("Configuration file not found. Will output new file to : " + DataStore.currentFileLocation);
            }
        }
    }
}
