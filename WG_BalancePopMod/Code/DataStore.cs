using System;
using System.Collections.Generic;
using System.IO;

namespace WG_BalancedPopMod
{
    public class DataStore
    {
        public const int CACHE_SIZE = 32768;

        // Array indexes
        public const int PEOPLE = 0;  // sqm per person, also calculated value for internal use only
        public const int LEVEL_HEIGHT = 1;  // m per floor.
        public const int DENSIFICATION = 2;  //
        public const int CALC_METHOD = 3;

        public const int WORK_LVL0 = 4;
        public const int WORK_LVL1 = WORK_LVL0 + 1; // 5
        public const int WORK_LVL2 = WORK_LVL1 + 1; // 6
        public const int WORK_LVL3 = WORK_LVL2 + 1; // 7

        public const int VISIT_MULT = 8;

        public const int POWER = 9;
        public const int WATER = POWER + 1; // 10
        public const int SEWAGE = WATER + 1; // 11
        public const int GARBAGE = SEWAGE + 1; // 12
        public const int INCOME = GARBAGE + 1; // 13

        public const int GROUND_POLLUTION = 14;
        public const int NOISE_POLLUTION = GROUND_POLLUTION + 1; // 15

        public const int PRODUCTION = 16;


        // Flags in XML
        public static bool enableExperimental = false;
        public static bool timeBasedRealism = false;

