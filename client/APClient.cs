using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Il2Cpp;
using Il2CppReloaded.Data;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.TreeStateActivities;
using Il2CppSource.Utils;
using Newtonsoft.Json.Linq;
using ReplantedArchipelago.Patches;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ReplantedArchipelago
{
    public class APClient
    {
        public static bool currentlyConnected = false;
        public static string slot;
        public static string host;
        public static string password;

        public static ArchipelagoSession apSession;
        public static List<long> receivedItems = new List<long>();
        public static List<string> receivedMessages = new List<string>();
        public static Dictionary<long, ScoutedItemInfo> scoutedLocations;
        public static int displayedIngameMessages = 0;
        public static string genVersion;
        public static DeathLinkService deathLinkService;

        public static Dictionary<string, object> slotData;
        public static JArray musicMap;
        public static JArray shopPrices;
        public static JObject minigameUnlocks;
        public static JObject survivalUnlocks;
        public static JObject vasebreakerUnlocks;
        public static JObject izombieUnlocks;
        public static JObject cloudyDayUnlocks;
        public static bool areaLockItems;
        public static bool openAreaItems;
        public static bool imitaterOpen;
        public static bool easyUpgradePlants;
        public static bool disableStormFlashes;
        public static int adventureLevelsGoal;
        public static int adventureAreasGoal;
        public static int minigameLevelsGoal;
        public static int puzzleLevelsGoal;
        public static int survivalLevelsGoal;
        public static int cloudyDayLevelsGoal;
        public static int bonusLevelsGoal;
        public static int overallLevelsGoal;
        public static bool deathlinkEnabled;
        public static bool fastGoal;
        public static int adventureProgression;
        public static int minigameLevels;
        public static int puzzleLevels;
        public static int survivalLevels;
        public static int cloudyDayLevels;
        public static int bonusLevels;
        public static JObject zombieMap;

        public static bool hugeWaveChecks;

        public static int shopPages;
        public static int shopPagesVisible = 1;
        public static List<int> clearedLevels = new List<int>();
        public static string chooserRefreshState = "none";
        public static int queuedUpCoins = 0;
        public static List<int> queuedUpPurchases = new List<int>();
        public static DeathLink receivedDeathLink = null;

        public static void AttemptConnection(string hostInput, string slotInput, string passwordInput)
        {
            apSession = ArchipelagoSessionFactory.CreateSession(hostInput);
            LoginResult result;

            apSession.MessageLog.OnMessageReceived += HandleMessage;
            apSession.Socket.SocketClosed += HandleDisconnect;
            apSession.Socket.ErrorReceived += HandleError;

            try
            {
                result = apSession.TryConnectAndLogin("Plants vs. Zombies: Replanted", slotInput, ItemsHandlingFlags.AllItems, password: passwordInput, requestSlotData: true);
            }
            catch (Exception e)
            {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful) //Connection failed
            {
                Menu.ShowErrorPanel("Connection Failed", "Ensure your connection information is correct and try again.");
            }
            else
            {
                var loginSuccess = (LoginSuccessful)result;
                slotData = loginSuccess.SlotData;

                genVersion = ((double)slotData["gen_version"]).ToString(CultureInfo.InvariantCulture);

                if (genVersion != Data.GenVersion) //Version mismatch
                {
                    Menu.ShowErrorPanel("Version Mismatch", $"You are using version {Data.GenVersion} to connect to a world generated with version {APClient.genVersion}.");
                }
                else
                {
                    //Reset received items
                    receivedItems = new List<long>();
                    queuedUpCoins = 0;

                    //Update server values
                    slot = apSession.Players.ActivePlayer.Name;
                    host = hostInput;
                    password = passwordInput;

                    //Slot data
                    musicMap = (JArray)slotData["music_map"];
                    shopPrices = (JArray)slotData["shop_prices"];
                    minigameUnlocks = (JObject)slotData["minigame_unlocks"];
                    survivalUnlocks = (JObject)slotData["survival_unlocks"];
                    vasebreakerUnlocks = (JObject)slotData["vasebreaker_unlocks"];
                    izombieUnlocks = (JObject)slotData["izombie_unlocks"];
                    cloudyDayUnlocks = (JObject)slotData["cloudy_day_unlocks"];
                    areaLockItems = (Convert.ToInt32(slotData["adventure_mode_progression"]) >= 1);
                    openAreaItems = (Convert.ToInt32(slotData["adventure_mode_progression"]) == 2);
                    imitaterOpen = Convert.ToBoolean(slotData["imitater_open"]);
                    easyUpgradePlants = Convert.ToBoolean(slotData["easy_upgrade_plants"]);
                    disableStormFlashes = Convert.ToBoolean(slotData["disable_storm_flashes"]);
                    adventureAreasGoal = Convert.ToInt32(slotData["adventure_areas_goal"]);
                    adventureLevelsGoal = Convert.ToInt32(slotData["adventure_levels_goal"]);
                    minigameLevelsGoal = Convert.ToInt32(slotData["minigame_levels_goal"]);
                    puzzleLevelsGoal = Convert.ToInt32(slotData["puzzle_levels_goal"]);
                    survivalLevelsGoal = Convert.ToInt32(slotData["survival_levels_goal"]);
                    deathlinkEnabled = Convert.ToBoolean(slotData["deathlink_enabled"]);
                    fastGoal = Convert.ToBoolean(slotData["fast_goal"]);
                    cloudyDayLevelsGoal = Convert.ToInt32(slotData["cloudy_day_levels_goal"]);
                    bonusLevelsGoal = Convert.ToInt32(slotData["bonus_levels_goal"]);
                    overallLevelsGoal = Convert.ToInt32(slotData["overall_levels_goal"]);
                    zombieMap = (JObject)slotData["zombie_map"];

                    adventureProgression = Convert.ToInt32(slotData["adventure_mode_progression"]);
                    minigameLevels = Convert.ToInt32(slotData["minigame_levels"]);
                    puzzleLevels = Convert.ToInt32(slotData["puzzle_levels"]);
                    survivalLevels = Convert.ToInt32(slotData["survival_levels"]);
                    cloudyDayLevels = Convert.ToInt32(slotData["cloudy_day_levels"]);
                    bonusLevels = Convert.ToInt32(slotData["bonus_levels"]);

                    //Scout locations and store each level's reward
                    long[] clearLocationIds = Enumerable.Range(1000, 120).Select(i => (long)i).ToArray();
                    long[] flagLocationIds = Enumerable.Range(2000, 160).Select(i => (long)i).ToArray();
                    long[] shopLocationIds = Enumerable.Range(5000, shopPrices.Count).Select(i => (long)i).ToArray();
                    long[] locationsArray = clearLocationIds
                        .Concat(flagLocationIds)
                        .Concat(shopLocationIds)
                        .ToArray();
                    scoutedLocations = apSession.Locations.ScoutLocationsAsync(false, locationsArray).Result;

                    shopPages = (int)Math.Ceiling((double)shopPrices.Count / 8);
                    Store.UpdateCustomEntries(); //Update shop entries

                    int startingInvCount = Convert.ToInt32(slotData["starting_inv_count"]); //Don't send messages for starting inventory items
                    apSession.DataStorage[Scope.Slot, "displayedIngameMessages"].Initialize(startingInvCount); //Store the amount of displayed messages in DataStorage so we don't re-show the same messages
                    apSession.DataStorage[Scope.Slot, "profileGuids"].Initialize(JArray.FromObject(new List<string>())); //Used to validate profile
                    apSession.DataStorage[Scope.Slot, "clearedLevels"].Initialize(JArray.FromObject(new List<int>()));
                    clearedLevels = apSession.DataStorage[Scope.Slot, "clearedLevels"];

                    Data.levelOrders = new Dictionary<string, int[]>();

                    Data.levelOrders["minigames"] = GetOrderedLevelIDs(minigameUnlocks, 20, 51);
                    Data.levelOrders["survival"] = GetOrderedLevelIDs(survivalUnlocks, 10, 89);
                    Data.levelOrders["puzzle"] = GetOrderedLevelIDs(vasebreakerUnlocks, 9, 71).Concat(GetOrderedLevelIDs(izombieUnlocks, 9, 80)).ToArray();
                    Data.levelOrders["cloudy"] = GetOrderedLevelIDs(cloudyDayUnlocks, 12, 109);

                    if (deathlinkEnabled)
                    {
                        Main.Log("Deathlink enabled.");
                        deathLinkService = apSession.CreateDeathLinkService();
                        deathLinkService.OnDeathLinkReceived += HandleDeathLink;
                        deathLinkService.EnableDeathLink();
                    }

                    currentlyConnected = true; //Connection successful!

                    Profile.DoProfileCheck();

                    if (currentlyConnected) //If didn't disconnect due to lack of profile slots...
                    {
                        ProcessAllItems(); //Process all items received since the beginning of time
                        apSession.Items.ItemReceived += RunOnItemReceived; //Set handler for any items received in this session
                        Menu.HideConnectionPanel();

                        Profile.ProcessIUserService();

                        if (Main.cachedLevelDataModel != null)
                        {
                            Main.cachedLevelDataModel.UpdateModelData();
                        }
                    }
                }
            }
        }

        public static int[] GetOrderedLevelIDs(JObject unlockRequirements, int levelCount, int startingLevelId) //Returns a list of Level IDs sorted in the order they will be unlocked
        {
            int[] levelOrder = new int[levelCount];
            List<int> alreadyUnlocked = new List<int>();

            for (int i = startingLevelId; i < startingLevelId + levelCount; i++)
            {
                if ((int)unlockRequirements[i.ToString()] == 0)
                {
                    alreadyUnlocked.Add(i);
                }
            }
            for (int i = startingLevelId; i < startingLevelId + levelCount; i++)
            {
                if ((int)unlockRequirements[i.ToString()] > 0)
                {
                    levelOrder[(int)unlockRequirements[i.ToString()] + alreadyUnlocked.Count() - 1] = i;
                }
            }
            for (int i = 0; i < alreadyUnlocked.Count(); i++)
            {
                levelOrder[i] = alreadyUnlocked[i];
            }

            return levelOrder;
        }

        public static void ProcessAllItems()
        {
            clearedLevels = apSession.DataStorage[Scope.Slot, "clearedLevels"];

            displayedIngameMessages = apSession.DataStorage[Scope.Slot, "displayedIngameMessages"];

            while (apSession.Items.PeekItem() != null)
            {
                ProcessItemInfo(apSession.Items.DequeueItem());
            }

            apSession.DataStorage[Scope.Slot, "displayedIngameMessages"] = displayedIngameMessages;
        }

        public static void ProcessItemInfo(ItemInfo item)
        {
            if (displayedIngameMessages <= receivedItems.Count)
            {
                if (!item.Player.Name.Equals(slot))
                {
                    string messageLabel = $"Received {item.ItemDisplayName} from {item.Player.Name}";
                    Main.QueuedIngameMessages.Enqueue(new Data.QueuedIngameMessage { MessageLabel = messageLabel, ItemId = item.ItemId, WasReceived = true });
                }

                //Add coins
                if (item.ItemId == 60)
                {
                    queuedUpCoins += 1;
                }
                else if (item.ItemId == 61)
                {
                    queuedUpCoins += 5;
                }
                else if (item.ItemId == 62)
                {
                    queuedUpCoins += 100;
                }

                //Increase counter for displayed messages (to prevent messages being displayed multiple times)
                displayedIngameMessages++;
            }

            receivedItems.Add(item.ItemId); //Add item to inventory

            if (item.ItemId == Data.itemIds["Twiddiedinkies Restock"])
            {
                shopPagesVisible++;
                if (shopPagesVisible > shopPages) //More Twiddydinkies Restocks than there are pages of items (should only occur if using start inventory or admin commands)
                {
                    shopPagesVisible = shopPages;
                }
            }
            else if (item.ItemId == Data.itemIds["Wall-nut First Aid"])
            {
                queuedUpPurchases.Add((int)StoreItem.Firstaid);
            }
            else if (item.ItemId == Data.itemIds["Pool Cleaners"])
            {
                queuedUpPurchases.Add((int)StoreItem.PoolCleaner);
            }
            else if (item.ItemId == Data.itemIds["Roof Cleaners"])
            {
                queuedUpPurchases.Add((int)StoreItem.RoofCleaner);
            }
            else if (item.ItemId >= 100 && item.ItemId < 200)
            {
                chooserRefreshState = "update"; //Used to force a Seed Chooser rebuild for any plants obtained after a level has already begun
            }

            if (item.ItemId > 205 && item.ItemId < 250)
            {
                Profile.focusedLevelId = (int)item.ItemId - 200;
            }
        }

        public static void RunOnItemReceived(ReceivedItemsHelper itemHandler)
        {
            ProcessItemInfo(itemHandler.DequeueItem());
            apSession.DataStorage[Scope.Slot, "displayedIngameMessages"] = displayedIngameMessages;
        }

        public static void HandleMessage(LogMessage message)
        {
            receivedMessages.Add(message.ToString());
        }

        public static void HandleDisconnect(string reason)
        {
            Main.Log("Disconnected.");
            currentlyConnected = false;
            if (Main.currentScene != "Frontend")
            {
                StateTransitionUtils.Transition("Frontend");
            }
        }

        public static void HandleDeathLink(DeathLink deathLink)
        {
            Main.Log("Deathlink received.");
            if (Main.currentScene == "Gameplay" && Main.cachedGameplayActivity != null && Main.cachedGameplayActivity.GameScene == GameScenes.Playing)
            {
                receivedDeathLink = deathLink;
            }
            else
            {
                Main.Log("Deathlink dodged!");
            }
        }

        public static void HandleError(Exception e, string reason)
        {
            Main.Log("Connection error.");
            currentlyConnected = false;
            if (Main.currentScene != "Frontend")
            {
                StateTransitionUtils.Transition("Frontend");
            }
        }

        public static void SendLocation(int locationId, bool queueMessage)
        {
            if (!apSession.Locations.AllLocationsChecked.Contains(locationId)) //Don't waste time re-sending already sent checks
            {
                if (scoutedLocations != null && scoutedLocations.ContainsKey(locationId)) //If the location has been scouted, queue an ingame message
                {
                    if (queueMessage)
                    {
                        if (scoutedLocations[locationId].Player.Name.Equals(slot) == false)
                        {
                            Main.QueuedIngameMessages.Enqueue(new Data.QueuedIngameMessage { MessageLabel = $"Sent {scoutedLocations[locationId].ItemName} to {scoutedLocations[locationId].Player.Name}", ItemId = -1, WasReceived = false });
                        }
                        else
                        {
                            Main.QueuedIngameMessages.Enqueue(new Data.QueuedIngameMessage { MessageLabel = $"You found your {scoutedLocations[locationId].ItemName}", ItemId = scoutedLocations[locationId].ItemId, WasReceived = true });
                        }
                    }
                }
                apSession.Locations.CompleteLocationChecks(locationId);
            }
        }

        public static void SendWaveLocation(LevelEntryData levelData, int hugeWaveNumber)
        {
            if (levelData.ReloadedGameMode == ReloadedGameMode.CloudyDay)
            {
                if (Data.AllLevelLocations[109 + levelData.m_subIndex].FlagLocations.Length > hugeWaveNumber)
                {
                    SendLocation(Data.AllLevelLocations[109 + levelData.m_subIndex].FlagLocations[hugeWaveNumber], true);
                }
            }
            else if (levelData.GameMode == GameMode.Adventure)
            {
                if (Data.AllLevelLocations[levelData.m_levelNumber].FlagLocations.Length > hugeWaveNumber)
                {
                    SendLocation(Data.AllLevelLocations[levelData.m_levelNumber].FlagLocations[hugeWaveNumber], true);
                }
            }
            else if (Data.GameModeLevelIDs.ContainsKey(levelData.GameMode))
            {
                if (Data.AllLevelLocations[Data.GameModeLevelIDs[levelData.GameMode]].FlagLocations.Length > hugeWaveNumber && hugeWaveNumber >= 0)
                {
                    SendLocation(Data.AllLevelLocations[Data.GameModeLevelIDs[levelData.GameMode]].FlagLocations[hugeWaveNumber], true);
                }
            }
        }

        public static bool HasSeedType(SeedType theSeedType)
        {
            if (currentlyConnected && receivedItems != null && receivedItems.Contains(100 + Array.IndexOf(Data.seedTypes, theSeedType)))
            {
                return true;
            }
            else if (Main.cachedGameplayActivity != null && Main.cachedGameplayActivity.GameMode == GameMode.ChallengeRainingSeeds)
            {
                return true;
            }

            return false;
        }

        public static bool HasShovel()
        {
            if (currentlyConnected && receivedItems != null && receivedItems.Contains(Data.itemIds["Shovel"]))
            {
                return true;
            }

            return false;
        }

        public static int GetSeedSlots(long[] extraPlants, long[] bannedPlants)
        {
            int numberOfSlots = receivedItems.Count(item => item == Data.itemIds["Extra Seed Slot"]) + 1;
            int numberOfPlants = GetHowManyPlants(extraPlants, bannedPlants);

            return Math.Min(numberOfPlants, numberOfSlots);
        }

        public static bool HasBoss()
        {
            if (!fastGoal)
            {
                if (areaLockItems && openAreaItems)
                {
                    if (!receivedItems.Contains(24)) //No Roof Access
                    {
                        return false;
                    }
                }
                else
                {
                    if (!clearedLevels.Contains(49)) //If you haven't beaten 5-9
                    {
                        return false;
                    }
                }
            }

            return clearedLevels.Count >= overallLevelsGoal && GetClearedAreaCount() >= adventureAreasGoal && clearedLevels.Count(levelId => levelId < 51) >= adventureLevelsGoal && clearedLevels.Count(levelId => levelId >= 51 && levelId < 71) >= minigameLevelsGoal && (clearedLevels.Count(levelId => levelId >= 71 && levelId < 89)) >= puzzleLevelsGoal && clearedLevels.Count(levelId => levelId >= 89 && levelId < 99) >= survivalLevelsGoal && clearedLevels.Count(levelId => levelId >= 109 && levelId < 121) >= cloudyDayLevelsGoal && clearedLevels.Count(levelId => levelId >= 99 && levelId < 109) >= bonusLevelsGoal;
        }

        public static int GetClearedAreaCount()
        {
            return Enumerable.Range(1, 5).Count(area => Enumerable.Range((area - 1) * 10 + 1, area == 5 ? 9 : 10).All(level => clearedLevels.Contains(level)));
        }

        public static int GetAreaUnlockItemId(int levelNumber)
        {
            return 20 + (levelNumber - 1) / 10;
        }

        public static bool CanPlayLevel(int levelId)
        {
            if (levelId == 50) //Dr. Zomboss
            {
                return HasBoss();
            }
            else if (levelId < 50) //Adventure Mode
            {
                if (adventureProgression == 3)
                {
                    return receivedItems.Contains(200 + levelId);
                }
                else if (areaLockItems)
                {
                    if (receivedItems.Contains(GetAreaUnlockItemId(levelId)))
                    {
                        if (openAreaItems)
                        {
                            return true;
                        }
                        else
                        {
                            int[] startingLevel = { 1, 11, 21, 31, 41 };
                            if (startingLevel.Contains(levelId) || clearedLevels.Contains(levelId - 1))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (levelId == 1 || clearedLevels.Contains(levelId - 1))
                {
                    return true;
                }
            }
            else if (levelId < 71) //Mini-games
            {
                if (minigameLevels == 4)
                {
                    return receivedItems.Contains(levelId + 200);
                }
                else
                {
                    return (clearedLevels.Count(clearedLevelId => clearedLevelId >= 51 && clearedLevelId < 71) >= (int)minigameUnlocks[levelId.ToString()]);
                }
            }
            else if (levelId < 80) //Vasebreaker
            {
                if (puzzleLevels == 4)
                {
                    return receivedItems.Contains(levelId + 200);
                }
                else
                {
                    return (clearedLevels.Count(clearedLevelId => clearedLevelId >= 71 && clearedLevelId < 80) >= (int)vasebreakerUnlocks[levelId.ToString()]);
                }
            }
            else if (levelId < 89) //I, Zombie
            {
                if (puzzleLevels == 4)
                {
                    return receivedItems.Contains(levelId + 200);
                }
                else
                {
                    return (clearedLevels.Count(clearedLevelId => clearedLevelId >= 80 && clearedLevelId < 89) >= (int)izombieUnlocks[levelId.ToString()]);
                }
            }
            else if (levelId < 99) //Survival
            {
                if (survivalLevels == 4)
                {
                    return receivedItems.Contains(levelId + 200);
                }
                else
                {
                    return (clearedLevels.Count(clearedLevelId => clearedLevelId >= 89 && clearedLevelId < 99) >= (int)survivalUnlocks[levelId.ToString()]);
                }
            }
            else if (levelId < 109) //Bonus Levels
            {
                if (bonusLevels == 2)
                {
                    return receivedItems.Contains(levelId + 200);
                }
                else
                {
                    return receivedItems.Contains(11);
                }
            }
            else if (levelId < 121) //Cloudy Day
            {
                if (cloudyDayLevels == 4)
                {
                    return receivedItems.Contains(levelId + 200);
                }
                else
                {
                    return (clearedLevels.Count(clearedLevelId => clearedLevelId >= 109 && clearedLevelId < 121) >= (int)cloudyDayUnlocks[levelId.ToString()]);
                }
            }
            return false; //Level not playable
        }

        public static int GetHowManyPlants(long[] extraPlants, long[] bannedPlants)
        {
            var availablePlants = receivedItems.Where(item => item >= 100 && item < 200).Union(extraPlants).Except(bannedPlants).Distinct().Count();
            return availablePlants;
        }

        public static ItemFlags GetPrimaryItemClassification(ItemFlags Flags)
        {
            if (Flags.HasFlag(ItemFlags.Advancement))
            {
                return ItemFlags.Advancement;
            }
            else if (Flags.HasFlag(ItemFlags.NeverExclude))
            {
                return ItemFlags.NeverExclude;
            }
            else if (Flags.HasFlag(ItemFlags.Trap))
            {
                return ItemFlags.Trap;
            }
            return ItemFlags.None;
        }

        public static void CompletedLevel(int levelId)
        {
            if (!clearedLevels.Contains(levelId))
            {
                clearedLevels.Add(levelId);
                Main.Log($"Cleared Levels: {clearedLevels.Count()}");
                apSession.DataStorage[Scope.Slot, "clearedLevels"] = clearedLevels;
            }

            if (levelId == 50)
            {
                apSession.SetGoalAchieved();
            }

            if (levelId < 50 || (levelId >= 109 && levelId < 121) || (levelId >= 99 && levelId < 109))
            {
                Profile.focusedLevelId = levelId;
                if (levelId < 50 && adventureProgression != 3)
                {
                    Profile.focusedLevelId += 1;
                }
            }
            SendLocation(Data.AllLevelLocations[levelId].ClearLocation, !Menu.showAwardScreen);
        }

        public static ItemInfo GetLevelCompleteAward(GameplayActivity gameplayActivity)
        {
            int levelId = Data.GetLevelIdFromGameplayActivity(gameplayActivity);
            if (levelId != -1)
            {
                int clearLocationId = Data.AllLevelLocations[levelId].ClearLocation;
                if (clearLocationId != -1 && APClient.scoutedLocations != null && APClient.scoutedLocations.ContainsKey(clearLocationId))
                {
                    return scoutedLocations[clearLocationId];
                }
            }
            return null;
        }
    }
}
