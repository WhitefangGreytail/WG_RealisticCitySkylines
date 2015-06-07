using System;

namespace WG_BalancedPopMod
{
    public class DataStore
    {
        // Array indexes
        public const int PEOPLE = 0;
        public const int POWER = 1;
        public const int WATER = 2;
        public const int SEWAGE = 3;
        public const int GARBAGE = 4;
        public const int INCOME = 5;
        public const int GROUND_POLLUTION = 6;
        public const int NOISE_POLLUTION = 7;


        // Water is consumed in the process of watering the lawns.
        // As well as water consumed for drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] {9, 10, 38, 30, 16, 100, 0, 1},
                                                 new int [] {8, 14, 40, 32, 15, 120, 0, 1},
                                                 new int [] {8, 16, 43, 35, 14, 140, 0, 1},
                                                 new int [] {7, 18, 46, 38, 13, 160, 0, 1},
                                                 new int [] {6, 20, 50, 42, 12, 180, 0, 1} };

        public static int[][] residentialHigh = { new int [] {200, 8, 24, 22, 11, 80, 0, 5},
                                                  new int [] {250, 10, 26, 24, 10, 96, 0, 5},
                                                  new int [] {300, 12, 28, 26, 9, 112, 0, 5},
                                                  new int [] {350, 14, 30, 28, 8, 128, 0, 5},
                                                  new int [] {375, 16, 33, 30, 7, 144, 0, 5} };

        public static int[][] commercialLow = { new int [] {50, 25, 50, 50, 13, 600, 0, 100},
                                                new int [] {75, 30, 60, 60, 12, 650, 0, 90},
                                                new int [] {130, 40, 70, 70, 11, 700, 0, 80} };

        public static int[][] commercialHigh = { new int [] {200, 25, 45, 45, 11, 575, 0, 80},
                                                 new int [] {300, 30, 50, 50, 10, 625, 0, 70},
                                                 new int [] {450, 40, 55, 55, 9, 675, 0, 60} };

        public static int[][] office = { new int [] {800, 100, 80, 80, 20, 800, 0, 1},
                                         new int [] {1800, 150, 100, 100, 20, 1000, 0, 1},
                                         new int [] {3000, 200, 120, 120, 18, 1300, 0, 1} };

        public static int[][] industry = { new int [] {100, 100, 100, 120, 50, 150, 300, 300},
                                           new int [] {150, 150, 130, 150, 48, 175, 150, 150},
                                           new int [] {200, 200, 160, 180, 46, 200, 25, 50} };

        // Preparation for extraction vs refinement
        public static int[][] industry_farm = { new int [] { 50, 100, 300, 400, 40, 180, 0, 200} };
        public static int[][] industry_forest = { new int [] { 50, 100, 50, 65, 30, 140, 0, 200} };
        public static int[][] industry_ore = { new int [] { 150, 200, 200, 230, 60, 250, 400, 500} };
        public static int[][] industry_oil = { new int[] { 150, 250, 200, 270, 60, 325, 500, 400 } };
    }
}