        // Static as required for functionality in EnsureCitizenUnits, but after loading the city
        public static bool allowRemovalOfCitizens = false;

        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] {2000, 50, -1, 0,   -1, -1, -1, -1,   -1,   10, 24, 18, 14, 120,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0,   -1, -1, -1, -1,   -1,   12, 26, 20, 13, 140,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0,   -1, -1, -1, -1,   -1,   14, 28, 22, 12, 160,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0,   -1, -1, -1, -1,   -1,   16, 31, 25, 11, 180,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0,   -1, -1, -1, -1,   -1,   18, 34, 28, 10, 200,   0, 1,   -1} };

        public static int[][] residentialHigh = { new int [] {150, 5, -1, 0,   -1, -1, -1, -1,   -1,    8, 20, 16, 11,  96,   0, 5,   -1},
                                                  new int [] {160, 5, -1, 0,   -1, -1, -1, -1,   -1,   10, 22, 18, 10, 112,   0, 5,   -1},
                                                  new int [] {170, 5, -1, 0,   -1, -1, -1, -1,   -1,   12, 24, 20,  9, 128,   0, 5,   -1},
                                                  new int [] {180, 5, -1, 0,   -1, -1, -1, -1,   -1,   14, 27, 23,  8, 144,   0, 5,   -1},
                                                  new int [] {190, 5, -1, 0,   -1, -1, -1, -1,   -1,   16, 30, 26,  7, 160,   0, 5,   -1} };

        // High floor levels to help with maintaining a single story 
        public static int[][] commercialLow = { new int [] { 90, 5, 1, 0,   70, 20, 10,  0,   1,   28, 40, 40, 11, 700,   0, 100,   -1},
                                                new int [] { 95, 5, 1, 0,   30, 45, 20,  5,   1,   32, 45, 45, 10, 750,   0,  90,   -1},
                                                new int [] {100, 5, 1, 0,    5, 30, 55, 10,   1,   36, 50, 50,  9, 800,   0,  75,   -1} };

        public static int[][] commercialHigh = { new int [] {110, 5, 1, 0,   10, 45, 40,  5,   1,   30, 45, 45, 11, 750,   0, 80,   -1},
                                                 new int [] {115, 5, 1, 0,    7, 32, 43, 18,   1,   34, 50, 50, 10, 800,   0, 70,   -1},
                                                 new int [] {120, 5, 1, 0,    5, 25, 45, 25,   1,   38, 55, 55,  9, 850,   0, 60,   -1} };

        // High floor level to get a dense base and to account for hotel employment structure.
        // Every other tourist building seems to be low height
        public static int[][] commercialTourist = { new int[] {1200, 10, 50, 0,   15, 35, 35, 15,  1,    40, 60, 65, 40, 750,   0, 150,   -1 } };

        // Seems to be short buildings all the time
        public static int[][] commercialLeisure = { new int[] {60, 10, 0, 0,   15, 40, 35, 10,   1,   40, 40, 44, 35, 700,   0, 300,   -1 } };

        public static int[][] office = { new int [] {34, 5, 0, 0,   2,  8, 20, 70,   -1,   24, 12, 11, 9, 1800,   0, 1,   20},
                                         new int [] {37, 5, 0, 0,   1,  5, 14, 80,   -1,   27, 13, 12, 9, 1925,   0, 1,   20},
                                         new int [] {40, 5, 0, 0,   1,  3,  6, 90,   -1,   32, 14, 13, 8, 2100,   0, 1,   20} };

        // Very high floor level because chimney stacks count to height level
        public static int[][] industry = { new int [] {38, 50, 0, 0,   70, 20, 10,  0,   -1,   100, 100, 120, 50, 180,   300, 300,   100},
                                           new int [] {35, 50, 0, 0,   20, 45, 25, 10,   -1,   150, 130, 150, 48, 210,   150, 150,   140},
                                           new int [] {32, 50, 0, 0,    5, 20, 45, 30,   -1,   200, 160, 180, 46, 250,    25,  50,   160} };

        public static int[][] industry_farm = { new int [] {250, 50, 0, 0,   90, 10,  0, 0,   -1,    70, 200, 225, 30, 180,   0, 175,    25},
                                                new int [] { 55, 25, 0, 0,   30, 60, 10, 0,   -1,   120, 400, 500, 50, 210,   0, 180,   100} };

        // The bounding box for a forest plantation is small
        public static int[][] industry_forest = { new int [] {160, 50, 0, 0,   90, 10,  0, 0,   -1,    90, 30, 40, 40, 180,   0, 210,    25},
                                                  new int [] { 45, 20, 0, 0,   30, 60, 10, 0,   -1,   130, 75, 80, 50, 220,   0, 200,   100} };

        public static int[][] industry_ore = { new int [] {80, 50, 0, 0,   18, 60, 20,  2,   -1,   180, 200, 200, 80, 240,   400, 500,    75},
                                               new int [] {40, 30, 0, 0,   15, 40, 35, 10,   -1,   240, 210, 280, 60, 300,   300, 475,   100} };

        public static int[][] industry_oil = { new int [] {80, 50, 0, 0,   15, 60, 23,  2,   -1,   200, 200, 250, 60, 300,   450, 375,    75},
                                               new int [] {38, 30, 0, 0,   10, 35, 45, 10,   -1,   280, 220, 300, 75, 380,   300, 400,   100} };

        // Bonus house hold data structure
        public static bool printResidentialNames = false;
        public static Dictionary<string, int> bonusHouseholdCache = new Dictionary<string, int>()
        {
            { "L1 2x3 Detached05", 1 },
            { "L1 3x3 Detached02", 1 },
            { "L1 4x4 Detached02", 1 },
            { "L1 4x4 Detached06a", 1 },
            { "L1 4x4 Detached11", 1 },
            { "L2 2x2 Detached05",  1 },
            { "L2 2x3 Semi-detachedhouse01", 1 },
            { "L2 3x4 Semi-detachedhouse02a", 1 },
            { "L3 3x3 Semi-detachedhouse02", 1 },
            { "L3 4x4 Semi-detachedhouse03a", 1 }
        };

        // Prefab stores
        public static Dictionary<int, int> prefabHouseHolds = new Dictionary<int, int>(512);
        public static Dictionary<int, prefabEmployStruct> prefabWorkers = new Dictionary<int, prefabEmployStruct>(1024);
        public static Dictionary<int, int> prefabVistors = new Dictionary<int, int>(512);

        public static void clearCache()
        {
            prefabHouseHolds.Clear();
            prefabWorkers.Clear();
            prefabVistors.Clear();
        }
    } // end DataStore


    /// <summary>
    /// Struct for caching prefab worker values
    /// </summary>
    public struct buildingEmployStruct
    {
        public int level;
        public int level0;
        public int level1;
        public int level2;
        public int level3;
    }

    /// <summary>
    /// Struct for caching prefab worker values
    /// </summary>
    public struct prefabEmployStruct
    {
        public int level0;
        public int level1;
        public int level2;
        public int level3;
    }

    /// <summary>
    /// Struct for caching building's consumption values
    /// </summary>
    public struct consumeStruct
    {
        public int productionRate;
        public int electricity;
        public int water;
        public int sewage;
        public int garbage;
        public int income;
    }
}
