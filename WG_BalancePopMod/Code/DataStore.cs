using System.Collections.Generic;


namespace WG_BalancedPopMod
{
    public class DataStore
    {
        public const int CACHE_SIZE = 32768;

        // Array indexes
        // TODO - Turn into structs
        public const int PEOPLE = 0;  // sqm per person, also calculated value for internal use only
        public const int LEVEL_HEIGHT = 1;  // m per floor.
        public const int DENSIFICATION = 2;  //
        public const int CALC_METHOD = 3;
        public const int VISIT = 4;

        public const int WORK_LVL0 = 5;
        public const int WORK_LVL1 = WORK_LVL0 + 1; // 6
        public const int WORK_LVL2 = WORK_LVL1 + 1; // 7
        public const int WORK_LVL3 = WORK_LVL2 + 1; // 8

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
        public static bool enableVisitMultiplier = false;
        public static bool timeBasedRealism = false;
        public static bool strictCapacity = true;

        // Static as required for functionality in EnsureCitizenUnits, but after loading the city
        public static bool allowRemovalOfCitizens = false;

        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   10, 24, 18, 14, 140,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   11, 26, 20, 13, 150,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   12, 28, 22, 12, 160,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   13, 31, 25, 11, 170,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   14, 34, 28, 10, 180,   0, 1,   -1} };

        public static int[][] residentialHigh = { new int [] {140, 5, -1, 0, -1,   -1, -1, -1, -1,    9, 18, 14, 11, 105,   0, 5,   -1},
                                                  new int [] {145, 5, -1, 0, -1,   -1, -1, -1, -1,   10, 20, 16, 10, 112,   0, 5,   -1},
                                                  new int [] {150, 5, -1, 0, -1,   -1, -1, -1, -1,   11, 22, 18,  9, 120,   0, 5,   -1},
                                                  new int [] {160, 5, -1, 0, -1,   -1, -1, -1, -1,   12, 25, 21,  8, 127,   0, 5,   -1},
                                                  new int [] {170, 5, -1, 0, -1,   -1, -1, -1, -1,   13, 28, 24,  7, 135,   0, 5,   -1} };

        // High floor levels to help with maintaining a single story 
        public static int[][] commercialLow = { new int [] {100, 6, 1, 0,  90,   70, 20, 10,  0,   14, 40, 40, 11, 750,   0, 100,   -1},
                                                new int [] {105, 6, 1, 0, 100,   30, 45, 20,  5,   16, 45, 45, 10, 800,   0,  90,   -1},
                                                new int [] {110, 6, 1, 0, 110,    5, 30, 55, 10,   18, 50, 50,  9, 850,   0,  75,   -1} };

        public static int[][] commercialHigh = { new int [] {115, 5, 1, 0, 220,   10, 45, 40,  5,   16, 45, 45, 11, 750,   0, 80,   -1},
                                                 new int [] {120, 5, 1, 0, 310,    7, 32, 43, 18,   18, 50, 50, 10, 800,   0, 70,   -1},
                                                 new int [] {125, 5, 1, 0, 400,    5, 25, 45, 25,   20, 55, 55,  9, 850,   0, 60,   -1} };

        // High floor level to get a dense base and to account for hotel employment structure.
        // Every other tourist building seems to be low height
        public static int[][] commercialTourist = { new int[] {1000, 10, 50, 0, 250,   15, 35, 35, 15,   35, 60, 65, 40, 900,   0, 150,   -1 } };

        // Seems to be short buildings all the time
        public static int[][] commercialLeisure = { new int[] {60, 10, 0, 0, 250,   15, 40, 35, 10,   35, 40, 44, 35, 700,   0, 300,   -1 } };

        public static int[][] office = { new int [] {34, 5, 0, 0, -1,   2,  8, 20, 70,   20,  9,  8, 9, 2000,   0, 1,   20},
                                         new int [] {36, 5, 0, 0, -1,   1,  5, 14, 80,   24, 10,  9, 9, 2250,   0, 1,   20},
                                         new int [] {38, 5, 0, 0, -1,   1,  3,  6, 90,   28, 11, 10, 9, 2500,   0, 1,   20} };

        // Very high floor level because chimney stacks count to height level
        public static int[][] industry = { new int [] {38, 50, 0, 0, -1,   70, 20, 10,  0,    50, 100, 120, 50, 200,   300, 300,   100},
                                           new int [] {35, 50, 0, 0, -1,   20, 45, 25, 10,    75, 130, 150, 48, 230,   150, 150,   140},
                                           new int [] {32, 50, 0, 0, -1,    5, 20, 45, 30,   100, 160, 180, 46, 260,    25,  50,   160} };

        public static int[][] industry_farm = { new int [] {250, 50, 0, 0, -1,   90, 10,  0, 0,   30, 200, 225, 30, 180,   0, 175,    25},
                                                new int [] { 55, 25, 0, 0, -1,   30, 60, 10, 0,   70, 400, 500, 50, 220,   0, 180,   100} };

        // The bounding box for a forest plantation is small
        public static int[][] industry_forest = { new int [] {160, 50, 0, 0, -1,   90, 10,  0, 0,   30, 30, 40, 40, 180,   0, 210,    25},
                                                  new int [] { 45, 20, 0, 0, -1,   30, 60, 10, 0,   80, 75, 80, 50, 240,   0, 200,   100} };

        public static int[][] industry_ore = { new int [] {80, 50, 0, 0, -1,   18, 60, 20,  2,   100, 200, 200, 80, 240,   400, 500,    75},
                                               new int [] {40, 30, 0, 0, -1,   15, 40, 35, 10,   200, 210, 280, 60, 300,   300, 475,   100} };

        public static int[][] industry_oil = { new int [] {80, 50, 0, 0, -1,   15, 60, 23,  2,   120, 200, 250, 60, 300,   450, 375,    75},
                                               new int [] {38, 30, 0, 0, -1,   10, 35, 45, 10,   240, 220, 300, 75, 400,   300, 400,   100} };

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
            { "L3 4x4 Semi-detachedhouse03a", 1 },
            { "H4 4x3 Tenement10", -11 },
            { "H4 4x4 Tenement12", -4 },
            { "H5 4x4 Highrise02", -8 },
            { "H5 4x4 Highrise07", -20 },
            { "H5 4x4 Highrise08", 11 },
            { "413694648.Tranquil Turquoise_Data", -20 },
            { "421547247.Gula's Adam Highrise_Data", -18 },
            { "453820359.Sprawl_Data", -12 },
            { "460321524.Truancy_Data", -22 },
            { "472267886.ZED68-PACHATOWER-4x4HRL5_Data", -31 },
            { "665177868.PURGIO S_CITY 101_Data", -577 },
            { "666347361.Nurture_Data", -19 },
            { "673778168.PURGIO S_CITY 103_Data", -451 },
            { "681918587.Marina Torch_Data", -787 },
            { "690181725.Aura Tower_Data", -578 },
            { "691942109.Sulafa tower_Data", -738 }
        };

        public static bool printEmploymentNames = false;
        public static Dictionary<string, int> bonusWorkerCache = new Dictionary<string, int>()
        {
            { "419078725.Barry Plaza_Data", 122 },
            { "428925673.Leviathan_Data", -362 },
            { "434097849.Monolith_Data", -1033 },
            { "435267158.Twin Monoliths_Data", -2768 },
            { "453154792.Gula's Kingdom Tower_Data", -15738 },
            { "476346019.Blue Rise Plaza_Data", -167 },
            { "509112645.Crystallization_Data", -155 },
            { "527535273.snowjaoONEONE_Data", -4868 },
            { "534117358.Two Prudential Plaza_Data", 46 },
            { "568336474.Javelin_Data", -145 },
            { "579129058.Figueroa at Wilshire 1:1_Data", -606 },
            { "604009267.One World Trade Center 1:1_Data", -533 },
            { "605000091.60 Wall Street_Data", -885 },
            { "621431766.Glory_Data", -2017 },
            { "622221778.Stalwart_Data", -148 },
            { "628101617.Pan Pacific Insurance_Data", -1770 },
            { "651651763.Alliance_Data", -154 },
            { "660411166.Elstree Building_Data", -2136 },
            { "660864619.Policy_Data", 42 },
            { "661075324.Santander Tower_Data", -261 },
            { "661823191.Whirligig XL_Data", 89 },
            { "670944160.Big Office Building_Data", -2003 },
            { "672448363.Foster Tower Madrid (Red)_Data", -440 },
            { "672525995.Foster Tower Madrid (White)_Data", -440 },
            { "680500415.Space Tower Madrid_Data", -404 },
            { "687777086.Mirage_Data", -1607 }
        };

        // Prefab stores
        public static Dictionary<int, int> prefabHouseHolds = new Dictionary<int, int>(512);
        public static Dictionary<int, prefabEmployStruct> prefabWorkerVisit = new Dictionary<int, prefabEmployStruct>(1024);
        public static Dictionary<ulong, ushort> seedToId = new Dictionary<ulong, ushort>();

        public static void clearCache()
        {
            prefabHouseHolds.Clear();
            prefabWorkerVisit.Clear();
            seedToId.Clear();
        }
    } // end DataStore


    /// <summary>
    /// Struct for caching prefab worker values
    /// </summary>
    public struct prefabEmployStruct
    {
        public int level0;
        public int level1;
        public int level2;
        public int level3;
        public int visitors;
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
