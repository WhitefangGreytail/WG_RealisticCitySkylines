using System;

namespace WG_BalancedPopMod
{
    public class DataStore
    {
        // Array indexes
        public const int PEOPLE = 0;  // This will store the dividor of floorspace per household
        public const int WORK_LVL0 = 1;
        public const int WORK_LVL1 = 2;
        public const int WORK_LVL2 = 3;
        public const int WORK_LVL3 = 4;
        public const int POWER = 5;
        public const int WATER = 6;
        public const int SEWAGE = 7;
        public const int GARBAGE = 8;
        public const int INCOME = 9;
        public const int GROUND_POLLUTION = 10;
        public const int NOISE_POLLUTION = 11;


        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] {9, -1, -1, -1, -1, 10, 38, 30, 15, 100, 0, 1},
                                                 new int [] {8, -1, -1, -1, -1, 14, 40, 32, 14, 120, 0, 1},
                                                 new int [] {8, -1, -1, -1, -1, 16, 43, 35, 13, 140, 0, 1},
                                                 new int [] {7, -1, -1, -1, -1, 18, 46, 38, 12, 160, 0, 1},
                                                 new int [] {6, -1, -1, -1, -1, 20, 50, 42, 11, 180, 0, 1} };

        public static int[][] residentialHigh = { new int [] {200, -1, -1, -1, -1,  8, 24, 20, 11, 80, 0, 5},
                                                  new int [] {250, -1, -1, -1, -1, 10, 26, 22, 10, 96, 0, 5},
                                                  new int [] {300, -1, -1, -1, -1, 12, 28, 24, 9, 112, 0, 5},
                                                  new int [] {340, -1, -1, -1, -1, 14, 30, 26, 8, 128, 0, 5},
                                                  new int [] {360, -1, -1, -1, -1, 16, 33, 28, 7, 144, 0, 5} };

        public static int[][] commercialLow = { new int [] { 50, 90, 10,  0,  0, 25, 50, 50, 13, 600, 0, 100},
                                                new int [] { 90, 35, 40, 20,  5, 30, 60, 60, 12, 650, 0,  90},
                                                new int [] {140, 15, 30, 40, 15, 40, 70, 70, 11, 700, 0,  75} };

        public static int[][] commercialHigh = { new int [] {200, 10, 35, 45, 10, 25, 45, 45, 11, 600, 0, 80},
                                                 new int [] {350, 10, 30, 40, 20, 30, 50, 50, 10, 650, 0, 70},
                                                 new int [] {500, 10, 25, 35, 30, 40, 55, 55,  9, 700, 0, 60} };

        public static int[][] office = { new int [] { 900, 3, 15, 42, 40, 100,  80,  80, 20,  900, 0, 1},
                                         new int [] {1600, 3, 10, 24, 63, 150, 100, 100, 20, 1100, 0, 1},
                                         new int [] {2500, 3,  4,  8, 85, 200, 120, 120, 18, 1300, 0, 1} };

        public static int[][] industry = { new int [] {100, 90, 10,  0,  0, 100, 100, 120, 50, 160, 300, 300},
                                           new int [] {150, 25, 40, 25, 10, 150, 130, 150, 48, 190, 150, 150},
                                           new int [] {200, 10, 20, 40, 30, 200, 160, 180, 46, 225,  25,  50} };

        public static int[][] industry_farm = { new int [] {  30, 90, 10, 0, 0, 100, 300, 400, 40, 180, 0, 200},
                                                new int [] { 100, 45, 50, 5, 0, 100, 300, 400, 40, 180, 0, 200} };

        public static int[][] industry_forest = { new int [] {  30, 90, 10, 0, 0, 100, 50, 65, 30, 160, 0, 200},
                                                  new int [] { 100, 40, 55, 5, 0, 100, 50, 65, 30, 160, 0, 200} };

        public static int[][] industry_ore = { new int [] { 100, 18, 60, 20,  2, 200, 200, 230, 60, 250, 400, 500},
                                               new int [] { 150, 15, 45, 30, 10, 200, 200, 230, 60, 250, 400, 500} };

        public static int[][] industry_oil = { new int [] { 100, 15, 60, 23,  2, 250, 200, 270, 60, 325, 500, 400},
                                               new int [] { 150, 15, 40, 35, 10, 250, 200, 270, 60, 325, 500, 400} };
    }
}