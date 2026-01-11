using Archipelago.MultiClient.Net.Enums;
using Il2CppReloaded.Data;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReplantedArchipelago
{
    public class Data
    {
        //Version to match with generation
        public static double GenVersion = 1.2;
        //Whether cheat keys are enabled
        public static bool CheatKeys = false;

        //UI templates
        public static GameObject buttonTemplate;
        public static GameObject headerTemplate;
        public static GameObject panelTemplate;
        public static GameObject errorTemplate;
        public static GameObject clientTemplate;
        public static GameObject logTemplate;
        public static GameObject inputTemplate;

        //Classes
        public class LevelLocationsEntry
        {
            public string Name { get; set; }
            public int ClearLocation { get; set; }
            public int[] FlagLocations { get; set; }
        }

        public class QueuedIngameMessage
        {
            public string MessageLabel { get; set; }
            public double ItemId { get; set; }
            public bool WasReceived { get; set; }
        }

        //Item IDs
        public static long[] menuUpdateItems = new long[] { 2, 5, 6, 7, 8, 9, 10, 11, 20, 21, 22, 23, 24, 50, 51, 52, 53, 54, 55, 56 }.Concat(Enumerable.Range(200, 201).Select(i => (long)i)).ToArray(); //Item IDs that require a menu refresh

        public static Dictionary<string, long> itemIds = new Dictionary<string, long>
        {
            { "Crazy Dave's Car Keys", 2 },
            { "Extra Seed Slot", 3 },
            { "Shovel", 4 },
            { "Almanac", 5 },
            { "Zen Garden", 6 },
            { "Minigame Mode", 7 },
            { "Puzzle Mode", 8 },
            { "Survival Mode", 9 },
            { "Cloudy Day", 10 },
            { "Bonus Levels", 11 },
            { "Roof Cleaners", 12 },
            { "Pool Cleaners", 13 },
            { "Lawn Mowers", 14 },
            { "Twiddiedinkies Restock", 15 },
            { "Wall-nut First Aid", 16 },

            { "Day", 20 },
            { "Night", 21 },
            { "Pool", 22 },
            { "Fog", 23 },
            { "Roof", 24 },

            { "mustache", 50 },
            { "future", 51 },
            { "trickedout", 52 },
            { "daisies", 53 },
            { "pinata", 54 },
            { "sukhbir", 55 },
            { "dance", 56 },

            { "Mower Deploy Trap", 70 },
            { "Seed Packet Cooldown Trap", 71 },
            { "Zombie Ambush Trap", 72 },

            { "Imitater", 148 }
        };

        public static Dictionary<long, string> itemIdDefaultTooltips = new Dictionary<long, string>
        {
            { 2, "KEYS_DESCRIPTION" },
            { 4, "SHOVEL_DESCRIPTION" },
            { 5, "SUBURBAN_ALMANAC_DESCRIPTION" }
        };

        public static Dictionary<long, string> itemIdSpriteName = new Dictionary<long, string>
        {
            { 2, "SPR_CarKeys" },
            { 4, "SPR_ShovelHiRes" },
            { 5, "SPR_Almanac" },

            { 100, "SPR_Almanac_Seedpackets_PeaShooter"},
            { 101, "SPR_Almanac_Seedpackets_Sunflower"},
            { 102, "SPR_Almanac_Seedpackets_CherryBomb"},
            { 103, "SPR_Almanac_Seedpackets_Wall-Nut"},
            { 104, "SPR_Almanac_Seedpackets_PotatoMine"},
            { 105, "SPR_Almanac_Seedpackets_SnowPea"},
            { 106, "SPR_Almanac_Seedpackets_Chomper"},
            { 107, "SPR_Almanac_Seedpackets_Repeater"},
            { 108, "SPR_Almanac_Seedpackets_Puff-shroom"},
            { 109, "SPR_Almanac_Seedpackets_Sun-shroom"},
            { 110, "SPR_Almanac_Seedpackets_Fume-shroom"},
            { 111, "SPR_Almanac_Seedpackets_GraveBuster"},
            { 112, "SPR_Almanac_Seedpackets_Hypno-shroom"},
            { 113, "SPR_Almanac_Seedpackets_Scaredy-shroom"},
            { 114, "SPR_Almanac_Seedpackets_Ice-shroom"},
            { 115, "SPR_Almanac_Seedpackets_Doom-shroom"},
            { 116, "SPR_Almanac_Seedpackets_LilyPad"},
            { 117, "SPR_Almanac_Seedpackets_Squash"},
            { 118, "SPR_Almanac_Seedpackets_Threepeater"},
            { 119, "SPR_Almanac_Seedpackets_TangleKelp"},
            { 120, "SPR_Almanac_Seedpackets_Jalapeno"},
            { 121, "SPR_Almanac_Seedpackets_Spikeweed"},
            { 122, "SPR_Almanac_Seedpackets_Torchwood"},
            { 123, "SPR_Almanac_Seedpackets_Tall-nut"},
            { 124, "SPR_Almanac_Seedpackets_Sea-shroom"},
            { 125, "SPR_Almanac_Seedpackets_Plantern"},
            { 126, "SPR_Almanac_Seedpackets_Cactus"},
            { 127, "SPR_Almanac_Seedpackets_Blover"},
            { 128, "SPR_Almanac_Seedpackets_SplitPea"},
            { 129, "SPR_Almanac_Seedpackets_Starfruit"},
            { 130, "SPR_Almanac_Seedpackets_Pumpkin"},
            { 131, "SPR_Almanac_Seedpackets_Magnet-shroom"},
            { 132, "SPR_Almanac_Seedpackets_Cabbage-pult"},
            { 133, "SPR_Almanac_Seedpackets_FlowerPot"},
            { 134, "SPR_Almanac_Seedpackets_Kernel-pult"},
            { 135, "SPR_Almanac_Seedpackets_CoffeeBean"},
            { 136, "SPR_Almanac_Seedpackets_Garlic"},
            { 137, "SPR_Almanac_Seedpackets_UmbrellaLeaf"},
            { 138, "SPR_Almanac_Seedpackets_Marigold"},
            { 139, "SPR_Almanac_Seedpackets_Melon-pult"},
            { 140, "SPR_Almanac_Seedpackets_GatlingPea"},
            { 141, "SPR_Almanac_Seedpackets_TwinSunflower"},
            { 142, "SPR_Almanac_Seedpackets_Gloom-shroom"},
            { 143, "SPR_Almanac_Seedpackets_Cattail"},
            { 144, "SPR_Almanac_Seedpackets_WinterMelon"},
            { 145, "SPR_Almanac_Seedpackets_GoldMagnet"},
            { 146, "SPR_Almanac_Seedpackets_Spikerock"},
            { 147, "SPR_Almanac_Seedpackets_CobCannon"},
            { 148, "SPR_Almanac_Seedpackets_Imitater"}
        };

        public static Dictionary<long, CoinType> awardCoinTypes = new Dictionary<long, CoinType>
        {
            { 2, CoinType.CarKeys },
            { 4, CoinType.Shovel },
            { 5, CoinType.Almanac }
        };

        //Common Game Data
        public static SeedType[] seedTypes = { SeedType.Peashooter, SeedType.Sunflower, SeedType.Cherrybomb, SeedType.Wallnut, SeedType.Potatomine, SeedType.Snowpea, SeedType.Chomper, SeedType.Repeater, SeedType.Puffshroom, SeedType.Sunshroom, SeedType.Fumeshroom, SeedType.Gravebuster, SeedType.Hypnoshroom, SeedType.Scaredyshroom, SeedType.Iceshroom, SeedType.Doomshroom, SeedType.Lilypad, SeedType.Squash, SeedType.Threepeater, SeedType.Tanglekelp, SeedType.Jalapeno, SeedType.Spikeweed, SeedType.Torchwood, SeedType.Tallnut, SeedType.Seashroom, SeedType.Plantern, SeedType.Cactus, SeedType.Blover, SeedType.Splitpea, SeedType.Starfruit, SeedType.Pumpkinshell, SeedType.Magnetshroom, SeedType.Cabbagepult, SeedType.Flowerpot, SeedType.Kernelpult, SeedType.InstantCoffee, SeedType.Garlic, SeedType.Umbrella, SeedType.Marigold, SeedType.Melonpult, SeedType.Gatlingpea, SeedType.Twinsunflower, SeedType.Gloomshroom, SeedType.Cattail, SeedType.Wintermelon, SeedType.GoldMagnet, SeedType.Spikerock, SeedType.Cobcannon, SeedType.Imitater };
        public static MusicTune[] musicTunes = { MusicTune.DayGrasswalk, MusicTune.MinigameLoonboon, MusicTune.Conveyer, MusicTune.NightMoongrains, MusicTune.PoolWaterygraves, MusicTune.FogRigormormist, MusicTune.RoofGrazetheroof, MusicTune.FinalBossBrainiacManiac, MusicTune.PuzzleCerebrawl };
        public static Dictionary<string, int[]> levelOrders = new Dictionary<string, int[]>();
        public static Dictionary<string, LevelEntryData[]> orderedLevelEntries = new Dictionary<string, LevelEntryData[]>();

        public static System.Collections.Generic.Dictionary<ZombieType, string> zombieTypeNames = new System.Collections.Generic.Dictionary<ZombieType, string>
        {
            { ZombieType.BackupDancer, "Backup Dancer Zombie" },
            { ZombieType.Balloon, "Balloon Zombie" },
            { ZombieType.Bobsled, "Zombie Bobsled Team" },
            { ZombieType.Boss, "Dr. Zomboss" },
            { ZombieType.Bungee, "Bungee Zombie" },
            { ZombieType.Catapult, "Catapult Zombie" },
            { ZombieType.Dancer, "Dancing Zombie" },
            { ZombieType.Digger, "Digger Zombie" },
            { ZombieType.DolphinRider, "Dolphin Rider Zombie" },
            { ZombieType.Door, "Screen Door Zombie Zombie" },
            { ZombieType.DuckyTube, "Ducky Tube Zombie" },
            { ZombieType.Flag, "Flag Zombie" },
            { ZombieType.Football, "Football Zombie" },
            { ZombieType.Gargantuar, "Gargantuar" },
            { ZombieType.GatlingHead, "Gatling Pea Zombie" },
            { ZombieType.Imp, "Imp" },
            { ZombieType.JackInTheBox, "Jack-in-the-Box Zombie" },
            { ZombieType.JalapenoHead, "Jalapeno Zombie" },
            { ZombieType.Ladder, "Ladder Zombie" },
            { ZombieType.Newspaper, "Newspaper Zombie" },
            { ZombieType.Normal, "Zombie" },
            { ZombieType.Pail, "Buckethead Zombie" },
            { ZombieType.PeaHead, "Peashooter Zombie" },
            { ZombieType.Pogo, "Pogo Zombie" },
            { ZombieType.Polevaulter, "Pole Vaulting Zombie" },
            { ZombieType.RedeyeGargantuar, "Giga Gargantuar" },
            { ZombieType.Snorkel, "Snorkel Zombie" },
            { ZombieType.SquashHead, "Squash Zombie" },
            { ZombieType.TallnutHead, "Tall-nut Zombie" },
            { ZombieType.TrafficCone, "Conehead Zombie" },
            { ZombieType.TrashCan, "Trash Can Zombie" },
            { ZombieType.WallnutHead, "Wall-nut Zombie" },
            { ZombieType.Yeti, "Yeti Zombie" },
            { ZombieType.Zamboni, "Zomboni" },
        };


        //Location IDs
        public static readonly Dictionary<int, LevelLocationsEntry> AllLevelLocations = new Dictionary<int, LevelLocationsEntry>
        {
            { 1, new LevelLocationsEntry { Name = "Day: Level 1-1", ClearLocation = 1000, FlagLocations = new int[0] } },
            { 2, new LevelLocationsEntry { Name = "Day: Level 1-2", ClearLocation = 1001, FlagLocations = new int[0] } },
            { 3, new LevelLocationsEntry { Name = "Day: Level 1-3", ClearLocation = 1002, FlagLocations = new int[0] } },
            { 4, new LevelLocationsEntry { Name = "Day: Level 1-4", ClearLocation = 1003, FlagLocations = new int[0] } },
            { 5, new LevelLocationsEntry { Name = "Day: Level 1-5", ClearLocation = 1004, FlagLocations = new int[0] } },
            { 6, new LevelLocationsEntry { Name = "Day: Level 1-6", ClearLocation = 1005, FlagLocations = new int[0] } },
            { 7, new LevelLocationsEntry { Name = "Day: Level 1-7", ClearLocation = 1006, FlagLocations = new int[] { 2000 } } },
            { 8, new LevelLocationsEntry { Name = "Day: Level 1-8", ClearLocation = 1007, FlagLocations = new int[0] } },
            { 9, new LevelLocationsEntry { Name = "Day: Level 1-9", ClearLocation = 1008, FlagLocations = new int[] { 2001 } } },
            { 10, new LevelLocationsEntry { Name = "Day: Level 1-10", ClearLocation = 1009, FlagLocations = new int[] { 2002 } } },
            { 11, new LevelLocationsEntry { Name = "Night: Level 2-1", ClearLocation = 1010, FlagLocations = new int[0] } },
            { 12, new LevelLocationsEntry { Name = "Night: Level 2-2", ClearLocation = 1011, FlagLocations = new int[] { 2003 } } },
            { 13, new LevelLocationsEntry { Name = "Night: Level 2-3", ClearLocation = 1012, FlagLocations = new int[0] } },
            { 14, new LevelLocationsEntry { Name = "Night: Level 2-4", ClearLocation = 1013, FlagLocations = new int[] { 2004 } } },
            { 15, new LevelLocationsEntry { Name = "Night: Level 2-5", ClearLocation = 1014, FlagLocations = new int[0] } },
            { 16, new LevelLocationsEntry { Name = "Night: Level 2-6", ClearLocation = 1015, FlagLocations = new int[0] } },
            { 17, new LevelLocationsEntry { Name = "Night: Level 2-7", ClearLocation = 1016, FlagLocations = new int[] { 2005 } } },
            { 18, new LevelLocationsEntry { Name = "Night: Level 2-8", ClearLocation = 1017, FlagLocations = new int[0] } },
            { 19, new LevelLocationsEntry { Name = "Night: Level 2-9", ClearLocation = 1018, FlagLocations = new int[] { 2006 } } },
            { 20, new LevelLocationsEntry { Name = "Night: Level 2-10", ClearLocation = 1019, FlagLocations = new int[] { 2007 } } },
            { 21, new LevelLocationsEntry { Name = "Pool: Level 3-1", ClearLocation = 1020, FlagLocations = new int[0] } },
            { 22, new LevelLocationsEntry { Name = "Pool: Level 3-2", ClearLocation = 1021, FlagLocations = new int[] { 2008 } } },
            { 23, new LevelLocationsEntry { Name = "Pool: Level 3-3", ClearLocation = 1022, FlagLocations = new int[] { 2009 } } },
            { 24, new LevelLocationsEntry { Name = "Pool: Level 3-4", ClearLocation = 1023, FlagLocations = new int[] { 2010, 2011 } } },
            { 25, new LevelLocationsEntry { Name = "Pool: Level 3-5", ClearLocation = 1024, FlagLocations = new int[] { 2012 } } },
            { 26, new LevelLocationsEntry { Name = "Pool: Level 3-6", ClearLocation = 1025, FlagLocations = new int[] { 2013 } } },
            { 27, new LevelLocationsEntry { Name = "Pool: Level 3-7", ClearLocation = 1026, FlagLocations = new int[] { 2014, 2015 } } },
            { 28, new LevelLocationsEntry { Name = "Pool: Level 3-8", ClearLocation = 1027, FlagLocations = new int[] { 2016 } } },
            { 29, new LevelLocationsEntry { Name = "Pool: Level 3-9", ClearLocation = 1028, FlagLocations = new int[] { 2017, 2018 } } },
            { 30, new LevelLocationsEntry { Name = "Pool: Level 3-10", ClearLocation = 1029, FlagLocations = new int[] { 2019, 2020 } } },
            { 31, new LevelLocationsEntry { Name = "Fog: Level 4-1", ClearLocation = 1030, FlagLocations = new int[0] } },
            { 32, new LevelLocationsEntry { Name = "Fog: Level 4-2", ClearLocation = 1031, FlagLocations = new int[] { 2021 } } },
            { 33, new LevelLocationsEntry { Name = "Fog: Level 4-3", ClearLocation = 1032, FlagLocations = new int[0] } },
            { 34, new LevelLocationsEntry { Name = "Fog: Level 4-4", ClearLocation = 1033, FlagLocations = new int[] { 2022 } } },
            { 35, new LevelLocationsEntry { Name = "Fog: Level 4-5", ClearLocation = 1034, FlagLocations = new int[0] } },
            { 36, new LevelLocationsEntry { Name = "Fog: Level 4-6", ClearLocation = 1035, FlagLocations = new int[0] } },
            { 37, new LevelLocationsEntry { Name = "Fog: Level 4-7", ClearLocation = 1036, FlagLocations = new int[] { 2023 } } },
            { 38, new LevelLocationsEntry { Name = "Fog: Level 4-8", ClearLocation = 1037, FlagLocations = new int[0] } },
            { 39, new LevelLocationsEntry { Name = "Fog: Level 4-9", ClearLocation = 1038, FlagLocations = new int[] { 2024 } } },
            { 40, new LevelLocationsEntry { Name = "Fog: Level 4-10", ClearLocation = 1039, FlagLocations = new int[] { 2025 } } },
            { 41, new LevelLocationsEntry { Name = "Roof: Level 5-1", ClearLocation = 1040, FlagLocations = new int[0] } },
            { 42, new LevelLocationsEntry { Name = "Roof: Level 5-2", ClearLocation = 1041, FlagLocations = new int[] { 2026 } } },
            { 43, new LevelLocationsEntry { Name = "Roof: Level 5-3", ClearLocation = 1042, FlagLocations = new int[] { 2027 } } },
            { 44, new LevelLocationsEntry { Name = "Roof: Level 5-4", ClearLocation = 1043, FlagLocations = new int[] { 2028, 2029 } } },
            { 45, new LevelLocationsEntry { Name = "Roof: Level 5-5", ClearLocation = 1044, FlagLocations = new int[] { 2030 } } },
            { 46, new LevelLocationsEntry { Name = "Roof: Level 5-6", ClearLocation = 1045, FlagLocations = new int[] { 2031 } } },
            { 47, new LevelLocationsEntry { Name = "Roof: Level 5-7", ClearLocation = 1046, FlagLocations = new int[] { 2032, 2033 } } },
            { 48, new LevelLocationsEntry { Name = "Roof: Level 5-8", ClearLocation = 1047, FlagLocations = new int[] { 2034 } } },
            { 49, new LevelLocationsEntry { Name = "Roof: Level 5-9", ClearLocation = 1048, FlagLocations = new int[] { 2035, 2036 } } },
            { 50, new LevelLocationsEntry { Name = "Roof: Dr. Zomboss", ClearLocation = 1049, FlagLocations = new int[0] } },
            { 51, new LevelLocationsEntry { Name = "Mini-games: ZomBotany", ClearLocation = 1050, FlagLocations = new int[] { 2037 } } },
            { 52, new LevelLocationsEntry { Name = "Mini-games: Wall-nut Bowling", ClearLocation = 1051, FlagLocations = new int[] { 2038 } } },
            { 53, new LevelLocationsEntry { Name = "Mini-games: Slot Machine", ClearLocation = 1052, FlagLocations = new int[0] } },
            { 54, new LevelLocationsEntry { Name = "Mini-games: It's Raining Seeds", ClearLocation = 1053, FlagLocations = new int[] { 2039, 2040, 2041 } } },
            { 55, new LevelLocationsEntry { Name = "Mini-games: Beghouled", ClearLocation = 1054, FlagLocations = new int[0] } },
            { 56, new LevelLocationsEntry { Name = "Mini-games: Invisi-ghoul", ClearLocation = 1055, FlagLocations = new int[] { 2042 } } },
            { 57, new LevelLocationsEntry { Name = "Mini-games: Seeing Stars", ClearLocation = 1056, FlagLocations = new int[0] } },
            { 58, new LevelLocationsEntry { Name = "Mini-games: Zombiquarium", ClearLocation = 1057, FlagLocations = new int[0] } },
            { 59, new LevelLocationsEntry { Name = "Mini-games: Beghouled Twist", ClearLocation = 1058, FlagLocations = new int[0] } },
            { 60, new LevelLocationsEntry { Name = "Mini-games: Big Trouble Little Zombie", ClearLocation = 1059, FlagLocations = new int[] { 2043, 2044 } } },
            { 61, new LevelLocationsEntry { Name = "Mini-games: Portal Combat", ClearLocation = 1060, FlagLocations = new int[] { 2045 } } },
            { 62, new LevelLocationsEntry { Name = "Mini-games: Column Like You See 'Em", ClearLocation = 1061, FlagLocations = new int[] { 2046, 2047 } } },
            { 63, new LevelLocationsEntry { Name = "Mini-games: Bobsled Bonanza", ClearLocation = 1062, FlagLocations = new int[] { 2048, 2049, 2050 } } },
            { 64, new LevelLocationsEntry { Name = "Mini-games: Zombie Nimble Zombie Quick", ClearLocation = 1063, FlagLocations = new int[] { 2051, 2052, 2053 } } },
            { 65, new LevelLocationsEntry { Name = "Mini-games: Whack a Zombie", ClearLocation = 1064, FlagLocations = new int[0] } },
            { 66, new LevelLocationsEntry { Name = "Mini-games: Last Stand", ClearLocation = 1065, FlagLocations = new int[] { 2054, 2055, 2056, 2057 } } },
            { 67, new LevelLocationsEntry { Name = "Mini-games: ZomBotany 2", ClearLocation = 1066, FlagLocations = new int[] { 2058, 2059 } } },
            { 68, new LevelLocationsEntry { Name = "Mini-games: Wall-nut Bowling 2", ClearLocation = 1067, FlagLocations = new int[] { 2060, 2061 } } },
            { 69, new LevelLocationsEntry { Name = "Mini-games: Pogo Party", ClearLocation = 1068, FlagLocations = new int[] { 2062, 2063 } } },
            { 70, new LevelLocationsEntry { Name = "Mini-games: Dr. Zomboss's Revenge", ClearLocation = 1069, FlagLocations = new int[0] } },
            { 71, new LevelLocationsEntry { Name = "Puzzle: Vasebreaker", ClearLocation = 1070, FlagLocations = new int[0] } },
            { 72, new LevelLocationsEntry { Name = "Puzzle: To The Left", ClearLocation = 1071, FlagLocations = new int[0] } },
            { 73, new LevelLocationsEntry { Name = "Puzzle: Third Vase", ClearLocation = 1072, FlagLocations = new int[0] } },
            { 74, new LevelLocationsEntry { Name = "Puzzle: Chain Reaction", ClearLocation = 1073, FlagLocations = new int[0] } },
            { 75, new LevelLocationsEntry { Name = "Puzzle: M is for Metal", ClearLocation = 1074, FlagLocations = new int[0] } },
            { 76, new LevelLocationsEntry { Name = "Puzzle: Scary Potter", ClearLocation = 1075, FlagLocations = new int[0] } },
            { 77, new LevelLocationsEntry { Name = "Puzzle: Hokey Pokey", ClearLocation = 1076, FlagLocations = new int[0] } },
            { 78, new LevelLocationsEntry { Name = "Puzzle: Another Chain Reaction", ClearLocation = 1077, FlagLocations = new int[0] } },
            { 79, new LevelLocationsEntry { Name = "Puzzle: Ace of Vase", ClearLocation = 1078, FlagLocations = new int[0] } },
            { 80, new LevelLocationsEntry { Name = "Puzzle: I, Zombie", ClearLocation = 1079, FlagLocations = new int[0] } },
            { 81, new LevelLocationsEntry { Name = "Puzzle: I, Zombie Too", ClearLocation = 1080, FlagLocations = new int[0] } },
            { 82, new LevelLocationsEntry { Name = "Puzzle: Can You Dig It?", ClearLocation = 1081, FlagLocations = new int[0] } },
            { 83, new LevelLocationsEntry { Name = "Puzzle: Totally Nuts", ClearLocation = 1082, FlagLocations = new int[0] } },
            { 84, new LevelLocationsEntry { Name = "Puzzle: Dead Zeppelin", ClearLocation = 1083, FlagLocations = new int[0] } },
            { 85, new LevelLocationsEntry { Name = "Puzzle: Me Smash!", ClearLocation = 1084, FlagLocations = new int[0] } },
            { 86, new LevelLocationsEntry { Name = "Puzzle: ZomBoogie", ClearLocation = 1085, FlagLocations = new int[0] } },
            { 87, new LevelLocationsEntry { Name = "Puzzle: Three Hit Wonder", ClearLocation = 1086, FlagLocations = new int[0] } },
            { 88, new LevelLocationsEntry { Name = "Puzzle: All your brainz r belong to us", ClearLocation = 1087, FlagLocations = new int[0] } },
            { 89, new LevelLocationsEntry { Name = "Survival: Day", ClearLocation = 1088, FlagLocations = new int[] { 2064, 2065, 2066, 2067 } } },
            { 90, new LevelLocationsEntry { Name = "Survival: Night", ClearLocation = 1089, FlagLocations = new int[] { 2068, 2069, 2070, 2071 } } },
            { 91, new LevelLocationsEntry { Name = "Survival: Pool", ClearLocation = 1090, FlagLocations = new int[] { 2072, 2073, 2074, 2075 } } },
            { 92, new LevelLocationsEntry { Name = "Survival: Fog", ClearLocation = 1091, FlagLocations = new int[] { 2076, 2077, 2078, 2079 } } },
            { 93, new LevelLocationsEntry { Name = "Survival: Roof", ClearLocation = 1092, FlagLocations = new int[] { 2080, 2081, 2082, 2083 } } },
            { 94, new LevelLocationsEntry { Name = "Survival: Day (Hard)", ClearLocation = 1093, FlagLocations = new int[] { 2084, 2085, 2086, 2087, 2088, 2089, 2090, 2091, 2092 } } },
            { 95, new LevelLocationsEntry { Name = "Survival: Night (Hard)", ClearLocation = 1094, FlagLocations = new int[] { 2093, 2094, 2095, 2096, 2097, 2098, 2099, 2100, 2101 } } },
            { 96, new LevelLocationsEntry { Name = "Survival: Pool (Hard)", ClearLocation = 1095, FlagLocations = new int[] { 2102, 2103, 2104, 2105, 2106, 2107, 2108, 2109, 2110 } } },
            { 97, new LevelLocationsEntry { Name = "Survival: Fog (Hard)", ClearLocation = 1096, FlagLocations = new int[] { 2111, 2112, 2113, 2114, 2115, 2116, 2117, 2118, 2119 } } },
            { 98, new LevelLocationsEntry { Name = "Survival: Roof (Hard)", ClearLocation = 1097, FlagLocations = new int[] { 2120, 2121, 2122, 2123, 2124, 2125, 2126, 2127, 2128 } } },
            { 99, new LevelLocationsEntry { Name = "Bonus Levels: Art Challenge Wall-nut", ClearLocation = 1098, FlagLocations = new int[0] } },
            { 100, new LevelLocationsEntry { Name = "Bonus Levels: Sunny Day", ClearLocation = 1099, FlagLocations = new int[] { 2129, 2130, 2131 } } },
            { 101, new LevelLocationsEntry { Name = "Bonus Levels: Unsodded", ClearLocation = 1100, FlagLocations = new int[] { 2132, 2133, 2134 } } },
            { 102, new LevelLocationsEntry { Name = "Bonus Levels: Big Time", ClearLocation = 1101, FlagLocations = new int[] { 2135, 2136, 2137 } } },
            { 103, new LevelLocationsEntry { Name = "Bonus Levels: Art Challenge Sunflower", ClearLocation = 1102, FlagLocations = new int[0] } },
            { 104, new LevelLocationsEntry { Name = "Bonus Levels: Air Raid", ClearLocation = 1103, FlagLocations = new int[] { 2138 } } },
            { 105, new LevelLocationsEntry { Name = "Bonus Levels: High Gravity", ClearLocation = 1104, FlagLocations = new int[] { 2139 } } },
            { 106, new LevelLocationsEntry { Name = "Bonus Levels: Grave Danger", ClearLocation = 1105, FlagLocations = new int[] { 2140 } } },
            { 107, new LevelLocationsEntry { Name = "Bonus Levels: Can You Dig It?", ClearLocation = 1106, FlagLocations = new int[] { 2141, 2142 } } },
            { 108, new LevelLocationsEntry { Name = "Bonus Levels: Dark Stormy Night", ClearLocation = 1107, FlagLocations = new int[] { 2143, 2144 } } },
            { 109, new LevelLocationsEntry { Name = "Cloudy Day: Level 1", ClearLocation = 1108, FlagLocations = new int[0] } },
            { 110, new LevelLocationsEntry { Name = "Cloudy Day: Level 2", ClearLocation = 1109, FlagLocations = new int[] { 2145 } } },
            { 111, new LevelLocationsEntry { Name = "Cloudy Day: Level 3", ClearLocation = 1110, FlagLocations = new int[0] } },
            { 112, new LevelLocationsEntry { Name = "Cloudy Day: Level 4", ClearLocation = 1111, FlagLocations = new int[] { 2146, 2147 } } },
            { 113, new LevelLocationsEntry { Name = "Cloudy Day: Level 5", ClearLocation = 1112, FlagLocations = new int[] { 2148 } } },
            { 114, new LevelLocationsEntry { Name = "Cloudy Day: Level 6", ClearLocation = 1113, FlagLocations = new int[] { 2149, 2150 } } },
            { 115, new LevelLocationsEntry { Name = "Cloudy Day: Level 7", ClearLocation = 1114, FlagLocations = new int[] { 2151 } } },
            { 116, new LevelLocationsEntry { Name = "Cloudy Day: Level 8", ClearLocation = 1115, FlagLocations = new int[] { 2152, 2153 } } },
            { 117, new LevelLocationsEntry { Name = "Cloudy Day: Level 9", ClearLocation = 1116, FlagLocations = new int[] { 2154 } } },
            { 118, new LevelLocationsEntry { Name = "Cloudy Day: Level 10", ClearLocation = 1117, FlagLocations = new int[] { 2155, 2156 } } },
            { 119, new LevelLocationsEntry { Name = "Cloudy Day: Level 11", ClearLocation = 1118, FlagLocations = new int[] { 2157 } } },
            { 120, new LevelLocationsEntry { Name = "Cloudy Day: Level 12", ClearLocation = 1119, FlagLocations = new int[] { 2158, 2159 } } }
        };

        //Matches GameMode to Level Locations dict
        public static readonly Dictionary<GameMode, int> GameModeLevelIDs = new Dictionary<GameMode, int>
        {
            { GameMode.ChallengeWarAndPeas, 51 },
            { GameMode.ChallengeWallnutBowling, 52 },
            { GameMode.ChallengeSlotMachine, 53 },
            { GameMode.ChallengeRainingSeeds, 54 },
            { GameMode.ChallengeBeghouled, 55 },
            { GameMode.ChallengeInvisighoul, 56 },
            { GameMode.ChallengeSeeingStars, 57 },
            { GameMode.ChallengeZombiquarium, 58 },
            { GameMode.ChallengeBeghouledTwist, 59 },
            { GameMode.ChallengeLittleTrouble, 60 },
            { GameMode.ChallengePortalCombat, 61 },
            { GameMode.ChallengeColumn, 62 },
            { GameMode.ChallengeBobsledBonanza, 63 },
            { GameMode.ChallengeSpeed, 64 },
            { GameMode.ChallengeWhackAZombie, 65 },
            { GameMode.ChallengeLastStand, 66 },
            { GameMode.ChallengeWarAndPeas2, 67 },
            { GameMode.ChallengeWallnutBowling2, 68 },
            { GameMode.ChallengePogoParty, 69 },
            { GameMode.ChallengeFinalBoss, 70 },

            { GameMode.ScaryPotter1, 71 },
            { GameMode.ScaryPotter2, 72 },
            { GameMode.ScaryPotter3, 73 },
            { GameMode.ScaryPotter4, 74 },
            { GameMode.ScaryPotter5, 75 },
            { GameMode.ScaryPotter6, 76 },
            { GameMode.ScaryPotter7, 77 },
            { GameMode.ScaryPotter8, 78 },
            { GameMode.ScaryPotter9, 79 },
            { GameMode.PuzzleIZombie1, 80 },
            { GameMode.PuzzleIZombie2, 81 },
            { GameMode.PuzzleIZombie3, 82 },
            { GameMode.PuzzleIZombie4, 83 },
            { GameMode.PuzzleIZombie5, 84 },
            { GameMode.PuzzleIZombie6, 85 },
            { GameMode.PuzzleIZombie7, 86 },
            { GameMode.PuzzleIZombie8, 87 },
            { GameMode.PuzzleIZombie9, 88 },

            { GameMode.SurvivalNormalStage1, 89 },
            { GameMode.SurvivalNormalStage2, 90 },
            { GameMode.SurvivalNormalStage3, 91 },
            { GameMode.SurvivalNormalStage4, 92 },
            { GameMode.SurvivalNormalStage5, 93 },
            { GameMode.SurvivalHardStage1, 94 },
            { GameMode.SurvivalHardStage2, 95 },
            { GameMode.SurvivalHardStage3, 96 },
            { GameMode.SurvivalHardStage4, 97 },
            { GameMode.SurvivalHardStage5, 98 },

            { GameMode.ChallengeArtChallenge1, 99 },
            { GameMode.ChallengeSunnyDay, 100 },
            { GameMode.ChallengeResodded, 101 },
            { GameMode.ChallengeBigTime, 102 },
            { GameMode.ChallengeArtChallenge2, 103 },
            { GameMode.ChallengeAirRaid, 104 },
            { GameMode.ChallengeHighGravity, 105 },
            { GameMode.ChallengeGraveDanger, 106 },
            { GameMode.ChallengeShovel, 107 },
            { GameMode.ChallengeStormyNight, 108 },
            { GameMode.ChallengeBungeeBlitz, 109 },
        };

        //Base costs for store items
        public static Dictionary<ItemFlags, int> BaseCosts = new Dictionary<ItemFlags, int>
        {
            { ItemFlags.Advancement, 900 },
            { ItemFlags.NeverExclude, 500 },
            { ItemFlags.None, 100 },
            { ItemFlags.Trap, 50 },
        };

        //Crazy Dave shop flavour texts
        public static Dictionary<ItemFlags, string[]> DaveFlavourTexts = new Dictionary<ItemFlags, string[]>
        {
            [ItemFlags.Advancement] = new[] { //Progression items
                "It's CRAZY important!",
                "You'd have to be CRAZY to pass this one up!",
                "I think of this store as quite a progressive place!",
                "It's as essential to progressing as hot sauce is to making a good taco."
            },

            [ItemFlags.NeverExclude] = new[] { //Useful items
                "It's darn handy. Handier than a staple gun.",
                "At a price like that, how can you say no?",
                "Buy this and you'll be A-O-GOOD.",
                "To some, this is worthless. To others, it's worth the arbitrary price I decided upon."
            },

            [ItemFlags.Trap] = new[] { //Trap items
                "Something smells fishy and I don't think it's just my lunch.",
                "I think a zombie put this here.",
                "No refunds for damage caused by cross-multiworld delivery!",
                "I wouldn't touch it without gloves on."
            },

            [ItemFlags.None] = new[] { //Filler items
                "It probably won't amount to much, but that's what they said about me!",
                "It tastes like dirt! It's around the same price, too.",
                "It's called filler, but I still feel hungry!",
                "It's about as useful as a Wall-nut is against a Zomboni.",
            }
        };

        public static int GetLevelIdFromGameplayActivity(GameplayActivity gameplayActivity)
        {
            int levelId = -1;
            try
            {
                if (gameplayActivity.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                {
                    levelId = 109 + gameplayActivity.m_levelData.m_subIndex;
                }
                else if (gameplayActivity.GameMode == GameMode.Adventure)
                {
                    levelId = gameplayActivity.m_levelData.m_levelNumber;
                }
                else if (GameModeLevelIDs.ContainsKey(gameplayActivity.GameMode))
                {
                    levelId = GameModeLevelIDs[gameplayActivity.GameMode];
                }
            }
            catch
            {
                return -1;
            }
            return levelId;
        }

        public static Sprite FindSpriteByName(string spriteName)
        {
            foreach (var sprite in Resources.FindObjectsOfTypeAll<Sprite>())
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }
            return null;
        }

        public static string GetTransitionNameFromLevelId(int levelId)
        {
            string result = "LevelSelect";
            if (levelId == 50)
            {
                result = "Credits";
            }
            if (levelId >= 71 && levelId <= 88)
            {
                result = "Puzzle";
            }
            else if (levelId >= 51 && levelId <= 70)
            {
                result = "MiniGames";
            }
            else if (levelId >= 89 && levelId <= 98)
            {
                result = "Survival";
            }
            return result;
        }
    }
}