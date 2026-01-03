using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using Il2CppSource.Controllers;
using Il2CppSource.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using UnityEngine;

namespace ReplantedArchipelago.Patches
{
    public class Gameplay
    {
        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.ActiveUpdate))] //Runs every frame during gameplay
        public class GameplayActivityUpdatePatch
        {
            private static void Postfix(GameplayActivity __instance)
            {
                if (Main.currentScene != "Gameplay" || __instance == null || __instance.Board == null || !(__instance.GameScene == GameScenes.Playing || __instance.GameScene == GameScenes.LevelIntro))
                {
                    return;
                }

                if (APClient.chooserRefreshState == "toggle" && __instance.m_crazyDaveService.CrazyDaveState == CrazyDaveState.Off)
                {
                    __instance.ShowSeedChooserScreen();
                    APClient.chooserRefreshState = "none";
                }

                //Cheat keys
                var board = __instance.Board; //Represents the lawn and its contents
                if (Data.CheatKeys)
                {
                    //Instant Level Win - F1
                    if (Input.GetKeyDown(KeyCode.F1))
                    {
                        board.FadeOutLevel();
                    }

                    //Add 500 Sun - F2
                    if (Input.GetKeyDown(KeyCode.F2))
                    {
                        board.AddSunMoney(500, 0);
                    }

                    //Chooser refresh - F3
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        APClient.chooserRefreshState = "update";
                    }

                    //Exit game - F4
                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        StateTransitionUtils.Transition("Frontend");
                    }
                }

                if (board.mTutorialState != TutorialState.Off)
                {
                    board.SetTutorialState(TutorialState.Off);
                }

                if (__instance.GameScene == GameScenes.Playing && Main.QueuedIngameMessages.Count > 0 && board.mAdvice.mDuration == 0) //If there are queued up AP messages to display
                {
                    Data.QueuedIngameMessage message = Main.QueuedIngameMessages.Dequeue();
                    Main.currentMessage = message.MessageLabel;

                    //Init ingame message
                    board.DisplayAdviceAgain("AP_PLACEHOLDER", MessageStyle.HintLong, AdviceType.NeedWheelbarrow);
                    board.mAdvice.ClearLabel();

                    //Set style
                    board.mAdvice.mLabel = Main.currentMessage;
                    board.mAdvice.mMessageStyle = MessageStyle.HintLong;
                    board.mAdvice.mFlashing = (float)0.7529412;
                    board.mAdvice.mPosY = 527;
                    board.mAdvice.mGreyBoxHeight = 55;
                    board.mAdvice.mColor = new UnityEngine.Color((float)0.992, (float)0.961, (float)0.678);

                    //Activate traps
                    if (message.ItemId == Data.itemIds["Seed Packet Cooldown Trap"]) //Seed Packet Cooldown Trap
                    {
                        if (board.HasConveyorBeltSeedBank() == false) //Don't trigger if playing a conveyor belt level (causes weird issues)
                        {
                            foreach (SeedPacket seedPacket in board.SeedBanks[0].SeedPackets)
                            {
                                if (seedPacket != null && seedPacket.PacketType != SeedType.None)
                                {
                                    seedPacket.mRefreshCounter = 0;
                                    seedPacket.mRefreshTime = Plant.GetRefreshTime(__instance, seedPacket.mPacketType, seedPacket.mImitaterType);
                                    seedPacket.mRefreshing = true;
                                    seedPacket.mActive = false;
                                }
                            }
                        }
                    }
                    else if (message.ItemId == Data.itemIds["Mower Deploy Trap"]) //Mower Deploy Trap
                    {
                        for (int i = 0; i < board.m_lawnMowers.Count; i++)
                        {
                            board.m_lawnMowers[i].StartMower();
                        }
                    }
                    else if (message.ItemId == Data.itemIds["Zombie Ambush Trap"]) //Zombie Ambush Trap
                    {
                        if (board.mBackground == BackgroundType.Pool || board.mBackground == BackgroundType.Fog)
                        {
                            board.SpawnZombiesFromPool();
                        }
                        else if (board.mBackground == BackgroundType.Night)
                        {
                            board.SpawnZombiesFromGraves();
                        }
                        else if (!(__instance.GameMode == GameMode.Adventure && board.mLevel < 4))
                        {
                            board.SpawnZombiesFromSky();
                        }
                    }

                    //Activate secrets
                    if (message.ItemId == Data.itemIds["mustache"])
                    {
                        __instance.UserService.ActiveUserProfile.mMustacheModeActive = true;
                        board.SetMustacheMode(true);
                    }
                    else if (message.ItemId == Data.itemIds["future"])
                    {
                        __instance.UserService.ActiveUserProfile.mFutureModeActive = true;
                        board.SetFutureMode(true);
                    }
                    else if (message.ItemId == Data.itemIds["trickedout"])
                    {
                        __instance.UserService.ActiveUserProfile.mTrickedOutModeActive = true;
                        board.SetSuperMowerMode(true);
                    }
                    else if (message.ItemId == Data.itemIds["daisies"])
                    {
                        __instance.UserService.ActiveUserProfile.mDaisesModeActive = true;
                        board.SetDaisyMode(true);
                    }
                    else if (message.ItemId == Data.itemIds["pinata"])
                    {
                        __instance.UserService.ActiveUserProfile.mPinataModeActive = true;
                        board.SetPinataMode(true);
                    }
                    else if (message.ItemId == Data.itemIds["sukhbir"])
                    {
                        __instance.UserService.ActiveUserProfile.mSukhbirModeActive = true;
                        board.SetSukhbirMode(true);
                    }
                    else if (message.ItemId == Data.itemIds["dance"])
                    {
                        __instance.UserService.ActiveUserProfile.mDanceModeActive = true;
                        board.SetDanceMode(true);
                    }

                    int messageDuration = 600 - (Main.QueuedIngameMessages.Count * 5); //Reduces message duration if there are lots of them queued up
                    if (messageDuration < 100)
                    {
                        messageDuration = 100;
                    }

                    board.mAdvice.mDuration = messageDuration;
                }
                else if (board.mAdvice.mDuration == 10000) //Certain tutorial messages are given this number
                {
                    board.mAdvice.mDuration = 0;
                }

                //Update shovel display
                if (board.ShowShovel == true && APClient.HasShovel() == false)
                {
                    board.ShowShovel = false;
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.AddCoin))] //Triggers when a Zombie drops loot
        public static class AddCoinPatch
        {
            private static bool Prefix(Board __instance, ref CoinType theCoinType)
            {
                if (theCoinType == CoinType.FinalSeedPacket ||
                    theCoinType == CoinType.AwardBagDiamond ||
                    theCoinType == CoinType.AwardMoneyBag ||
                    theCoinType == CoinType.Almanac ||
                    theCoinType == CoinType.Taco ||
                    theCoinType == CoinType.CarKeys ||
                    theCoinType == CoinType.Shovel ||
                    theCoinType == CoinType.WateringCan ||
                    theCoinType == CoinType.Trophy ||
                    theCoinType == CoinType.Note)
                {
                    __instance.FadeOutLevel();
                    return false;
                }
                else if (theCoinType == CoinType.PresentMinigames ||
                        theCoinType == CoinType.PresentPuzzleMode ||
                        theCoinType == CoinType.PresentSurvivalMode)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.FadeOutLevel))] //Triggers on level complete
        public static class FadeOutPatch
        {
            private static void Prefix(Board __instance)
            {
                if ((!__instance.mApp.IsScaryPotterLevel() || __instance.IsFinalScaryPotterStage()) && (!__instance.mApp.IsSurvivalMode() || __instance.IsFinalSurvivalStage()) && !__instance.IsLastStandStageWithRepick())
                {
                    if (__instance.mApp.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                    {
                        int cloudyNumber = __instance.mApp.LevelData.m_subIndex;
                        APClient.CompletedLevel(cloudyNumber, "Cloudy Day");
                        APClient.SendLocation(Data.AllLevelLocations[110 + cloudyNumber].ClearLocation);
                    }
                    else if (__instance.mApp.GameMode == GameMode.Adventure)
                    {
                        int levelNumber = __instance.mLevel;

                        if (!APClient.clearedAdventure.Contains(levelNumber))
                        {
                            APClient.clearedAdventure.Add(levelNumber);
                            APClient.apSession.DataStorage[Scope.Slot, "clearedAdventure"] = JArray.FromObject(APClient.clearedAdventure);
                        }

                        APClient.CompletedLevel(__instance.mLevel, "Adventure");
                        APClient.SendLocation(Data.AllLevelLocations[levelNumber].ClearLocation);
                    }
                    else if (Data.GameModeLevelIDs.ContainsKey(__instance.mApp.GameMode))
                    {
                        if (!(__instance.mApp.GameMode == GameMode.ChallengeLastStand && __instance.GetSurvivalFlagsCompleted() < 5))
                        {
                            int gameModeLevelId = Data.GameModeLevelIDs[__instance.mApp.GameMode];
                            APClient.SendLocation(Data.AllLevelLocations[gameModeLevelId].ClearLocation);
                            if (gameModeLevelId >= 51 && gameModeLevelId <= 70)
                            {
                                if (!APClient.clearedMinigames.Contains(gameModeLevelId))
                                {
                                    APClient.clearedMinigames.Add(gameModeLevelId);
                                    APClient.apSession.DataStorage[Scope.Slot, "clearedMinigames"] = JArray.FromObject(APClient.clearedMinigames);
                                }
                            }
                            else if (gameModeLevelId >= 89 && gameModeLevelId <= 98)
                            {
                                if (!APClient.clearedSurvival.Contains(gameModeLevelId))
                                {
                                    APClient.clearedSurvival.Add(gameModeLevelId);
                                    APClient.apSession.DataStorage[Scope.Slot, "clearedSurvival"] = JArray.FromObject(APClient.clearedSurvival);
                                }
                            }
                            else if (gameModeLevelId >= 71 && gameModeLevelId <= 79)
                            {
                                if (!APClient.clearedVasebreaker.Contains(gameModeLevelId))
                                {
                                    APClient.clearedVasebreaker.Add(gameModeLevelId);
                                    APClient.apSession.DataStorage[Scope.Slot, "clearedVasebreaker"] = JArray.FromObject(APClient.clearedVasebreaker);
                                }
                            }
                            else if (gameModeLevelId >= 80 && gameModeLevelId <= 88)
                            {
                                if (!APClient.clearedIZombie.Contains(gameModeLevelId))
                                {
                                    APClient.clearedIZombie.Add(gameModeLevelId);
                                    APClient.apSession.DataStorage[Scope.Slot, "clearedIZombie"] = JArray.FromObject(APClient.clearedIZombie);
                                }
                            }
                        }
                    }
                }
                else if (__instance.mApp.IsSurvivalMode() && !__instance.IsFinalSurvivalStage())
                {
                    int completedFlags = __instance.GetSurvivalFlagsCompleted();
                    Main.Log($"Survival Flag {completedFlags}");
                    if (Data.GameModeLevelIDs.ContainsKey(__instance.mApp.GameMode))
                    {
                        try
                        {
                            APClient.SendWaveLocation(__instance.mApp.LevelData, completedFlags);
                        }
                        catch
                        {
                            Main.Log($"Unexpected Wave Number (#{completedFlags})");
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.CanDropLoot))] //Triggers when checking to spawn coins - typically waits until after 2-1, but we change it to happen at any time
        public static class CanDropLootPatch
        {
            private static bool Prefix(Board __instance, ref bool __result)
            {
                __result = __instance.mApp.GameMode != GameMode.Intro;
                return false;
            }
        }

        [HarmonyPatch(typeof(AudioService), nameof(AudioService.StartGameMusic))] //Triggers after "Ready, Set, Plant!" to begin the level's music
        public class StartGameMusicPatch
        {
            private static void Postfix(AudioService __instance)
            {
                if (APClient.musicMap.Count > 0 && __instance.m_currentMusicTune != MusicTune.None)
                {
                    if (APClient.musicMap.Count == 9)
                    {
                        int currentMusicIndex = Array.IndexOf(Data.musicTunes, __instance.m_currentMusicTune);
                        __instance.MakeSureMusicIsPlaying(Data.musicTunes[(int)APClient.musicMap[currentMusicIndex]]);
                    }
                    else
                    {
                        int levelIndex;
                        if (__instance.m_app.GameMode == GameMode.Adventure)
                        {
                            levelIndex = __instance.m_app.m_levelData.LevelNumber - 1;
                        }
                        else
                        {
                            levelIndex = Data.GameModeLevelIDs[__instance.m_app.GameMode];
                        }
                        __instance.MakeSureMusicIsPlaying(Data.musicTunes[(int)APClient.musicMap[levelIndex]]);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Zombie), nameof(Zombie.ZombieInitialize))] //Triggers when new Zombie spawns
        public static class ZombieInitPatch
        {
            private static void Postfix(Zombie __instance)
            {
                int zombieWave = __instance.mFromWave;
                if (zombieWave > 0)
                {
                    int wavesPerFlag = __instance.mBoard.GetNumWavesPerFlag();
                    if (zombieWave % wavesPerFlag == 0)
                    {
                        if (__instance.mApp.GameMode == GameMode.ChallengeLastStand || __instance.mApp.IsSurvivalMode())
                        {
                            int completedFlags = __instance.mBoard.GetSurvivalFlagsCompleted();
                            APClient.SendWaveLocation(__instance.mApp.LevelData, completedFlags);
                        }
                        else
                        {
                            APClient.SendWaveLocation(__instance.mApp.LevelData, (__instance.mFromWave / __instance.mBoard.GetNumWavesPerFlag()) - 1);
                        }
                    }
                }
                if (Data.AllZombiesDie) //Cheat to kill zombies instantly
                {
                    __instance.TakeDamage(10000, DamageFlags.BypassesShield);
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.ActiveStarted))]
        public class NewGameplayActivityPatch
        {
            private static void Postfix(GameplayActivity __instance)
            {
                Main.gameplayActivity = __instance;
                Main.Log("GameplayActivity Updated");
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.GetSeedsAvailable))]
        public class GetSeedsAvailablePatch
        {
            private static void Postfix(GameplayActivity __instance, ref int __result)
            {
                if (__instance.GameMode == GameMode.ChallengeRainingSeeds)
                {
                    __result = 49;
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.GetNumSeedsInBank))]
        public class NumSeedsInBankPatch
        {
            private static bool Prefix(Board __instance, ref int __result)
            {
                if (!__instance.mApp.IsCoopMode() && !__instance.mApp.IsVersusMode() && !__instance.mApp.IsIZombieLevel() && !__instance.mApp.IsScaryPotterLevel() && !__instance.mApp.IsWhackAZombieLevel() && !__instance.mApp.IsChallengeWithoutSeedBank() && !__instance.HasConveyorBeltSeedBank() && __instance.mApp.GameMode != GameMode.ChallengeBeghouled && __instance.mApp.GameMode != GameMode.ChallengeBeghouledTwist && __instance.mApp.GameMode != GameMode.ChallengeZombiquarium)
                {
                    long[] forcedPlants = Array.Empty<long>();
                    long[] bannedPlants = Array.Empty<long>();

                    if (__instance.mApp.GameMode == GameMode.ChallengeArtChallenge1)
                    {
                        forcedPlants = new long[] { 103 };
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeArtChallenge2)
                    {
                        forcedPlants = new long[] { 103, 129, 137 };
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeSeeingStars)
                    {
                        forcedPlants = new long[] { 129 };
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeLastStand)
                    {
                        bannedPlants = new long[] { 101, 109, 141 };
                    }
                    else if (__instance.mApp.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                    {
                        bannedPlants = new long[] { 109, 141 };
                    }

                    __result = APClient.GetSeedSlots(forcedPlants, bannedPlants);
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(Board), nameof(Board.ChooseSeedsOnCurrentLevel))]
        public static class ChooseSeedsOnCurrentLevelPatch
        {
            private static bool Prefix(Board __instance, ref bool __result)
            {
                var app = __instance.mApp;
                if (!app.IsChallengeWithoutSeedBank() &&
                    !__instance.HasConveyorBeltSeedBank() &&
                    app.GameMode != GameMode.ChallengeIce &&
                    app.GameMode != GameMode.ChallengeZenGarden &&
                    app.GameMode != GameMode.TreeOfWisdom &&
                    app.GameMode != GameMode.ChallengeBeghouled &&
                    app.GameMode != GameMode.ChallengeBeghouledTwist &&
                    app.GameMode != GameMode.ChallengeZombiquarium &&
                    !app.IsIZombieLevel() &&
                    !app.IsSquirrelLevel() &&
                    !app.IsSlotMachineLevel())
                {
                    __result = true;
                }
                else
                {
                    __result = false;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.MouseDownButterUpZombie))]
        public static class ButterPatch
        {
            private static bool Prefix(Board __instance)
            {
                if (!APClient.HasShovel())
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(CutScene), nameof(CutScene.ShowShovel))]
        public static class CutSceneShowShovelPatch
        {
            private static bool Prefix(CutScene __instance)
            {
                GameplayActivity app = __instance.mApp;
                if (!app.IsWhackAZombieLevel() &&
                    !app.IsWallnutBowlingLevel() &&
                    app.GameMode != GameMode.ChallengeBeghouled &&
                    app.GameMode != GameMode.ChallengeBeghouledTwist &&
                    app.GameMode != GameMode.ChallengeZenGarden &&
                    app.GameMode != GameMode.TreeOfWisdom &&
                    app.GameMode != GameMode.ChallengeZombiquarium &&
                    !app.IsIZombieLevel())
                {
                    if (APClient.HasShovel())
                    {
                        __instance.mBoard.mShowShovel = true;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(GamepadCursorController), nameof(GamepadCursorController.Update))]
        public class ControllerShovelPatch
        {
            private static void Postfix(GamepadCursorController __instance)
            {
                if (__instance.m_canShovel && !APClient.HasShovel())
                {
                    __instance.m_canShovel = false;
                }

                if (__instance.m_canButter && !APClient.HasShovel())
                {
                    __instance.m_canButter = false;
                }
            }
        }

        [HarmonyPatch(typeof(BackgroundController), nameof(BackgroundController.EnableBowlingLine))]
        public static class BowlingPatch
        {
            private static void Postfix(BackgroundController __instance)
            {
                if (__instance.m_board.mLevel == 5)
                {
                    __instance.m_bowlingLine.SetActive(true); //Restore bowling line
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.HasNotCompletedFirstTimeAdventureLevel))] //Triggers when starting a new level
        public static class HasNotCompletedFirstTimeAdventureLevelPatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(CutScene), nameof(CutScene.StartLevelIntro))]
        public class LevelIntroPatch
        {
            private static void Postfix(CutScene __instance)
            {
                int[] bannedCrazyDaveDialogs = { 1401, 1501, 1551 };
                if (bannedCrazyDaveDialogs.Contains(__instance.mCrazyDaveDialogStart))
                {
                    __instance.mCrazyDaveDialogStart = -1;
                    __instance.mCrazyDaveTime = 0;
                    if (__instance.IsNonScrollingCutscene())
                    {
                        __instance.CancelIntro();
                    }
                    __instance.mApp.Music.MakeSureMusicIsPlaying(Il2CppReloaded.Services.MusicTune.ChooseYourSeeds);
                }

                if (__instance.mBoard.mLevel == 5)
                {
                    __instance.mBoard.ShowShovel = false;
                }
            }
        }

        [HarmonyPatch(typeof(AwardScreenActivity), nameof(AwardScreenActivity.ActiveStarted))] //Disable the award screen for puzzles
        public static class ShowAwardScreenPatch
        {
            private static bool Prefix(AwardScreenActivity __instance)
            {
                if (Data.GameModeLevelIDs.ContainsKey(__instance.m_gameplayActivity.GameMode) && Data.GameModeLevelIDs[__instance.m_gameplayActivity.GameMode] >= 71 && Data.GameModeLevelIDs[__instance.m_gameplayActivity.GameMode] < 89)
                {
                    StateTransitionUtils.Transition("Puzzle");
                    return false;
                }
                return true;
            }
        }
    }
}
