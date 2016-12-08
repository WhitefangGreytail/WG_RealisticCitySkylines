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
        public const int CALC_METHOD = 3;  // 0 for model, 1 for plot
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
        public static bool strictCapacity = true;

        // Static as required for functionality in EnsureCitizenUnits, but after loading the city
        public static bool allowRemovalOfCitizens = false;

        // Water is consumed in the process of watering the lawns, drinking/cooking/cleaning
        public static int[][] residentialLow = { new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   10, 24, 18, 14, 150,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   11, 26, 20, 13, 160,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   12, 28, 22, 12, 170,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   13, 31, 25, 11, 180,   0, 1,   -1},
                                                 new int [] {2000, 50, -1, 0, -1,   -1, -1, -1, -1,   14, 34, 28, 10, 190,   0, 1,   -1} };

        public static int[][] residentialHigh = { new int [] {140, 5, -1, 0, -1,   -1, -1, -1, -1,    9, 18, 14, 11, 112,   0, 5,   -1},
                                                  new int [] {145, 5, -1, 0, -1,   -1, -1, -1, -1,   10, 20, 16, 10, 120,   0, 5,   -1},
                                                  new int [] {150, 5, -1, 0, -1,   -1, -1, -1, -1,   11, 22, 18,  9, 127,   0, 5,   -1},
                                                  new int [] {160, 5, -1, 0, -1,   -1, -1, -1, -1,   12, 25, 21,  8, 135,   0, 5,   -1},
                                                  new int [] {170, 5, -1, 0, -1,   -1, -1, -1, -1,   13, 28, 24,  7, 143,   0, 5,   -1} };

        // High floor levels to help with maintaining a single story 
        public static int[][] commercialLow = { new int [] {100, 6, 1, 0,  90,   70, 20, 10,  0,   14, 40, 40, 11, 800,   0, 100,   -1},
                                                new int [] {105, 6, 1, 0, 100,   30, 45, 20,  5,   16, 45, 45, 10, 850,   0,  90,   -1},
                                                new int [] {110, 6, 1, 0, 110,    5, 30, 55, 10,   18, 50, 50,  9, 900,   0,  75,   -1} };

        public static int[][] commercialHigh = { new int [] {115, 5, 1, 0, 220,   10, 45, 40,  5,   16, 45, 45, 11, 800,   0, 80,   -1},
                                                 new int [] {120, 5, 1, 0, 310,    7, 32, 43, 18,   18, 50, 50, 10, 850,   0, 70,   -1},
                                                 new int [] {125, 5, 1, 0, 400,    5, 25, 45, 25,   20, 55, 55,  9, 900,   0, 60,   -1} };

        // High floor level to get a dense base and to account for hotel employment structure.
        // Every other tourist building seems to be low height
        public static int[][] commercialTourist = { new int[] {1000, 10, 50, 0, 250,   15, 35, 35, 15,   35, 60, 65, 40, 1000,   0, 150,   -1 } };

        // Seems to be short buildings all the time
        public static int[][] commercialLeisure = { new int[] {60, 10, 0, 0, 250,   15, 40, 35, 10,   35, 40, 44, 35, 800,   0, 300,   -1 } };

        public static int[][] office = { new int [] {34, 5, 0, 0, -1,   2,  8, 20, 70,   10, 4, 4, 4, 1100,   0, 1,   10},
                                         new int [] {36, 5, 0, 0, -1,   1,  5, 14, 80,   12, 5, 4, 4, 1200,   0, 1,   10},
                                         new int [] {38, 5, 0, 0, -1,   1,  3,  6, 90,   14, 5, 5, 4, 1300,   0, 1,   10} };

        // Very high floor level because chimney stacks count to height level
        public static int[][] industry = { new int [] {38, 50, 0, 0, -1,   70, 20, 10,  0,    50, 100, 120, 50, 220,   300, 300,   100},
                                           new int [] {35, 50, 0, 0, -1,   20, 45, 25, 10,    75, 130, 150, 48, 250,   150, 150,   140},
                                           new int [] {32, 50, 0, 0, -1,    5, 20, 45, 30,   100, 160, 180, 46, 270,    25,  50,   160} };

        public static int[][] industry_farm = { new int [] {250, 50, 0, 0, -1,   90, 10,  0, 0,   30, 200, 225, 30, 190,   0, 175,    25},
                                                new int [] { 55, 25, 0, 0, -1,   30, 60, 10, 0,   70, 400, 500, 50, 230,   0, 180,   100} };

        // The bounding box for a forest plantation is small
        public static int[][] industry_forest = { new int [] {160, 50, 0, 0, -1,   90, 10,  0, 0,   30, 30, 40, 40, 190,   0, 210,    25},
                                                  new int [] { 45, 20, 0, 0, -1,   30, 60, 10, 0,   80, 75, 80, 50, 250,   0, 200,   100} };

        public static int[][] industry_ore = { new int [] {80, 50, 0, 0, -1,   18, 60, 20,  2,   100, 200, 200, 80, 250,   400, 500,    75},
                                               new int [] {40, 30, 0, 0, -1,   15, 40, 35, 10,   200, 210, 280, 60, 320,   300, 475,   100} };

        public static int[][] industry_oil = { new int [] {80, 50, 0, 0, -1,   15, 60, 23,  2,   120, 200, 250, 60, 310,   450, 375,    75},
                                               new int [] {38, 30, 0, 0, -1,   10, 35, 45, 10,   240, 220, 300, 75, 420,   300, 400,   100} };

        // Bonus house hold data structure
        public static bool printResidentialNames = false;
        public static bool mergeResidentialNames = true;
        public static Dictionary<string, int> bonusHouseholdCache = new Dictionary<string, int>();
        public static Dictionary<string, int> defaultBonusHousehold = new Dictionary<string, int>()
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
            { "415635897.Highcliff from hongkong_Data", -129 },
            { "421547247.Gula's Adam Highrise_Data", -18 },
            { "417797417.ZED68-PHOENIXTOWER-4x4HRL5_Data", -22 },
            { "417805731.ZED68-RIVERRESIDENCE-4x4HRL4_Data", 0},
            { "425922383.Pearl Tower 1.4_Data", 6 },
            { "453820359.Sprawl_Data", -24 },
            { "460321524.Truancy_Data", -22 },
            { "472267886.ZED68-PACHATOWER-4x4HRL5_Data", -31 },
            { "501398369.ZED68-HOPPER TOWER-4x4HRL5_Data", -17 },
            { "643034042.ZED68-OzOne Building (Ozo)_Data", -4 },
            { "665177868.PURGIO S_CITY 101_Data", -577 },
            { "666347361.Nurture_Data", -19 },
            { "673778168.PURGIO S_CITY 103_Data", -451 },
            { "681918587.Marina Torch_Data", -937 },
            { "690181725.Aura Tower_Data", -578 },
            { "691942109.Sulafa tower_Data", -798 },
            { "700349615.The Pentominium_Data", -1168 },
            //{ "700381657.Terrace_Data", 0 },
            { "719510655.Launch_Data", -2090 },
            { "727276417.City Commons Condos_Data", 5 },
            { "749570687.Chelsea Tower_Data", -48 },
            { "754279432.Ahmed Abdul Rahim Al Attar Tower_Data", -66 },
            { "768450784.Damac residenze_Data", -1118 },
            { "780452440.Haven_Data", -8 },
            { "781950755.Vozrojdenie_Data", -5 },
            { "789974462.Moskovskiy 14 floor 2 sec_Data", -12 },
            { "792304093.Legenda_Data", -7 },
            { "793247873.Regatta_Data", -9 },
            { "798976213.Evropeyskiy_Data", -284 },
            { "801913109.Arcadia_Data", -2344 },
            { "803307441.Moskovskiy16fl2s_Data", -23 } // This should have been a straight cut item
        }; // end bonus households

        public static bool printEmploymentNames = false;
        public static bool mergeEmploymentNames = true;
        public static Dictionary<string, int> bonusWorkerCache = new Dictionary<string, int>();
        public static Dictionary<string, int> defaultBonusWorker = new Dictionary<string, int>()
        {
            { "H2 4x3 BigFactory08", 16 },
            { "419078725.Barry Plaza_Data", 122 },
            { "422472215.Lever Tower_Data", 21 },
            //{ "422434231.Bank of America_Data", 0 },
            { "426607732.Ares Tower_Data", -19264 },
            { "428001234.Trump Tower Chicago_Data", -8053 },
            { "428925673.Leviathan_Data", -962 },
            { "432230772.Antares Tower_Data", -3543 },
            { "434097849.Monolith_Data", -1033 },
            { "435267158.Twin Monoliths_Data", -2768 },
            { "439107462.Gula's Rivergate Lykes_Data", -23 },
            { "453154792.Gula's Kingdom Tower_Data", -21238 },  // 243886 sqm
            { "476346019.Bluerise Plaza_Data", -1067 },
            { "480111098.Messeturm (1:1)_Data", -470 },
            { "494346780.Drosovilas USBank tower_Data", -213 },
            { "509112645.Crystallization_Data", -155 },
            { "517277801.Central Plaza_Data", -3304 },
            { "527535273.snowjaoONEONE_Data", -5568 },
            { "531215484.Shanghai Tower, Shanghai (1:1)_Data", -18603 },
            { "534117358.Two Prudential Plaza_Data", -554 },
            { "534142273.TorreTrump_World_Tower_Data", 258 },
            { "536393392.383 Madison Avenue 1:1_Data", -1060 },
            { "537804449.Bank of America - 1:1_Data", 390 },
            { "568336474.Javelin_Data", -181 },
            { "572463914.Plaza Centenario_Data", -352 },
            { "579129058.Figueroa at Wilshire 1:1_Data", -1306 },
            { "591074989.The Tower_Data", -661 },
            { "604009267.One World Trade Center 1:1_Data", -2933 },  // 325279 sqm
            { "605000091.60 Wall Street_Data", -1535 },
            { "607981950.Chirpigroup Center_Data", -1788 },
            { "610913759.Kkorea Jongno Tower [종로타워]_Data", -800 },
            { "615095287.GITS Huge Office Building(ID#04)_Data", -6054 },
            { "621431766.Glory_Data", -2717 },
            { "622221778.Stalwart_Data", -148 },
            { "626845833.The Empire State Building_Data", -12215 },  // 208879 sqm
            { "628101617.Pan Pacific Insurance_Data", -2369 },
            { "635997321.1330 Post Oak Boulevard_Data", 325 },
            { "663727769.Plant Administration 01_Data", -259 },
            { "663729149.Plant Administration 02_Data", -123 },
            { "640403421.GITS Office Tower (ID#02c)_Data", -38 },
            { "647059381.GiTS Mega Office Bldg (ID#05)_Data", -19150 },
            { "651651763.Alliance_Data", -311 },
            { "652765729.Zed68-Mint 3x4L3Off_Data", 58 },
            { "660411166.Elstree Building_Data", -2736 },
            { "660864619.Policy_Data", 42 },
            { "661075324.Santander Tower_Data", -661 },
            { "661823191.Whirligig XL_Data", -2450 },
            { "664835898.US Bank Plaza_Data", -356 },
            { "670944160.Big Office Building_Data", -2403 },
            { "672448363.Foster Tower Madrid (Red)_Data", -640 },
            { "672525995.Foster Tower Madrid (White)_Data", -640 },
            { "676602837.PWC Tower Madrid_Data", -640 },
            { "678159892.Crystal Tower Madrid_Data", -993 },
            { "680500415.Space Tower Madrid_Data", -704 },
            { "687777086.Mirage_Data", -2207 },
            { "700553372.One Canada Square_Data", -3142 },
            { "700549442.8 Canada Square_Data", -2673 },
            { "708476856.Al habtoor business tower_Data", -92 },
            { "708992454.Eclipse_Data", -2067 },
            { "717997754.Miyagi Motors front_Data", -156 },
            { "725222700.Willis Tower_Data", 9308 },  // 416000 sqm
            { "730926381.The Bay Gate large_Data", 82 },
            //{ "748418268.Reaver_Data", 0 },
            { "749227212.Abeno Harukas_Data", -10700 },
            { "751581536.Research Facility_Data", -32 },
            { "762728624.Goldin Finance 117_Data", -4429 },
            { "766038657.Sant'Elia Tower_Data", -3133 },
            { "766038657.San'tElia Tower_Data", -3133 },
            //{ "768372480.Audacity_Data", 0 },
            { "777048160.Sunshine 60 RICO_Data", -7058 },
            { "777050916.Mustard Inc_Data", -256 },
            { "778876495.The Aon Center_Data", -3351 },
            { "779406148.McGinnis Tower_Data", -1623 },
            { "782848688.Distribution Center Long_Data", -75 },
            { "785268004.FIB Los Sanots HQ _Data", -2512 },
            { "786448278.Cancer Research Center_Data", -156 },
            { "792727102.Quadrillion_Data", -15920 },
            { "794601473.Woodlog Architects_Data", -312 },
            { "796614604.Perth Council Building_Data", -251 },
            //{ "798838680.Commerce Court Complex_Data", 0 },
            { "798838680.Commerce Court Complex_Data", -256 },
            { "799186937.Perth CitiBank House_Data", -12 },
            { "799801877.100 St Georges Terrace_Data", -962 },
            { "800259881.700 De La Gauchetiere_Data", -2769 },
            { "800259881.National Bank Tower_Data", -1005 },
            { "800260798.Place Ville Marie_Data", -5427 },
            { "804749788.Event Hall_Data", -69 },
            { "810925693.Cosine Building_Data", -255 }
        }; // end bonus workers

        // Prefab stores
        public static Dictionary<int, int> prefabHouseHolds = new Dictionary<int, int>(512);
        public static Dictionary<int, prefabEmployStruct> prefabWorkerVisit = new Dictionary<int, prefabEmployStruct>(1024);
        public static Dictionary<ulong, ushort> seedToId = new Dictionary<ulong, ushort>();

        public static void clearCache()
        {
            bonusWorkerCache.Clear();
            bonusHouseholdCache.Clear();
            prefabHouseHolds.Clear();
            prefabWorkerVisit.Clear();
            seedToId.Clear();

            printResidentialNames = false;
            printEmploymentNames = false;
            mergeResidentialNames = true;
            mergeEmploymentNames = true;

            strictCapacity = true;
            allowRemovalOfCitizens = false;
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
