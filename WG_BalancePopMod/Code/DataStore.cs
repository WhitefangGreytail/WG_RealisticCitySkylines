using System;
using System.Collections.Generic;
using System.IO;

namespace WG_BalancedPopMod
{
    public class DataStore
    {
        public const int CACHE_SIZE = 65536;

        // Array indexes
        public const int PEOPLE = 0;  // sqm per person, also calculated value for internal use only
        public const int LEVEL_HEIGHT = 1;  // m per floor.
        public const int DENSIFICATION = 2;  //

        public const int WORK_LVL0 = 3;
        public const int WORK_LVL1 = WORK_LVL0 + 1; // 4
        public const int WORK_LVL2 = WORK_LVL1 + 1; // 5
        public const int WORK_LVL3 = WORK_LVL2 + 1; // 6

        public const int POWER = 7;
        public const int WATER = POWER + 1; // 8
        public const int SEWAGE = WATER + 1; // 9
        public const int GARBAGE = SEWAGE + 1; // 10
        public const int INCOME = GARBAGE + 1; // 11

        public const int GROUND_POLLUTION = 12;
        public const int NOISE_POLLUTION = GROUND_POLLUTION + 1; // 13

        public const int PRODUCTION = 14;

        // Flags in XML
        public static bool enableExperimental = false;
        public static bool timeBasedRealism = false;

        // Static as required for functionality in EnsureCitizenUnits
        public static bool allowRemovalOfCitizens = false;


        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] { 800, 20, -1,   -1, -1, -1, -1,   10, 24, 18, 14, 100,   0, 1,   -1},
                                                 new int [] { 900, 20, -1,   -1, -1, -1, -1,   12, 26, 20, 13, 120,   0, 1,   -1},
                                                 new int [] {1000, 20, -1,   -1, -1, -1, -1,   14, 28, 22, 12, 140,   0, 1,   -1},
                                                 new int [] {1100, 20, -1,   -1, -1, -1, -1,   16, 31, 25, 11, 160,   0, 1,   -1},
                                                 new int [] {1200, 20, -1,   -1, -1, -1, -1,   18, 34, 28, 10, 180,   0, 1,   -1} };

        public static int[][] residentialHigh = { new int [] {150, 5, -1,   -1, -1, -1, -1,    8, 20, 16, 11,  80,   0, 5,   -1},
                                                  new int [] {165, 5, -1,   -1, -1, -1, -1,   10, 22, 18, 10,  96,   0, 5,   -1},
                                                  new int [] {180, 5, -1,   -1, -1, -1, -1,   12, 24, 20,  9, 112,   0, 5,   -1},
                                                  new int [] {200, 5, -1,   -1, -1, -1, -1,   14, 27, 23,  8, 128,   0, 5,   -1},
                                                  new int [] {220, 5, -1,   -1, -1, -1, -1,   16, 30, 26,  7, 144,   0, 5,   -1} };

        // High floor levels to help with maintaining a single story 
        public static int[][] commercialLow = { new int [] {150, 7, 2,   70, 20, 10,  0,   28, 40, 40, 11, 650,   0, 100,   -1},
                                                new int [] {110, 6, 1,   30, 45, 20,  5,   32, 45, 45, 10, 700,   0,  90,   -1},
                                                new int [] {115, 5, 1,    5, 30, 55, 10,   36, 50, 50,  9, 750,   0,  75,   -1} };

        public static int[][] commercialHigh = { new int [] {110, 5, 1,   10, 40, 40,  5,   30, 45, 45, 11, 700,   0, 80,   -1},
                                                 new int [] {120, 5, 1,    7, 32, 43, 18,   34, 50, 50, 10, 750,   0, 70,   -1},
                                                 new int [] {130, 5, 1,    5, 25, 45, 25,   38, 55, 55,  9, 800,   0, 60,   -1} };

        // High floor level to get a dense base and to account for hotel employment structure.
        // Every other tourist building seems to be low height
        public static int[][] commercialTourist = { new int[] {1200, 10, 50,   15, 35, 35, 15,   40, 60, 65, 40, 650,   0, 150,   -1 } };

        // Seems to be short buildings all the time
        public static int[][] commercialLeisure = { new int[] {60, 10, 0,   15, 40, 35, 10,   40, 40, 44, 35, 700,   0, 300,   -1 } };

        public static int[][] office = { new int [] {35, 5, 0,   2,  8, 20, 70,    80, 14, 13, 10, 2000,   0, 1,   20},
                                         new int [] {38, 5, 0,   1,  5, 14, 80,    90, 15, 14, 10, 2200,   0, 1,   20},
                                         new int [] {42, 5, 0,   1,  3,  6, 90,   100, 16, 15,  9, 2400,   0, 1,   20} };

        // Very high floor level because chimney stacks count to height level
        public static int[][] industry = { new int [] {38, 50, 0,   70, 20, 10,  0,   100, 100, 120, 50, 170,   300, 300,   100},
                                           new int [] {35, 50, 0,   20, 45, 25, 10,   150, 130, 150, 48, 200,   150, 150,   140},
                                           new int [] {32, 50, 0,    5, 20, 45, 30,   200, 160, 180, 46, 240,    25,  50,   160} };

        public static int[][] industry_farm = { new int [] {250, 25, 0,   90, 10,  0, 0,    70, 200, 225, 30, 180,   0, 175,    25},
                                                new int [] { 55, 25, 0,   30, 60, 10, 0,   120, 400, 500, 50, 210,   0, 180,   100} };

        // The bounding box for a forest plantation is small
        public static int[][] industry_forest = { new int [] {150, 20, 0,   90, 10,  0, 0,    90, 30, 40, 40, 180,   0, 210,    25},
                                                  new int [] { 45, 20, 0,   30, 60, 10, 0,   130, 75, 80, 50, 220,   0, 200,   100} };

        public static int[][] industry_ore = { new int [] {80, 30, 0,   18, 60, 20,  2,   180, 200, 200, 80, 240,   400, 500,    75},
                                               new int [] {40, 30, 0,   15, 40, 35, 10,   240, 210, 280, 60, 300,   300, 475,   100} };

        public static int[][] industry_oil = { new int [] {80, 30, 0,   15, 60, 23,  2,   200, 200, 250, 60, 300,   450, 375,    75},
                                               new int [] {38, 30, 0,   10, 35, 45, 10,   280, 220, 300, 75, 380,   300, 400,   100} };
    } // end DataStore

        
    /// <summary>
    /// Struct for caching CalculateWorkplaceCount values
    /// </summary>
    public struct employStruct
    {
        public int level;
        public int level0;
        public int level1;
        public int level2;
        public int level3;
    }

    /// <summary>
    /// Struct for caching GetConsumptionRates values
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
