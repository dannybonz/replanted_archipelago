using HarmonyLib;
using Il2CppBest.HTTP.Shared.Extensions;
using Il2CppReloaded.Data;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using System;

namespace ReplantedArchipelago.Patches
{
    public class Unlocks
    {
        [HarmonyPatch(typeof(UserService), nameof(UserService.IsLocked), new Type[] { typeof(LevelEntryData), typeof(bool) })]
        public static class LevelIsLockedPatch
        {
            private static bool Prefix(LevelEntryData levelEntryData, bool isRipMode, ref bool __result)
            {
                __result = true; //Default to locked

                if (levelEntryData == null)
                {
                    return false; //Level not present in this run
                }
                else if (levelEntryData.IsLimboContent) //Bonus levels
                {
                    if (APClient.receivedItems.Contains(Data.itemIds["Bonus Levels"]))
                    {
                        __result = false; //Unlocked!
                    }
                }
                else if (levelEntryData.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                {
                    if (APClient.receivedItems.Contains(Data.itemIds["Cloudy Day"]))
                    {
                        int levelNum = levelEntryData.m_subIndex;
                        if (APClient.unlockedCloudyDayLevels >= levelNum)
                        {
                            __result = false;
                        }
                    }
                }
                else if (levelEntryData.EntryType == ChallengeEntryType.MiniGame)
                {
                    if (APClient.clearedMinigames.Count >= (int)APClient.minigameUnlocks[Data.GameModeLevelIDs[levelEntryData.GameMode].ToString()]) //Unlock requirement met
                    {
                        __result = false;
                    }
                }
                else if (levelEntryData.EntryType == ChallengeEntryType.Puzzle)
                {
                    if (Data.GameModeLevelIDs.ContainsKey(levelEntryData.GameMode))
                    {
                        string gameModeLevelId = Data.GameModeLevelIDs[levelEntryData.GameMode].ToString();
                        if (gameModeLevelId.ToInt32() >= 80)
                        {
                            if (APClient.clearedIZombie.Count >= (int)APClient.izombieUnlocks[gameModeLevelId])
                            {
                                __result = false;
                            }
                        }
                        else
                        {
                            if (APClient.clearedVasebreaker.Count >= (int)APClient.vasebreakerUnlocks[gameModeLevelId])
                            {
                                __result = false;
                            }
                        }
                    }
                }
                else if (levelEntryData.EntryType == ChallengeEntryType.Survival && Data.GameModeLevelIDs.ContainsKey(levelEntryData.GameMode))
                {
                    if (APClient.clearedSurvival.Count >= (int)APClient.survivalUnlocks[Data.GameModeLevelIDs[levelEntryData.GameMode].ToString()])
                    {
                        __result = false;
                    }
                }
                else if (levelEntryData.GameMode == GameMode.Adventure)
                {
                    int levelNum = levelEntryData.m_levelNumber;
                    if (APClient.areaLockItems)
                    {
                        if (levelNum <= 10) //Day level
                        {
                            if (APClient.receivedItems.Contains(Data.DayId)) //Unlocked Day Access
                            {
                                if (APClient.unlockedDayLevels >= levelNum)
                                {
                                    __result = false;
                                }
                            }
                        }
                        else if (levelNum >= 11 && levelNum <= 20) //Night level
                        {
                            if (APClient.receivedItems.Contains(Data.NightId)) //Unlocked Night Access
                            {
                                if (APClient.unlockedNightLevels >= levelNum - 10)
                                {
                                    __result = false;
                                }
                            }
                        }
                        else if (levelNum >= 21 && levelNum <= 30)
                        {
                            if (APClient.receivedItems.Contains(Data.PoolId))
                            {
                                if (APClient.unlockedPoolLevels >= levelNum - 20)
                                {
                                    __result = false;
                                }
                            }
                        }
                        else if (levelNum >= 31 && levelNum <= 40)
                        {
                            if (APClient.receivedItems.Contains(Data.FogId))
                            {
                                if (APClient.unlockedFogLevels >= levelNum - 30)
                                {
                                    __result = false;
                                }
                            }
                        }
                        else if (levelNum >= 41 && levelNum <= 50)
                        {
                            if (APClient.receivedItems.Contains(Data.RoofId))
                            {
                                if (APClient.unlockedRoofLevels >= levelNum - 40)
                                {
                                    if (!(levelNum == 50 && APClient.requireAllLevels && APClient.clearedAdventure.Count < 49))
                                    {
                                        if (levelNum == 50)
                                        {
                                            APClient.bossUnlocked = true;
                                            Main.userService.ActiveUserProfile.mLevel = 50;
                                        }
                                        __result = false;
                                    }
                                }
                            }
                        }

                    }
                    else //Area lock items disabled
                    {
                        if (levelNum <= APClient.unlockedAdventureLevels)
                        {
                            __result = false;
                        }
                    }
                }
                return false; //Don't run default method
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.HasSeedType))]
        public class HasSeedTypePatch
        {
            private static void Postfix(UserService __instance, SeedType theSeedType, ref bool __result)
            {
                __result = APClient.HasSeedType(theSeedType);
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsPuzzleModeAvailable))]
        public class IsPuzzleAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Puzzle Mode"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsCoopModeAvailable))]
        public class IsCoopAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false; //Co-Op mode not included... yet
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsAlmanacAvailable))]
        public class IsAlmanacAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Almanac"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsStoreAvailable))]
        public class IsStoreAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Crazy Dave's Car Keys"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsZenGardenAvailable))]
        public class IsZenGardenAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Zen Garden"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsCloudyDayAvailable))]
        public class IsCloudyDayAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Cloudy Day"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsLimboContentAvailable))]
        public class IsLimboAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Bonus Levels"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsMinigameModeAvailable))]
        public class IsMinigameAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Minigame Mode"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsSurvivalModeAvailable))]
        public class IsSurvivalAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["Survival Mode"]);
                return false;
            }
        }
    }
}
