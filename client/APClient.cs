using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Il2CppReloaded.Data;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using MelonLoader;
using Newtonsoft.Json.Linq;
using ReplantedArchipelago.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReplantedArchipelago
{
    public class APClient
    {
        public static int connectionStatus = 0; //0 = not connected, 1 = connected, 2 = error, 3 = too many profiles, 4 = version mismatch
        public static string slot;
        public static string host;
        public static string password;

        public static ArchipelagoSession apSession;
        public static List<long> receivedItems = new List<long>();
        public static Dictionary<long, ScoutedItemInfo> scoutedLocations;
        public static int displayedIngameMessages = 0;
        public static double genVersion;

        public static Dictionary<string, object> slotData;
        public static JArray musicMap;
        public static JArray shopPrices;
        public static JObject minigameUnlocks;
        public static JObject survivalUnlocks;
        public static JObject vasebreakerUnlocks;
        public static JObject izombieUnlocks;
        public static bool areaLockItems;
        public static bool requireAllLevels;
        public static bool imitaterOpen;

        public static int shopPages;
        public static int shopPagesVisible = 1;
        public static int unlockedAdventureLevels = 1;
        public static int unlockedDayLevels = 1;
        public static int unlockedNightLevels = 1;
        public static int unlockedPoolLevels = 1;
        public static int unlockedFogLevels = 1;
        public static int unlockedRoofLevels = 1;
        public static int unlockedCloudyDayLevels = 0;
        public static bool bossUnlocked = false;
        public static bool newSecrets = false;
        public static string chooserRefreshState = "none";

        public static List<int> clearedAdventure = new List<int>();
        public static List<int> clearedMinigames = new List<int>();
        public static List<int> clearedSurvival = new List<int>();
        public static List<int> clearedIZombie = new List<int>();
        public static List<int> clearedVasebreaker = new List<int>();

        public static void AttemptConnection(string hostInput, string slotInput, string passwordInput)
        {
            apSession = ArchipelagoSessionFactory.CreateSession(hostInput);
            LoginResult result;

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
                connectionStatus = 2; //Displays error on login box
            }
            else
            {
                var loginSuccess = (LoginSuccessful)result;
                slotData = loginSuccess.SlotData;

                genVersion = Convert.ToDouble(slotData["gen_version"]);

                if (genVersion != Data.GenVersion) //Version mismatch
                {
                    connectionStatus = 4;
                }
                else
                {
                    //Reset received items
                    receivedItems = new List<long>();

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
                    areaLockItems = Convert.ToBoolean(slotData["adventure_area_items"]);
                    requireAllLevels = Convert.ToBoolean(slotData["require_all_levels"]);
                    imitaterOpen = Convert.ToBoolean(slotData["imitater_open"]);

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

                    //Initialise DataStorage
                    if (areaLockItems)
                    {
                        apSession.DataStorage[Scope.Slot, "unlockedDayLevels"].Initialize(1);
                        apSession.DataStorage[Scope.Slot, "unlockedNightLevels"].Initialize(1);
                        apSession.DataStorage[Scope.Slot, "unlockedPoolLevels"].Initialize(1);
                        apSession.DataStorage[Scope.Slot, "unlockedFogLevels"].Initialize(1);
                        apSession.DataStorage[Scope.Slot, "unlockedRoofLevels"].Initialize(1);
                    }
                    else
                    {
                        apSession.DataStorage[Scope.Slot, "unlockedAdventureLevels"].Initialize(1);
                    }
                    apSession.DataStorage[Scope.Slot, "unlockedCloudyDayLevels"].Initialize(0);

                    //Stores completed minigames (used for unlocking the next one)
                    apSession.DataStorage[Scope.Slot, "clearedAdventure"].Initialize(JArray.FromObject(new List<int>()));
                    apSession.DataStorage[Scope.Slot, "clearedMinigames"].Initialize(JArray.FromObject(new List<int>()));
                    apSession.DataStorage[Scope.Slot, "clearedSurvival"].Initialize(JArray.FromObject(new List<int>()));
                    apSession.DataStorage[Scope.Slot, "clearedIZombie"].Initialize(JArray.FromObject(new List<int>()));
                    apSession.DataStorage[Scope.Slot, "clearedVasebreaker"].Initialize(JArray.FromObject(new List<int>()));

                    clearedAdventure = apSession.DataStorage[Scope.Slot, "clearedAdventure"];
                    clearedMinigames = apSession.DataStorage[Scope.Slot, "clearedMinigames"];
                    clearedSurvival = apSession.DataStorage[Scope.Slot, "clearedSurvival"];
                    clearedIZombie = apSession.DataStorage[Scope.Slot, "clearedIZombie"];
                    clearedVasebreaker = apSession.DataStorage[Scope.Slot, "clearedVasebreaker"];

                    connectionStatus = 1; //Connection successful!

                    Main.DoProfileCheck(); //Swap to correct profile

                    if (connectionStatus == 1) //If profile swapped sucessfully..
                    {
                        ProcessAllItems(); //Process all items received since the beginning of time
                        apSession.Items.ItemReceived += RunOnItemReceived; //Set handler for any items received in this session

                        Time.timeScale = 1f; // Resume the game if paused
                    }
                }
            }
        }

        public static void ProcessAllItems()
        {
            if (areaLockItems)
            {
                unlockedDayLevels = apSession.DataStorage[Scope.Slot, "unlockedDayLevels"];
                unlockedNightLevels = apSession.DataStorage[Scope.Slot, "unlockedNightLevels"];
                unlockedPoolLevels = apSession.DataStorage[Scope.Slot, "unlockedPoolLevels"];
                unlockedFogLevels = apSession.DataStorage[Scope.Slot, "unlockedFogLevels"];
                unlockedRoofLevels = apSession.DataStorage[Scope.Slot, "unlockedRoofLevels"];
            }
            else
            {
                unlockedAdventureLevels = apSession.DataStorage[Scope.Slot, "unlockedAdventureLevels"];
            }
            unlockedCloudyDayLevels = apSession.DataStorage[Scope.Slot, "unlockedCloudyDayLevels"];


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
                    Main.userService.AddCoins(1);
                }
                else if (item.ItemId == 61)
                {
                    Main.userService.AddCoins(5);
                }
                else if (item.ItemId == 62)
                {
                    Main.userService.AddCoins(100);
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
                Main.userService.ActiveUserProfile.mPurchases[(int)StoreItem.Firstaid] = 1;
            }
            else if (item.ItemId == Data.itemIds["Pool Cleaners"])
            {
                Main.userService.ActiveUserProfile.mPurchases[(int)StoreItem.PoolCleaner] = 1;
            }
            else if (item.ItemId == Data.itemIds["Roof Cleaners"])
            {
                Main.userService.ActiveUserProfile.mPurchases[(int)StoreItem.RoofCleaner] = 1;
            }
            else if (item.ItemId >= 50 && item.ItemId < 60)
            {
                newSecrets = true;
            }
            else if (item.ItemId >= 100 && item.ItemId < 200)
            {
                chooserRefreshState = "update"; //Used to force a Seed Chooser rebuild for any plants obtained after a level has already begun
            }

            if (Data.menuUpdateItems.Contains(item.ItemId))
            {
                Main.profileRefreshRequired = true;
            }
        }

        public static void RunOnItemReceived(ReceivedItemsHelper itemHandler)
        {
            ProcessItemInfo(itemHandler.DequeueItem());
            apSession.DataStorage[Scope.Slot, "displayedIngameMessages"] = displayedIngameMessages;
        }

        public static void HandleDisconnect()
        {
            Main.readyToConnect = true;
            connectionStatus = 2;
        }

        public static void SendLocation(int locationId)
        {
            if (!apSession.Locations.AllLocationsChecked.Contains(locationId)) //Don't waste time re-sending already sent checks
            {
                if (scoutedLocations != null && scoutedLocations.ContainsKey(locationId)) //If the location has been scouted, queue an ingame message
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
                apSession.Locations.CompleteLocationChecks(locationId);
            }
        }

        public static void SendWaveLocation(LevelEntryData levelData, int hugeWaveNumber)
        {
            if (levelData.ReloadedGameMode == ReloadedGameMode.CloudyDay)
            {
                if (Data.AllLevelLocations[110 + levelData.m_subIndex].FlagLocations.Length > hugeWaveNumber)
                {
                    SendLocation(Data.AllLevelLocations[110 + levelData.m_subIndex].FlagLocations[hugeWaveNumber]);
                }
            }
            else if (levelData.GameMode == GameMode.Adventure)
            {
                if (Data.AllLevelLocations[levelData.m_levelNumber].FlagLocations.Length > hugeWaveNumber)
                {
                    SendLocation(Data.AllLevelLocations[levelData.m_levelNumber].FlagLocations[hugeWaveNumber]);
                }
            }
            else if (Data.GameModeLevelIDs.ContainsKey(levelData.GameMode))
            {
                if (Data.AllLevelLocations[Data.GameModeLevelIDs[levelData.GameMode]].FlagLocations.Length > hugeWaveNumber)
                {
                    SendLocation(Data.AllLevelLocations[Data.GameModeLevelIDs[levelData.GameMode]].FlagLocations[hugeWaveNumber]);
                }
            }
        }

        public static void RegisterProfile(UserProfile theProfile)
        {
            List<string> profileGuids = apSession.DataStorage[Scope.Slot, "profileGuids"];
            profileGuids.Add(theProfile.mGuid);
            apSession.DataStorage[Scope.Slot, "profileGuids"] = JArray.FromObject(profileGuids);

            MelonLogger.Msg("Profile Validation: Registered new profile.");
            Main.creatingProfile = false;
        }

        public static bool HasSeedType(SeedType theSeedType)
        {
            if (connectionStatus == 1 && receivedItems != null && receivedItems.Contains(100 + Array.IndexOf(Data.seedTypes, theSeedType)))
            {
                return true;
            }

            return false;
        }

        public static bool HasShovel()
        {
            if (connectionStatus == 1 && receivedItems != null && receivedItems.Contains(Data.itemIds["Shovel"]))
            {
                return true;
            }

            return false;
        }

        public static int GetSeedSlots(params long[] extraPlants)
        {
            int numberOfSlots = receivedItems.Count(item => item == Data.itemIds["Extra Seed Slot"]) + 1;

            int numberOfPlants = GetHowManyPlants(extraPlants);

            return Math.Min(GetHowManyPlants(), numberOfSlots);
        }

        public static int GetHowManyPlants(params long[] extraPlants)
        {
            var receivedPlants = receivedItems.Where(item => item >= 100 && item < 200);

            if (extraPlants == null || extraPlants.Length == 0)
            {
                return receivedPlants.Distinct().Count();
            }
            else
            {
                return receivedPlants.Union(extraPlants).Distinct().Count();
            }
        }

        public static void CompletedLevel(int levelNumber, string mode)
        {
            if (mode == "Adventure")
            {
                if (levelNumber == 50)
                {
                    apSession.SetGoalAchieved();
                }

                if (areaLockItems)
                {
                    if (levelNumber <= 10 && levelNumber >= unlockedDayLevels)
                    {
                        unlockedDayLevels = levelNumber + 1;
                        apSession.DataStorage[Scope.Slot, "unlockedDayLevels"] = unlockedDayLevels;
                        Main.userService.SetLevel(levelNumber + 1);
                    }
                    else if (levelNumber <= 20 && levelNumber > 10 && (levelNumber - 9) > unlockedNightLevels)
                    {
                        unlockedNightLevels = levelNumber - 9;
                        apSession.DataStorage[Scope.Slot, "unlockedNightLevels"] = unlockedNightLevels;
                        Main.userService.SetLevel(levelNumber + 1);
                    }
                    else if (levelNumber <= 30 && levelNumber > 20 && (levelNumber - 19) > unlockedPoolLevels)
                    {
                        unlockedPoolLevels = levelNumber - 19;
                        apSession.DataStorage[Scope.Slot, "unlockedPoolLevels"] = unlockedPoolLevels;
                        Main.userService.SetLevel(levelNumber + 1);
                    }
                    else if (levelNumber <= 40 && levelNumber > 30 && (levelNumber - 29) > unlockedFogLevels)
                    {
                        unlockedFogLevels = levelNumber - 29;
                        apSession.DataStorage[Scope.Slot, "unlockedFogLevels"] = unlockedFogLevels;
                        Main.userService.SetLevel(levelNumber + 1);
                    }
                    else if (levelNumber <= 50 && levelNumber > 40 && (levelNumber - 39) > unlockedRoofLevels)
                    {
                        unlockedRoofLevels = levelNumber - 39;
                        apSession.DataStorage[Scope.Slot, "unlockedRoofLevels"] = unlockedRoofLevels;
                        Main.userService.SetLevel(levelNumber + 1);
                    }
                }
                else if (levelNumber >= unlockedAdventureLevels)
                {
                    if (levelNumber != 50)
                    {
                        unlockedAdventureLevels = levelNumber + 1;
                        apSession.DataStorage[Scope.Slot, "unlockedAdventureLevels"] = unlockedAdventureLevels;
                        Main.userService.SetLevel(levelNumber + 1);
                    }
                }
            }
            else if (mode == "Cloudy Day")
            {
                if (levelNumber >= unlockedCloudyDayLevels)
                {
                    unlockedCloudyDayLevels = levelNumber + 1;
                    apSession.DataStorage[Scope.Slot, "unlockedCloudyDayLevels"] = unlockedCloudyDayLevels;
                }
            }
        }
    }
}
