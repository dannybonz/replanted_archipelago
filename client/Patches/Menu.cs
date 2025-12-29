using HarmonyLib;
using Il2CppBest.HTTP.Shared.Extensions;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.UI;
using Il2CppSource.DataModels;
using Il2CppTekly.DataModels.Models;
using Il2CppUI.Scripts;
using System.Linq;
using UnityEngine;

namespace ReplantedArchipelago.Patches
{
    public class Menu
    {
        [HarmonyPatch(typeof(MainMenuPanelView), nameof(MainMenuPanelView.Update))]
        public class MainMenuUpdatePatch
        {
            private static void Postfix(MainMenuPanelView __instance)
            {
                GameObject usersPanel = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                if (usersPanel != null)
                {
                    usersPanel.SetActive(false);
                }

                APClient.chooserRefreshState = "none"; //No need to refresh seed chooser
            }
        }

        [HarmonyPatch(typeof(MainMenuPanelView), nameof(MainMenuPanelView.Start))]
        public class MainMenuStartPatch
        {
            private static void Postfix(MainMenuPanelView __instance)
            {
                if (APClient.newSecrets)
                {
                    APClient.newSecrets = false;
                    Main.RefreshProfile();
                }

                GameObject accountSign = __instance.transform.Find("Canvas/Layout/Center/Main/AccountSign").gameObject;
                if (accountSign != null)
                {
                    accountSign.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(LevelSelectScreen), nameof(LevelSelectScreen.OnEnterLevelSelect))]
        public class LevelSelectEnterPatch
        {
            private static void Postfix(LevelSelectScreen __instance)
            {
                Main.userService.ActiveUserProfile.mNeedsMagicTacoReward = 0; //Fixes the 4-5 shop bug

                //Jump straight to boss if unlocked
                if (APClient.bossUnlocked)
                {
                    __instance.CarouselGroups[4].levelCarousel.SelectLevel(9);
                    __instance.CarouselGroups[4].levelCarousel.SetSelected();
                    __instance.UpdateSelectedCarousel(__instance.CarouselGroups[4]);
                }
            }
        }

        [HarmonyPatch(typeof(AwardScreenDataModel), nameof(AwardScreenDataModel.GetTransitionName))] //Decides where the game will go after closing the level's award screen
        public class AwardScreenTransitionPatch
        {
            private static bool Prefix(AwardScreenDataModel __instance, ref string __result)
            {
                if (__result == null)
                {
                    __result = "LevelSelect";
                    if (Data.GameModeLevelIDs.ContainsKey(__instance.m_gameplayActivity.GameMode))
                    {
                        int levelId = Data.GameModeLevelIDs[__instance.m_gameplayActivity.GameMode];
                        if (levelId >= 71 && levelId <= 88)
                        {
                            __result = "Puzzle";
                        }
                        else if (levelId >= 51 && levelId <= 70)
                        {
                            __result = "MiniGames";
                        }
                        else if (levelId >= 89 && levelId <= 98)
                        {
                            __result = "Survival";
                        }
                    }
                }
                if (__instance.m_gameplayActivity.m_levelData.m_levelNumber == 50)
                {
                    __result = "Credits";
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(SeedChooserDataModel), nameof(SeedChooserDataModel.OnTick))]
        public static class SeedChooserDataModelOnTickPatch
        {
            private static void Prefix(SeedChooserDataModel __instance)
            {
                __instance.m_isImitaterUnlocked.m_value = APClient.HasSeedType(SeedType.Imitater);

                if (!APClient.imitaterOpen)
                {
                    Il2CppSystem.Collections.Generic.List<ModelReference> imitaterEntries = __instance.m_imitaterEntriesModel.m_models;
                    for (int i = imitaterEntries.Count - 1; i >= 0; i--)
                    {
                        var entry = imitaterEntries[i].Model.Cast<SeedChooserEntryModel>();

                        if (!APClient.HasSeedType(entry.m_chosenSeed.mSeedType))
                        {
                            imitaterEntries.RemoveAt(i);
                        }
                    }
                }

                if (APClient.chooserRefreshState == "update" && __instance.m_isVisibleModel.Value)
                {
                    __instance.m_entriesUnlockedModel.Clear();
                    __instance.UpdateEntries();
                    APClient.chooserRefreshState = "toggle";
                }
            }
        }

        [HarmonyPatch(typeof(SeedChooserScreen), nameof(SeedChooserScreen.DisplayRepickWarningDialog))] //Triggers when displaying a warning after seed selection
        public static class SeedWarningPatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false; //Never show the warning
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelDataModel), nameof(LevelDataModel.UpdateModelData))] //Used to order Survival mode level entries (not sure why they are handled differently by the game, but they are)
        public static class RefreshEntriesPatch
        {
            private static void Prefix(LevelDataModel __instance)
            {
                LevelEntriesModel survivalModel = __instance.m_survivalModel;
                Il2CppSystem.Collections.Generic.List<ModelReference> survivalEntries = survivalModel.m_entriesModel.m_models;

                if (Data.orderedSurvivalEntries.Count() == 0)
                {
                    LevelEntryData[] defaultSurvivalEntries = new LevelEntryData[10];
                    for (int i = 0; i < survivalEntries.Count; i++)
                    {
                        defaultSurvivalEntries[i] = survivalEntries[i].Model.Cast<LevelEntryModel>().m_entryData;
                    }

                    var sortedOrder = APClient.survivalUnlocks.Properties().OrderBy(p => (int)p.Value).ToList();
                    Data.orderedSurvivalEntries = new LevelEntryData[10];
                    for (int i = 0; i < survivalEntries.Count; i++)
                    {
                        Data.orderedSurvivalEntries[i] = defaultSurvivalEntries[sortedOrder[i].Name.ToInt32() - 89];
                    }
                }

                for (int i = 0; i < survivalEntries.Count; i++)
                {
                    LevelEntryModel currentModel = survivalEntries[i].Model.Cast<LevelEntryModel>();
                    currentModel.m_entryData = Data.orderedSurvivalEntries[i];
                }

                survivalModel.RefreshEntries();
            }
        }
    }
}
