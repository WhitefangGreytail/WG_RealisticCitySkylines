using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WG_BalancedPopMod
{
    class ExternalCalls
    {
        public static int GetResidential(string name)
        {
            int returnValue;
            if (DataStore.householdCache.TryGetValue(name, out returnValue))
            {
                return returnValue;
            }
            return 0;
        }

        public static void AddResidential(string name, int houses)
        {
            Dictionary<string, int> cache = DataStore.householdCache;
            if (cache.ContainsKey(name))
            {
                cache.Remove(name);
            }
            cache.Add(name, houses);
        }

        public static void RemoveResidential(string name)
        {
            Dictionary<string, int> cache = DataStore.householdCache;
            cache.Remove(name);
            // Add the default back in if it is there
            if (DataStore.defaultHousehold.ContainsKey(name))
            {
                int value;
                DataStore.defaultHousehold.TryGetValue(name, out value);
                cache.Add(name, value);
            }
        }

        public static int GeWorker(string name)
        {
            int returnValue;
            if (DataStore.workerCache.TryGetValue(name, out returnValue))
            {
                return returnValue;
            }
            return 0;
        }

        public static void AddWorker(string name, int workers)
        {
            Dictionary<string, int> cache = DataStore.workerCache;
            if (cache.ContainsKey(name))
            {
                cache.Remove(name);
            }
            cache.Add(name, workers);
        }

        public static void RemoveWorker(string name)
        {
            Dictionary<string, int> cache = DataStore.workerCache;
            cache.Remove(name);
            // Add the default back in if it is there
            if (DataStore.defaultWorker.ContainsKey(name))
            {
                int value;
                DataStore.defaultWorker.TryGetValue(name, out value);
                cache.Add(name, value);
            }
        }

        public static void Save()
        {
            WG_XMLBaseVersion xml = new XML_VersionSix();
            xml.writeXML(DataStore.currentFileLocation);
        }
    }
}
