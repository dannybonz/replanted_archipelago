using Archipelago.MultiClient.Net.Enums;
using Il2CppReloaded.Data;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ReplantedArchipelago.Patches.Store;

namespace ReplantedArchipelago
{
    public class Data
    {
        //Version to match with generation
        public static string GenVersion = "1.6";
        //Whether cheat keys are enabled
        public static bool CheatKeys = false;
        public static bool SkipAwardScreen = false;

        public static System.Random random = new System.Random();

        //UI templates
        public static GameObject buttonTemplate;
        public static GameObject panelTemplate;
        public static GameObject errorTemplate;
        public static GameObject clientTemplate;
        public static GameObject logTemplate;
        public static GameObject inputTemplate;
        public static GameObject subheaderTemplate;
        public static GameObject headerTemplate;

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

        public static double[] gameEffectItems = { 50, 51, 52, 53, 54, 55, 56, 64, 70, 71, 72 };

        public static Dictionary<long, string> itemIdDefaultTooltips = new Dictionary<long, string>
        {
            { 2, "KEYS_DESCRIPTION" },
            { 4, "SHOVEL_DESCRIPTION" },
            { 5, "SUBURBAN_ALMANAC_DESCRIPTION" },
            { 6, "WATERING_CAN_DESCRIPTION" },
            { 60, "ADVICE_CLICKED_ON_COIN" },
            { 61, "ADVICE_CLICKED_ON_COIN" },
            { 62, "ADVICE_CLICKED_ON_COIN" }
        };

        public static Dictionary<long, string> itemIdCustomDescriptions = new Dictionary<long, string>
        {
            { 3, "Lets you carry an extra plant" },
            { 7, "You can now play Mini-games" },
            { 8, "You can now play Puzzle mode" },
            { 9, "You can now play Survival mode" },
            { 10, "You can now play Cloudy Day" },
            { 11, "You can now play Bonus Levels" },
            { 12, "Adds an extra line of defense for roof levels" },
            { 13, "Adds an extra line of defense on levels with a pool" },
            { 15, "More items are available in Crazy Dave's shop" },
            { 18, "Lets you start with more sun" },
            { 19, "Increases the reward received from lawn mowers" },
            { 20, "You can now play Adventure: Day" },
            { 21, "You can now play Adventure: Night" },
            { 22, "You can now play Adventure: Pool" },
            { 23, "You can now play Adventure: Fog" },
            { 24, "You can now play Adventure: Roof" },
            { 25, "You can now play China: The Great Wall" },
            { 27, "Required to unlock the final level" },
            { 30, "A useful Zen Garden item" },
            { 31, "A useful Zen Garden item" },
            { 32, "A useful Zen Garden item" },
            { 33, "A useful Zen Garden item" },
            { 34, "A useful Zen Garden item" },
            { 63, "It won't help against the zombies, but everyone loves bacon!" },
            { 64, "Drops a random seed packet" },
            { 65, "Used to grow your Tree of Wisdom" },
            { 66, "Used to grow Zen Garden plants" },
            { 67, "Used to keep Zen Garden plants happy" },
            { 68, "Fed to Stinky or your Zen Garden plants" },
            { 70, "Instantly deploys your lawn mowers" },
            { 71, "Puts all seeds on cooldown" },
            { 72, "Summons an ambush of zombies" },
        };

        public static Dictionary<long, CoinType> awardCoinTypes = new Dictionary<long, CoinType>
        {
            { 2, CoinType.CarKeys },
            { 4, CoinType.Shovel },
            { 5, CoinType.Almanac },
            { 6, CoinType.WateringCan }
        };

        public static Dictionary<long, int> itemIdToPurchaseId = new Dictionary<long, int>
        {
            { 12, (int)StoreItem.RoofCleaner },
            { 13, (int)StoreItem.PoolCleaner },
            { 16, (int)StoreItem.Firstaid },
            { 30, (int)StoreItem.GardeningGlove },
            { 31, (int)StoreItem.GoldWateringcan },
            { 32, (int)StoreItem.Phonograph },
            { 33, (int)StoreItem.StinkyTheSnail },
            { 34, (int)StoreItem.WheelBarrow },
            { 148, (int)StoreItem.PlantImitater }
        };

