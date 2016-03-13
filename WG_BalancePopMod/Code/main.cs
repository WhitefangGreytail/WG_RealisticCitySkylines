using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICities;
using System.Diagnostics;
using Boformer.Redirection;
using ColossalFramework.Math;

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


        // This can be with the local application directory, or the directory where the exe file exists.
        // Default location is the local application directory, however the exe directory is checked first
        private string currentFileLocation = "";
        private static volatile bool isModEnabled = false;
        private static volatile bool isLevelLoaded = false;

        public override void OnCreated(ILoading loading)
        {
            if (!isModEnabled)
            {
                isModEnabled = true;
                Stopwatch sw = Stopwatch.StartNew();

                Redirect(true);
                readFromXML();

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
                        Debugging.writeDebugToFile("Seed collision at number: " + i);
                    }
                }

                sw.Stop();
                UnityEngine.Debug.Log("WG_BalancedPopMod: Successfully loaded in " + sw.ElapsedMilliseconds + " ms.");
            }
        }


        public override void OnReleased()
        {
            if (isModEnabled)
            {
                isModEnabled = false;

                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionFive();
                    xml.writeXML(currentFileLocation);
                }
                catch (Exception e)
                {
                    Debugging.panelMessage(e.Message);
                }
                DataStore.clearCache();

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
                    Debugging.writeDebugToFile($"An error occured while applying {type.Name} redirects!");
                    Debugging.writeDebugToFile(e.StackTrace);
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
                    Debugging.writeDebugToFile($"An error occured while reverting {kvp.Key.Name} redirect!");
                    Debugging.writeDebugToFile(e.StackTrace);
                }
            }
            redirects.Clear();
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

                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version == 4)
                    {
                        reader = new XML_VersionFour();

                        // Make a back up copy of the old system to be safe
                        File.Copy(currentFileLocation, currentFileLocation + ".ver4", true);
                        string error = "Detected an old version of the XML (v4). " + currentFileLocation + ".ver4 has been created for future reference and will be upgraded to the new version.";
                        Debugging.panelWarning(error);
                        UnityEngine.Debug.Log(error);
                    }
                    else if (version <= 3) // Uh oh... version 3 was a while back..
                    {
                        string error = "Detected an unsupported version of the XML (v3 or less). Backing up for a new configuration as :" + currentFileLocation + ".ver3";
                        Debugging.panelWarning(error);
                        UnityEngine.Debug.Log(error);
                        File.Copy(currentFileLocation, currentFileLocation + ".ver3", true);
                        return;
                    }
                    reader.readXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Debugging.panelMessage("Detected exception. Unloaded values will become default. See unity log for more details");
                    UnityEngine.Debug.LogException(e);
                }
            }
            else
            {
                Debugging.panelMessage("Configuration file not found. Will output new file to : " + currentFileLocation);
            }
        }
    }
}
