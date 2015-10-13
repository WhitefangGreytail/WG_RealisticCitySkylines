using System;

namespace WG_BalancedPopMod
{
    public class DataStore
    {
        // Array indexes
        public const int PEOPLE = 0;  // sqm per person, also calculated value for internal use only
        public const int LEVEL_HEIGHT = 1;  // m per floor

        public const int WORK_LVL0 = 2;
        public const int WORK_LVL1 = WORK_LVL0 + 1; // 3
        public const int WORK_LVL2 = WORK_LVL1 + 1; // 4
        public const int WORK_LVL3 = WORK_LVL2 + 1; // 5

        public const int POWER = 6;
        public const int WATER = POWER + 1; // 7
        public const int SEWAGE = WATER + 1; // 8
        public const int GARBAGE = SEWAGE + 1; // 9
        public const int INCOME = GARBAGE + 1; // 10

        public const int GROUND_POLLUTION = 11;
        public const int NOISE_POLLUTION = GROUND_POLLUTION + 1; // 12

        public const int PRODUCTION = 12;

        public static bool enableExperimental = false;

        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] { 800, 20,   -1, -1, -1, -1,   10, 28, 20, 15, 100,   0, 1},
                                                 new int [] { 900, 20,   -1, -1, -1, -1,   14, 30, 22, 14, 120,   0, 1},
                                                 new int [] {1000, 20,   -1, -1, -1, -1,   16, 33, 25, 13, 140,   0, 1},
                                                 new int [] {1100, 20,   -1, -1, -1, -1,   18, 36, 28, 12, 160,   0, 1},
                                                 new int [] {1200, 20,   -1, -1, -1, -1,   20, 40, 32, 11, 180,   0, 1} };

        public static int[][] residentialHigh = { new int [] {150, 5,   -1, -1, -1, -1,    8, 20, 16, 11, 80,   0, 5},
                                                  new int [] {160, 5,   -1, -1, -1, -1,   10, 22, 18, 10, 96,   0, 5},
                                                  new int [] {175, 5,   -1, -1, -1, -1,   12, 24, 20, 9, 112,   0, 5},
                                                  new int [] {190, 5,   -1, -1, -1, -1,   14, 27, 23, 8, 128,   0, 5},
                                                  new int [] {210, 5,   -1, -1, -1, -1,   16, 30, 26, 7, 144,   0, 5} };

        // High floor levels to help with maintaining a single story 
        public static int[][] commercialLow = { new int [] {110, 7,   90, 10,  0,  0,   25, 45, 45, 12, 600,   0, 100},
                                                new int [] {112, 6,   35, 40, 20,  5,   30, 50, 50, 11, 650,   0,  90},
                                                new int [] {115, 5,   15, 30, 40, 15,   40, 55, 55, 10, 700,   0,  75} };

        public static int[][] commercialHigh = { new int [] {100, 5,   20, 40, 30,  5,   25, 45, 45, 11, 650,   0, 80},
                                                 new int [] {110, 5,   15, 35, 35, 15,   30, 50, 50, 10, 700,   0, 70},
                                                 new int [] {120, 5,   10, 25, 40, 25,   40, 55, 55,  9, 750,   0, 60} };

        // High floor level to get a dense base and to account for hotel employment structure.
        // Every other tourist building seems to be low height
        public static int[][] commercialTourist = { new int[] {32, 35,   20, 30, 35, 15,   30, 70, 80, 50, 600,   0, 150 } };

        // Seems to be short buildings all the time
        public static int[][] commercialLeisure = { new int[] {60, 10,   25, 40, 25, 10,   45, 55, 60, 45, 700,   0, 300 } };

        public static int[][] office = { new int [] {35, 5,   3, 17, 40, 40,   100,  80,  80, 20,  900,   0, 1},
                                         new int [] {37, 5,   3, 10, 24, 63,   150, 100, 100, 20, 1100,   0, 1},
                                         new int [] {40, 5,   3,  4,  8, 85,   200, 120, 120, 18, 1300,   0, 1} };

        // Very high floor level because chimney stacks count to height level
        public static int[][] industry = { new int [] {38, 50,   90, 10,  0,  0,   100, 100, 120, 50, 160,   300, 300},
                                           new int [] {35, 50,   25, 40, 25, 10,   150, 130, 150, 48, 190,   150, 150},
                                           new int [] {32, 50,   10, 20, 40, 30,   200, 160, 180, 46, 225,    25,  50} };

        public static int[][] industry_farm = { new int [] {150, 25,   90, 10, 0, 0,    75, 200, 225, 30, 170,   0, 175},
                                                new int [] { 55, 25,   45, 50, 5, 0,   120, 400, 500, 50, 200,   0, 180} };

        // The bounding box for a forest plantation is tiny. Why is this so?
        public static int[][] industry_forest = { new int [] {150, 20,   90, 10, 0, 0,    90, 30, 40, 40, 170,   0, 210},
                                                  new int [] {45, 20,   40, 55, 5, 0,   130, 75, 80, 50, 210,   0, 200} };

        public static int[][] industry_ore = { new int [] {80, 30,   18, 60, 20,  2,   180, 200, 200, 80, 220,   400, 500},
                                               new int [] {40, 30,   15, 40, 35, 10,   240, 210, 280, 60, 300,   300, 475} };

        public static int[][] industry_oil = { new int [] {80, 30,   15, 60, 23,  2,   200, 200, 250, 60, 300,   450, 375},
                                               new int [] {38, 30,   15, 35, 40, 10,   280, 220, 300, 75, 350,   300, 400} };
    }
}