        public static Dictionary<long, int> itemIdToConsumablePurchaseId = new Dictionary<long, int>
        {
            { 68, (int)StoreItem.Chocolate },
            { 66, (int)StoreItem.Fertilizer },
            { 67, (int)StoreItem.BugSpray },
            { 65, (int)StoreItem.TreeFood }
        };

        //Common Game Data
        public static SeedType[] seedTypes = { SeedType.Peashooter, SeedType.Sunflower, SeedType.Cherrybomb, SeedType.Wallnut, SeedType.Potatomine, SeedType.Snowpea, SeedType.Chomper, SeedType.Repeater, SeedType.Puffshroom, SeedType.Sunshroom, SeedType.Fumeshroom, SeedType.Gravebuster, SeedType.Hypnoshroom, SeedType.Scaredyshroom, SeedType.Iceshroom, SeedType.Doomshroom, SeedType.Lilypad, SeedType.Squash, SeedType.Threepeater, SeedType.Tanglekelp, SeedType.Jalapeno, SeedType.Spikeweed, SeedType.Torchwood, SeedType.Tallnut, SeedType.Seashroom, SeedType.Plantern, SeedType.Cactus, SeedType.Blover, SeedType.Splitpea, SeedType.Starfruit, SeedType.Pumpkinshell, SeedType.Magnetshroom, SeedType.Cabbagepult, SeedType.Flowerpot, SeedType.Kernelpult, SeedType.InstantCoffee, SeedType.Garlic, SeedType.Umbrella, SeedType.Marigold, SeedType.Melonpult, SeedType.Gatlingpea, SeedType.Twinsunflower, SeedType.Gloomshroom, SeedType.Cattail, SeedType.Wintermelon, SeedType.GoldMagnet, SeedType.Spikerock, SeedType.Cobcannon, SeedType.Imitater };
        public static MusicTune[] musicTunes = { MusicTune.DayGrasswalk, MusicTune.MinigameLoonboon, MusicTune.Conveyer, MusicTune.NightMoongrains, MusicTune.PoolWaterygraves, MusicTune.FogRigormormist, MusicTune.RoofGrazetheroof, MusicTune.FinalBossBrainiacManiac, MusicTune.PuzzleCerebrawl };
        public static Dictionary<string, int[]> levelOrders = new Dictionary<string, int[]>();
        public static Dictionary<string, LevelEntryData[]> orderedLevelEntries = new Dictionary<string, LevelEntryData[]>();
        public static string[] plantNames = { "Peashooter", "Sunflower", "Cherry Bomb", "Wall-nut", "Potato Mine", "Snow Pea", "Chomper", "Repeater", "Puff-shroom", "Sun-shroom", "Fume-shroom", "Grave Buster", "Hypno-shroom", "Scaredy-shroom", "Ice-shroom", "Doom-shroom", "Lily Pad", "Squash", "Threepeater", "Tangle Kelp", "Jalapeno", "Spikeweed", "Torchwood", "Tall-nut", "Sea-shroom", "Plantern", "Cactus", "Blover", "Split Pea", "Starfruit", "Pumpkin", "Magnet-shroom", "Cabbage-pult", "Flower Pot", "Kernel-pult", "Coffee Bean", "Garlic", "Umbrella Leaf", "Marigold", "Melon-pult", "Gatling Pea", "Twin Sunflower", "Gloom-shroom", "Cattail", "Winter Melon", "Gold Magnet", "Spikerock", "Cob Cannon", "Imitater" };

        //Plant stats
        public class PlantStats
        {
            public int Cost { get; set; }
            public int Refresh { get; set; }
            public int Rate { get; set; }
            public int Health { get; set; }
            public List<string> Projectiles { get; set; }
            public int EasyUpgradeCost { get; set; }
            public string StatsString { get; set; }
            public string ConveyorStatsString { get; set; }
            public PlantStats OldStats { get; private set; }
            public void BackupStats() //Save old stats to restore during I, Zombie etc.
            {
                if (OldStats == null)
                {
                    OldStats = new PlantStats();
                    OldStats.Cost = this.Cost;
                    OldStats.Refresh = this.Refresh;
                    OldStats.Rate = this.Rate;
                    OldStats.Health = this.Health;
                    if (Projectiles != null)
                    {
                        OldStats.Projectiles = new List<string>(Projectiles);
                    }
                    OldStats.EasyUpgradeCost = this.EasyUpgradeCost;
                }
            }
        }

