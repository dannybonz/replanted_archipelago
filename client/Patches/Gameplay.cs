using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Il2Cpp;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
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
                if (Main.currentScene == "Gameplay" && __instance != null && Menu.showAwardScreen && __instance.m_board == null)
                {
                    AwardScreen.EditAwardScreen(__instance);
                }

                if (Main.currentScene != "Gameplay" || __instance == null || __instance.m_board == null || !(__instance.GameScene == GameScenes.Playing || __instance.GameScene == GameScenes.LevelIntro))
                {
                    return;
                }

                if (APClient.chooserRefreshState == "toggle" && __instance.m_crazyDaveService.CrazyDaveState == CrazyDaveState.Off)
                {
                    __instance.ShowSeedChooserScreen();
                    APClient.chooserRefreshState = "none";
                }

                if (APClient.deathlinkEnabled && APClient.receivedDeathLink != null)
                {
                    string deathMessage = $"DeathLink sent by {APClient.receivedDeathLink.Source}";
                    Main.Log(deathMessage);
                    if (APClient.receivedDeathLink.Cause != "")
                    {
                        deathMessage = $"DeathLink: {APClient.receivedDeathLink.Cause}";
                    }
                    __instance.m_board.mCutScene.StartZombiesLost(deathMessage);
                    TimeUtil.SetFlowingTimeScale(0f);
                    APClient.receivedDeathLink = null;
                    return;
                }

                Profile.ProcessIUserService(__instance.m_userService);

                //Cheat keys
                Board board = __instance.m_board; //Represents the lawn and its contents
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

                    //Refresh all packets - F5
                    if (Input.GetKeyDown(KeyCode.F5))
                    {
                        board.SeedBanks[0].RefreshAllPackets();
                    }

                    //Instant death - F6
                    if (Input.GetKeyDown(KeyCode.F6))
                    {
                        board.mCutScene.StartZombiesLost("Death Triggered");
                        TimeUtil.SetFlowingTimeScale(0f);
                    }

                    //Spawn wave - F7
                    if (Input.GetKeyDown(KeyCode.F7))
                    {
                        board.SpawnZombieWave();
                    }

                    //Bombs - F8
                    if (Input.GetKeyDown(KeyCode.F8))
                    {
                        board.AddPlant(1, 0, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(1, 2, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(1, 4, SeedType.Cherrybomb, SeedType.Cherrybomb);

                        board.AddPlant(2, 0, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(2, 2, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(2, 4, SeedType.Cherrybomb, SeedType.Cherrybomb);

                        board.AddPlant(4, 0, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(4, 2, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(4, 4, SeedType.Cherrybomb, SeedType.Cherrybomb);

                        board.AddPlant(6, 0, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(6, 2, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(6, 4, SeedType.Cherrybomb, SeedType.Cherrybomb);

                        board.AddPlant(8, 0, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(8, 2, SeedType.Cherrybomb, SeedType.Cherrybomb);
                        board.AddPlant(8, 4, SeedType.Cherrybomb, SeedType.Cherrybomb);
                    }
                }

                if (board.mTutorialState != TutorialState.Off)
                {
                    board.SetTutorialState(TutorialState.Off);
                }

                if (__instance.GameScene == GameScenes.LevelIntro && __instance.Board.mSeedBank.mIsChoosing)
                {
                    if (APClient.rechargeTimes.Count > 0)
                    {
                        Menu.AddCustomTooltips();
                    }

                    if (APClient.preferredSeeds.Count > 0 || board.SeedBanks[0].mSeedPackets[0].mPacketType != SeedType.None)
                    {
                        Menu.RepickUI.Activate();
                        if (Input.GetKeyDown(KeyCode.R) || Menu.RepickUI.repickRequested || Input.GetKeyDown(KeyCode.JoystickButton3))
                        {
                            Menu.RepickUI.repickRequested = false;
                            if (board.SeedBanks[0].mSeedPackets[0].mPacketType != SeedType.None) //If there are already seeds in the bank, empty it instead
                            {
                                foreach (ChosenSeed seed in __instance.m_seedChooserScreen.mChosenSeeds)
                                {
                                    if (seed.mSeedState == ChosenSeedState.SeedInBank)
                                    {
                                        __instance.m_seedChooserScreen.ClickedSeedInBank(seed, 0);
                                        __instance.m_seedChooserScreen.LandAllFlyingSeeds();
                                    }
                                }
                            }
                            else //Otherwise auto re-pick
                            {
                                foreach (SeedType seedType in APClient.preferredSeeds)
                                {
                                    if (seedType != SeedType.None)
                                    {
                                        ChosenSeed seed = __instance.m_seedChooserScreen.GetChosenSeedFromType(seedType);
                                        __instance.m_seedChooserScreen.ClickedSeedInChooser(seed, 0);
                                        __instance.m_seedChooserScreen.LandAllFlyingSeeds();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Menu.RepickUI.Hide();
                    }
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
                        if (board.HasConveyorBeltSeedBank() == false && __instance.GameMode != GameMode.ChallengeBeghouled && __instance.GameMode != GameMode.ChallengeBeghouledTwist) //Don't trigger if playing a conveyor belt level (causes weird issues)
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
                    Menu.showAwardScreen = false;
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                    if (levelId != -1 && APClient.scoutedLocations != null && Data.AllLevelLocations.ContainsKey(levelId) && !APClient.apSession.Locations.AllLocationsChecked.Contains(Data.AllLevelLocations[levelId].ClearLocation))
                    {
                        ItemInfo itemInfo = APClient.scoutedLocations[Data.AllLevelLocations[levelId].ClearLocation];
                        if (itemInfo.Player.Name == APClient.slot && Data.itemIdSpriteName.ContainsKey(itemInfo.ItemId))
                        {
                            theCoinType = CoinType.FinalSeedPacket;
                            Menu.showAwardScreen = true;
                            return true;
                        }
                    }
                    __instance.FadeOutLevel();
                    return false;
                }
                else if (theCoinType == CoinType.PresentMinigames || theCoinType == CoinType.PresentPuzzleMode || theCoinType == CoinType.PresentSurvivalMode)
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Coin), nameof(Coin.Draw))] //Change the image drawn for FinalSeedPacket
        public static class CoinDrawPatch
        {
            private static void Prefix(Coin __instance)
            {
                if (__instance.mType == CoinType.FinalSeedPacket)
                {
                    ItemInfo itemInfo = APClient.GetLevelCompleteAward(__instance.mApp);

                    if (itemInfo.ItemId == Data.itemIds["Shovel"])
                    {
                        __instance.mType = CoinType.Shovel;
                    }
                    else if (itemInfo.ItemId == Data.itemIds["Almanac"])
                    {
                        __instance.mType = CoinType.Almanac;
                    }
                    else if (itemInfo.ItemId == Data.itemIds["Crazy Dave's Car Keys"])
                    {
                        __instance.mType = CoinType.CarKeys;
                    }
                    else if (itemInfo.ItemId == Data.itemIds["Zen Garden"])
                    {
                        __instance.mType = CoinType.WateringCan;
                    }
                    else if (Data.itemIdSpriteName.ContainsKey(itemInfo.ItemId))
                    {
                        __instance.mController.m_plantImage.sprite = Data.FindSpriteByName(Data.itemIdSpriteName[itemInfo.ItemId]);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.FadeOutLevel))] //Triggers on level complete
        public static class FadeOutPatch
        {
            private static void Prefix(Board __instance)
            {
                if ((!__instance.mApp.IsScaryPotterLevel() || __instance.IsFinalScaryPotterStage()) && (!__instance.mApp.IsSurvivalMode() || __instance.IsFinalSurvivalStage()) && !__instance.IsLastStandStageWithRepick())
                {
                    APClient.CompletedLevel(Data.GetLevelIdFromGameplayActivity(__instance.mApp));
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
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.ActiveStarted))]
        public class NewGameplayActivityPatch
        {
            private static void Postfix(GameplayActivity __instance)
            {
                Main.cachedGameplayActivity = __instance;
                Menu.showAwardScreen = false;
                Main.Log("Re-cached GameplayActivity.");
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
                if (!__instance.mApp.IsCoopMode() && !__instance.mApp.IsVersusMode() && !__instance.mApp.IsIZombieLevel() && !__instance.mApp.IsScaryPotterLevel() && !__instance.mApp.IsWhackAZombieLevel() && !__instance.mApp.IsChallengeWithoutSeedBank() && !__instance.HasConveyorBeltSeedBank() && __instance.mApp.GameMode != GameMode.ChallengeBeghouled && __instance.mApp.GameMode != GameMode.ChallengeBeghouledTwist && __instance.mApp.GameMode != GameMode.ChallengeZombiquarium && __instance.mApp.GameMode != GameMode.ChallengeSlotMachine)
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
                GameplayActivity app = __instance.mApp;
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

        [HarmonyPatch(typeof(StormyNightLightningController), nameof(StormyNightLightningController._setAlpha))] //Disable storm flashes
        public static class DrawStormPatch
        {
            private static bool Prefix()
            {
                return !APClient.disableStormFlashes;
            }
        }

        [HarmonyPatch(typeof(SeedBankEntryModel), nameof(SeedBankEntryModel.HasUpgradeablePlants))]
        public static class HasUpgradeablePlantsPatch
        {
            private static bool Prefix(SeedBankEntryModel __instance, ref bool __result)
            {
                if (APClient.easyUpgradePlants)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.IsUpgrade))]
        public static class PlantUpgradePatch
        {
            private static bool Prefix(SeedType theSeedtype, ref bool __result)
            {
                if (APClient.easyUpgradePlants)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.IsUpgradableTo))]
        public static class PlantIsUpgradableToPatch
        {
            private static bool Prefix(SeedType aUpdatedType, ref bool __result)
            {
                if (APClient.easyUpgradePlants)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.IsPartOfUpgradableTo))]
        public static class PlantIsPartOfUpgradableToPatch
        {
            private static bool Prefix(SeedType aUpdatedType, ref bool __result)
            {
                if (APClient.easyUpgradePlants)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.PlantingRequirementsMet))]
        public static class RequirementsMetPatch
        {
            private static bool Prefix(Board __instance, SeedType theSeedType, ref bool __result)
            {
                if (APClient.easyUpgradePlants)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.CanPlantAt))] //The final check before letting you plop a plant down
        public static class CanPlantAtPatch
        {
            private static void Postfix(Board __instance, int theGridX, int theGridY, SeedType theType, ref PlantingReason __result)
            {
                if (APClient.easyUpgradePlants && __result == PlantingReason.Ok)
                {
                    if (theType == SeedType.Cobcannon)
                    {
                        if ((__instance.CanPlantAt(theGridX + 1, theGridY, SeedType.Kernelpult) != PlantingReason.Ok) ||
                            (__instance.GetPlantsOnLawn(theGridX, theGridY).PumpkinPlant != null) ||
                            (__instance.GetPlantsOnLawn(theGridX + 1, theGridY).PumpkinPlant != null))
                        {
                            __result = PlantingReason.NotHere;
                        }
                    }
                    else if (theType == SeedType.Cattail)
                    {
                        __result = __instance.CanPlantAt(theGridX, theGridY, SeedType.Lilypad);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CutScene), nameof(CutScene.StartZombiesLost))]
        public static class ZombiesLostPatch
        {
            private static void Postfix()
            {
                if (APClient.deathlinkEnabled && APClient.deathLinkService != null && APClient.receivedDeathLink == null)
                {
                    DeathLink deathLink = new DeathLink(APClient.slot);
                    APClient.deathLinkService.SendDeathLink(deathLink);
                }
            }
        }

        [HarmonyPatch(typeof(Zombie), nameof(Zombie.WalkIntoHouse))]
        public static class ZombieWalkIntoHousePatch
        {
            private static void Postfix(Zombie __instance)
            {
                if (APClient.deathlinkEnabled && APClient.deathLinkService != null)
                {
                    string messageEnding = "";
                    if (Data.zombieTypeNames.ContainsKey(__instance.mZombieType))
                    {
                        messageEnding = $" to a {Data.zombieTypeNames[__instance.mZombieType]}";
                    }
                    DeathLink deathLink = new DeathLink(APClient.slot, $"{APClient.slot} lost their brains{messageEnding}!");
                    APClient.deathLinkService.SendDeathLink(deathLink);
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.GetAwardSeedForLevel))]
        public static class GetAwardSeedPatch
        {
            private static void Postfix(GameplayActivity __instance, ref SeedType __result)
            {
                ItemInfo itemInfo = APClient.GetLevelCompleteAward(__instance);
                if (itemInfo != null)
                {
                    long itemId = itemInfo.ItemId;
                    if (itemId >= 100 && itemId < 200)
                    {
                        __result = Data.seedTypes[itemId - 100];
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.CanZombieSpawnOnLevel))]
        public static class CanZombieSpawnOnLevelPatch
        {
            private static bool Prefix(Board __instance, ZombieType theZombieType, int theLevel, ref bool __result)
            {
                int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                if (levelId != -1 && APClient.zombieMap.ContainsKey(levelId.ToString()))
                {
                    int zombieIndex = Array.FindIndex(Data.zombieTypes, zombieType => zombieType == theZombieType);
                    __result = APClient.zombieMap[levelId.ToString()].Any(includedZombie => includedZombie.Value<int>() == zombieIndex);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.GetZombieDefinition))]
        public static class GetZombieDefinitionPatch
        {
            private static void Postfix(GameplayActivity __instance, ref ZombieDefinition __result)
            {
                __result.m_firstLevel = -1;
                if (__result.ZombieType == ZombieType.PeaHead)
                {
                    if (__instance.GameMode == GameMode.ChallengeWarAndPeas || __instance.GameMode == GameMode.ChallengeWarAndPeas2)
                    {
                        __result.m_value = 1; //Default Peahead weight
                    }
                    else
                    {
                        __result.m_value = 3; //Re-weights Peahead to not be so pervasive with Zombie rando enabled - we could also use this in future for Zombie value rando
                    }

                }
                else if (__result.ZombieType == ZombieType.TrashCan)
                {
                    if (__instance.GameMode == GameMode.Adventure)
                    {
                        __result.m_weight = 4000; //Makes Trash Can Zombie eligible to spawn
                    } else
                    {
                        __result.m_weight = 0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.StartLevel))]
        public static class StartLevelPatch
        {
            private static void Postfix(Board __instance)
            {
                if (__instance.ChooseSeedsOnCurrentLevel())
                {
                    APClient.preferredSeeds = new System.Collections.Generic.List<SeedType>();
                    foreach (SeedPacket seedPacket in __instance.SeedBanks[0].mSeedPackets)
                    {
                        if (seedPacket.mImitaterType == SeedType.None)
                        {
                            APClient.preferredSeeds.Add(seedPacket.mPacketType);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.GetPlantDefinition))]
        public static class GetPlantDefinitionPatch
        {
            private static void Postfix(GameplayActivity __instance, ref PlantDefinition __result)
            {
                if (__result != null && APClient.sunPrices.Count > 0 && __instance.Board != null && __instance.Board.ChooseSeedsOnCurrentLevel())
                {
                    SeedType theSeedType = __result.m_seedType;
                    if (Data.plantStats.ContainsKey(theSeedType))
                    {
                        Data.PlantStats theStats = Data.plantStats[theSeedType];
                        __result.m_seedCost = theStats.Cost;
                        __result.m_refreshTime = theStats.Refresh;
                        __result.m_launchRate = theStats.Rate;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.GetProjectileDefinition))]
        public static class GetProjectileDefinitionPatch
        {
            private static void Postfix(GameplayActivity __instance, ref ProjectileDefinition __result)
            {
                if (__result != null && APClient.projectileDamages.Count > 0 && __instance.Board != null && __instance.Board.ChooseSeedsOnCurrentLevel())
                {
                    ProjectileType theProjectileType = __result.m_projectileType;
                    if (Data.projectileTypes.Contains(theProjectileType))
                    {
                        string projectileIndex = Array.FindIndex(Data.projectileTypes, projectileType => projectileType == theProjectileType).ToString();
                        if (APClient.projectileDamages.ContainsKey(projectileIndex))
                        {
                            __result.m_damage = (int)APClient.projectileDamages[projectileIndex];
                        }
                    }
                    else if (theProjectileType == ProjectileType.Fireball && APClient.projectileDamages.ContainsKey("0"))
                    {
                        __result.m_damage = ((int)APClient.projectileDamages[0]) * 2;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.PlantInitialize))]
        public static class PlantInitializePatch
        {
            private static void Postfix(Plant __instance)
            {
                if (APClient.plantHealths.Count > 0 && __instance.mBoard != null && __instance.mSeedType != null && __instance.mBoard.ChooseSeedsOnCurrentLevel())
                {
                    SeedType theSeedType = __instance.mSeedType;
                    if (Data.plantStats.ContainsKey(theSeedType))
                    {
                        Data.PlantStats theStats = Data.plantStats[theSeedType];
                        __instance.mPlantMaxHealth = theStats.Health;
                        __instance.mPlantHealth = theStats.Health;
                    }
                }
            }
        }
    }
}
