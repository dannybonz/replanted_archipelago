using HarmonyLib;
using Il2CppReloaded.Data;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using System;
using System.Linq;

namespace ReplantedArchipelago.Patches
{
    public class Unlocks
    {
        [HarmonyPatch(typeof(UserService), nameof(UserService.IsLocked), new Type[] { typeof(LevelEntryData), typeof(bool) })]
        public static class LevelIsLockedPatch
        {
            private static bool Prefix(LevelEntryData levelEntryData, bool isRipMode, ref bool __result)
            {
                __result = true; //Default to unlocked
                if (!(levelEntryData == null || !APClient.currentlyConnected))
                {
                    int levelId = -1;

                    if (levelEntryData.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                    {
                        levelId = 109 + levelEntryData.m_subIndex;
                    }
                    else if (levelEntryData.GameMode == GameMode.Adventure)
                    {
                        levelId = levelEntryData.m_levelNumber;
                    }
                    else if (Data.GameModeLevelIDs.ContainsKey(levelEntryData.GameMode))
                    {
                        levelId = Data.GameModeLevelIDs[levelEntryData.GameMode];
                    }

                    if (levelId != -1)
                    {
                        __result = !APClient.CanPlayLevel(levelId);
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
                if (APClient.individualMinigameUnlockItems)
                {
                    __result = APClient.receivedItems.Any(x => x >= 271 && x < 289);
                }
                else
                {
                    __result = APClient.receivedItems.Contains(Data.itemIds["Puzzle Mode"]);
                }
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
                if (APClient.individualMinigameUnlockItems)
                {
                    __result = APClient.receivedItems.Any(x => x >= 251 && x < 271);
                }
                else
                {
                    __result = APClient.receivedItems.Contains(Data.itemIds["Minigame Mode"]);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsSurvivalModeAvailable))]
        public class IsSurvivalAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                if (APClient.individualMinigameUnlockItems)
                {
                    __result = APClient.receivedItems.Any(x => x >= 289 && x < 299);
                }
                else
                {
                    __result = APClient.receivedItems.Contains(Data.itemIds["Survival Mode"]);
                }
                return false;
            }
        }
    }
}