        public static Dictionary<SeedType, PlantStats> plantStats = new Dictionary<SeedType, PlantStats>
        {
            [SeedType.Peashooter] = new PlantStats { Cost = 100, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Pea" } },
            [SeedType.Sunflower] = new PlantStats { Cost = 50, Refresh = 750, Rate = 2500, Health = 300 },
            [SeedType.Cherrybomb] = new PlantStats { Cost = 150, Refresh = 5000, Health = 300 },
            [SeedType.Wallnut] = new PlantStats { Cost = 50, Refresh = 3000, Health = 4000 },
            [SeedType.Potatomine] = new PlantStats { Cost = 25, Refresh = 3000, Health = 300 },
            [SeedType.Snowpea] = new PlantStats { Cost = 175, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Frozen Pea" } },
            [SeedType.Chomper] = new PlantStats { Cost = 150, Refresh = 750, Health = 300 },
            [SeedType.Repeater] = new PlantStats { Cost = 200, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Pea" } },
            [SeedType.Puffshroom] = new PlantStats { Cost = 0, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Spore" } },
            [SeedType.Sunshroom] = new PlantStats { Cost = 25, Refresh = 750, Rate = 2500, Health = 300 },
            [SeedType.Fumeshroom] = new PlantStats { Cost = 75, Refresh = 750, Rate = 150, Health = 300 },
            [SeedType.Gravebuster] = new PlantStats { Cost = 75, Refresh = 750, Health = 300 },
            [SeedType.Hypnoshroom] = new PlantStats { Cost = 75, Refresh = 3000, Health = 300 },
            [SeedType.Scaredyshroom] = new PlantStats { Cost = 25, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Spore" } },
            [SeedType.Iceshroom] = new PlantStats { Cost = 75, Refresh = 5000, Health = 300 },
            [SeedType.Doomshroom] = new PlantStats { Cost = 125, Refresh = 5000, Health = 300 },
            [SeedType.Lilypad] = new PlantStats { Cost = 25, Refresh = 750, Health = 300 },
            [SeedType.Squash] = new PlantStats { Cost = 50, Refresh = 3000, Health = 300 },
            [SeedType.Threepeater] = new PlantStats { Cost = 325, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Pea" } },
            [SeedType.Tanglekelp] = new PlantStats { Cost = 25, Refresh = 3000, Health = 300 },
            [SeedType.Jalapeno] = new PlantStats { Cost = 125, Refresh = 5000, Health = 300 },
            [SeedType.Spikeweed] = new PlantStats { Cost = 100, Refresh = 750, Health = 300 },
            [SeedType.Torchwood] = new PlantStats { Cost = 175, Refresh = 750, Health = 300 },
            [SeedType.Tallnut] = new PlantStats { Cost = 125, Refresh = 3000, Health = 8000 },
            [SeedType.Seashroom] = new PlantStats { Cost = 0, Refresh = 3000, Rate = 150, Health = 300, Projectiles = new List<string> { "Spore" } },
            [SeedType.Plantern] = new PlantStats { Cost = 25, Refresh = 3000, Health = 300 },
            [SeedType.Cactus] = new PlantStats { Cost = 125, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Spike" } },
            [SeedType.Blover] = new PlantStats { Cost = 100, Refresh = 750, Health = 300 },
            [SeedType.Splitpea] = new PlantStats { Cost = 125, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Pea" } },
            [SeedType.Starfruit] = new PlantStats { Cost = 125, Refresh = 750, Rate = 150, Health = 300, Projectiles = new List<string> { "Star" } },
            [SeedType.Pumpkinshell] = new PlantStats { Cost = 125, Refresh = 3000, Health = 4000 },
            [SeedType.Magnetshroom] = new PlantStats { Cost = 100, Refresh = 750, Health = 300 },
            [SeedType.Cabbagepult] = new PlantStats { Cost = 100, Refresh = 750, Rate = 300, Health = 300, Projectiles = new List<string> { "Cabbage" } },
            [SeedType.Flowerpot] = new PlantStats { Cost = 25, Refresh = 750, Health = 300 },
            [SeedType.Kernelpult] = new PlantStats { Cost = 100, Refresh = 750, Rate = 300, Health = 300, Projectiles = new List<string> { "Kernel", "Butter" } },
            [SeedType.InstantCoffee] = new PlantStats { Cost = 75, Refresh = 750, Health = 300 },
            [SeedType.Garlic] = new PlantStats { Cost = 50, Refresh = 750, Health = 400 },
            [SeedType.Umbrella] = new PlantStats { Cost = 100, Refresh = 750, Health = 300 },
            [SeedType.Marigold] = new PlantStats { Cost = 50, Refresh = 3000, Rate = 2500, Health = 300 },
            [SeedType.Melonpult] = new PlantStats { Cost = 300, Refresh = 750, Rate = 300, Health = 300, Projectiles = new List<string> { "Melon" } },
            [SeedType.Gatlingpea] = new PlantStats { Cost = 250, Refresh = 5000, Rate = 150, Health = 300, Projectiles = new List<string> { "Pea" }, EasyUpgradeCost = 450 },
            [SeedType.Twinsunflower] = new PlantStats { Cost = 150, Refresh = 5000, Rate = 2500, Health = 300, EasyUpgradeCost = 200 },
            [SeedType.Gloomshroom] = new PlantStats { Cost = 150, Refresh = 5000, Rate = 200, Health = 300, EasyUpgradeCost = 225 },
            [SeedType.Cattail] = new PlantStats { Cost = 225, Refresh = 5000, Rate = 150, Health = 300, Projectiles = new List<string> { "Spike" }, EasyUpgradeCost = 250 },
            [SeedType.Wintermelon] = new PlantStats { Cost = 200, Refresh = 5000, Rate = 300, Health = 300, Projectiles = new List<string> { "Frozen Melon" }, EasyUpgradeCost = 500 },
            [SeedType.GoldMagnet] = new PlantStats { Cost = 50, Refresh = 5000, Health = 300, EasyUpgradeCost = 150 },
            [SeedType.Spikerock] = new PlantStats { Cost = 125, Refresh = 5000, Health = 450, EasyUpgradeCost = 225 },
            [SeedType.Cobcannon] = new PlantStats { Cost = 500, Refresh = 5000, Rate = 600, Health = 300, EasyUpgradeCost = 700 }
        };

        public static SeedType[] upgradePlants = { SeedType.Gatlingpea, SeedType.Twinsunflower, SeedType.Gloomshroom, SeedType.Cattail, SeedType.Wintermelon, SeedType.GoldMagnet, SeedType.Spikerock, SeedType.Cobcannon };

        //Projectiles
        public static ProjectileType[] projectileTypes = { ProjectileType.Pea, ProjectileType.Snowpea, ProjectileType.Cabbage, ProjectileType.Melon, ProjectileType.Puff, ProjectileType.Wintermelon, ProjectileType.Star, ProjectileType.Spike, ProjectileType.Kernel, ProjectileType.Butter, ProjectileType.Fireball };

        public static Dictionary<string, ProjectileType> projectileNamesToTypes = new Dictionary<string, ProjectileType>
        {
            ["Pea"] = ProjectileType.Pea,
            ["Frozen Pea"] = ProjectileType.Snowpea,
            ["Cabbage"] = ProjectileType.Cabbage,
            ["Melon"] = ProjectileType.Melon,
            ["Spore"] = ProjectileType.Puff,
            ["Frozen Melon"] = ProjectileType.Wintermelon,
            ["Star"] = ProjectileType.Star,
            ["Spike"] = ProjectileType.Spike,
            ["Kernel"] = ProjectileType.Kernel,
            ["Butter"] = ProjectileType.Butter
        };

        public static Dictionary<ProjectileType, int> defaultProjectileDamages = new Dictionary<ProjectileType, int>
        {
            [ProjectileType.Pea] = 20,
            [ProjectileType.Snowpea] = 20,
            [ProjectileType.Cabbage] = 40,
            [ProjectileType.Melon] = 80,
            [ProjectileType.Puff] = 20,
            [ProjectileType.Wintermelon] = 80,
            [ProjectileType.Star] = 20,
            [ProjectileType.Spike] = 20,
            [ProjectileType.Kernel] = 20,
            [ProjectileType.Butter] = 40,
            [ProjectileType.Fireball] = 40
        };

        public static string FormatPlantStatChanges(string label, double oldValue, double newValue, bool upIsGood)
        {
            double multiplier = newValue / oldValue;
            if (label == "Rate")
            {
                multiplier = 1 / multiplier; //Invert the firing rate multiplier for clarity
            }
            if (oldValue == newValue)
            {
                multiplier = 1;
                return "";
            }

            string textColor = "<color=black>◌ ";
            if (multiplier > 1)
            {
                if (upIsGood)
                {
                    textColor = "<color=#00B400>↑ ";
                }
                else
                {
                    textColor = "<color=#B40000>↑ ";
                }
            }
            else if (multiplier < 1)
            {
                if (upIsGood)
                {
                    textColor = "<color=#B40000>↓ ";
                }
                else
                {
                    textColor = "<color=#00B400>↓ ";
                }
            }

            return $"{textColor}{label} x{multiplier:F2}</color><br>";
        }

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

        public static ZombieType[] zombieTypes =
        {
            ZombieType.Normal,
            ZombieType.Flag,
            ZombieType.TrafficCone,
            ZombieType.Polevaulter,
            ZombieType.Pail,
            ZombieType.Newspaper,
            ZombieType.Door,
            ZombieType.Football,
            ZombieType.Dancer,
            ZombieType.BackupDancer,
            ZombieType.DuckyTube,
            ZombieType.Snorkel,
            ZombieType.Zamboni,
            ZombieType.Bobsled,
            ZombieType.DolphinRider,
            ZombieType.JackInTheBox,
            ZombieType.Balloon,
            ZombieType.Digger,
            ZombieType.Pogo,
            ZombieType.Yeti,
            ZombieType.Bungee,
            ZombieType.Ladder,
            ZombieType.Catapult,
            ZombieType.Gargantuar,
            ZombieType.Imp,
            ZombieType.Boss,
            ZombieType.PeaHead,
            ZombieType.WallnutHead,
            ZombieType.JalapenoHead,
            ZombieType.GatlingHead,
            ZombieType.SquashHead,
            ZombieType.TallnutHead,
            ZombieType.RedeyeGargantuar,
            ZombieType.Zombatar,
            ZombieType.Target,
            ZombieType.TrashCan
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
            { 120, new LevelLocationsEntry { Name = "Cloudy Day: Level 12", ClearLocation = 1119, FlagLocations = new int[] { 2158, 2159 } } },
            { 121, new LevelLocationsEntry { Name = "China: The Great Wall", ClearLocation = 1120, FlagLocations = new int[] { 2160 } } }
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

            { GameMode.BonusChina, 121 }
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
                "It's about as useful as a Wall-nut is against a Zomboni."
            }
        };

        public static Dictionary<string, string[]> DaveCrossoverTexts = new Dictionary<string, string[]>
        {
            ["K-On! After School Live!!"] = new[] {
                "Imported straight from Kyoto!",
                "Undead things are fun!"
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

        public static int GetLevelIdFromEntryData(LevelEntryData levelEntryData)
        {
            int levelId = -1;
            if (levelEntryData != null)
            {
                if (levelEntryData.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                {
                    levelId = 109 + levelEntryData.m_subIndex;
                }
                else if (levelEntryData.GameMode == GameMode.Adventure)
                {
                    levelId = levelEntryData.m_levelNumber;
                }
                else if (GameModeLevelIDs.ContainsKey(levelEntryData.GameMode))
                {
                    levelId = GameModeLevelIDs[levelEntryData.GameMode];
                }
            }
            return levelId;
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

        public static long EnergyLinkWithdrawMultiplier = 100000000; //per 10 coins
        public static long EnergyLinkDepositMultiplier = (long)(EnergyLinkWithdrawMultiplier * 0.75);
        public static string FormatEnergyString(long energyAmount)
        {
            if (energyAmount < 1000)
            {
                return $"{energyAmount} J";
            }
            else if (energyAmount < 1000000)
            {
                return $"{energyAmount / 1000.0:0.##} kJ";
            }
            else if (energyAmount < 1000000000)
            {
                return $"{energyAmount / 1000000.0:0.##} MJ";
            }
            else
            {
                return $"{energyAmount / 1000000000.0:0.##} GJ";
            }
        }

        public static SeedType GetFreeSeedType(Board board)
        {
            //Any level
            List<SeedType> freeSeedTypes = new List<SeedType> { SeedType.Cherrybomb, SeedType.Wallnut, SeedType.Potatomine, SeedType.Chomper, SeedType.Squash, SeedType.Jalapeno, SeedType.Tallnut, SeedType.Pumpkinshell, SeedType.Cabbagepult, SeedType.Kernelpult, SeedType.Garlic, SeedType.Marigold, SeedType.Melonpult };

            //Sun producers - not on conveyor levels
            if (!board.HasConveyorBeltSeedBank())
            {
                freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Sunflower }).ToList();
                if (APClient.easyUpgradePlants)
                {
                    freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Twinsunflower }).ToList();
                }
            }

            //Upgrade plants - any level
            if (APClient.easyUpgradePlants)
            {
                freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Wintermelon, SeedType.GoldMagnet, SeedType.Cobcannon }).ToList();
            }

            //Nocturnal
            if (board.mBackground == BackgroundType.Night || board.mBackground == BackgroundType.Fog)
            {
                freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Puffshroom, SeedType.Sunshroom, SeedType.Fumeshroom, SeedType.Hypnoshroom, SeedType.Scaredyshroom, SeedType.Iceshroom, SeedType.Doomshroom, SeedType.Magnetshroom }).ToList();
                if (APClient.easyUpgradePlants)
                {
                    freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Gloomshroom }).ToList();
                }
            }

            //Aquatic
            if (board.mBackground == BackgroundType.Pool || board.mBackground == BackgroundType.Fog)
            {
                freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Lilypad, SeedType.Tanglekelp }).ToList();
                if (board.mBackground == BackgroundType.Fog)
                {
                    freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Seashroom, SeedType.Plantern }).ToList();
                }
                if (APClient.easyUpgradePlants)
                {
                    freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Cattail }).ToList();
                }
            }

            //No Roof
            if (board.mBackground != BackgroundType.Roof)
            {
                freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Peashooter, SeedType.Snowpea, SeedType.Repeater, SeedType.Threepeater, SeedType.Torchwood, SeedType.Spikeweed, SeedType.Cactus, SeedType.Splitpea, SeedType.Starfruit }).ToList();
                if (APClient.easyUpgradePlants)
                {
                    freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Gatlingpea, SeedType.Spikerock }).ToList();
                }
            }
            else //Roof
            {
                freeSeedTypes = freeSeedTypes.Concat(new List<SeedType> { SeedType.Flowerpot }).ToList();
            }

            return freeSeedTypes[random.Next(freeSeedTypes.Count)];
        }

        public static CustomStoreEntry[] zenGardenStoreEntries = {
            new CustomStoreEntry { Name = "Fertilizer", Class = ItemFlags.None, Cost = 750 },
            new CustomStoreEntry { Name = "Bug Spray", Class = ItemFlags.None, Cost = 1000 },
            new CustomStoreEntry { Name = "Tree Food", Class = ItemFlags.None, Cost = 1000 }
        };

        public static Dictionary<int, string> zenGardenItemStrings = new Dictionary<int, string>
        {
            { 5000, "CRAZY_DAVE_2020" },
            { 5001, "CRAZY_DAVE_2022" },
            { 5002, "CRAZY_DAVE_2031" }
        };

        //AP Client item text colors
        public static Dictionary<ItemFlags, string> itemColors = new Dictionary<ItemFlags, string>()
        {
            { ItemFlags.Trap, "#FA8072" },
            { ItemFlags.Advancement, "#AF99EF" },
            { ItemFlags.NeverExclude, "#6D8BE8" },
            { ItemFlags.None, "#00BCBC" }
        };
    }
}
