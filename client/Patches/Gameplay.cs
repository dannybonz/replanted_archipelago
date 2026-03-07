using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Il2Cpp;
using Il2CppBest.HTTP.Shared.Extensions;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using Il2CppSource.Controllers;
using Il2CppSource.Utils;
using Il2CppTMPro;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReplantedArchipelago.Patches
{
    public class Gameplay
    {
        public static float displayingSeedStatsTime = 0;
        public static int displayingSeedStatsIndex = -1;
        public static int queuedMowerCoins = 0;
        public static List<int> queuedFlagItems = new List<int>();
        public static int currentLevelId = -1;
        public static List<int> spawnedFlagItems = new List<int>();
        public static bool showAwardScreen = false;

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.ActiveUpdate))] //Runs every frame during gameplay
        public class GameplayActivityUpdatePatch
        {
            private static void Postfix(GameplayActivity __instance)
            {
                if (Main.currentScene == "Gameplay" && __instance != null && __instance.m_board == null)
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

                if (__instance.GameMode == GameMode.TreeOfWisdom && !APClient.receivedItems.Contains(2))
                {
                    GameObject shopButton = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center/P_Zen_TopBar/KMBButtons/TutorialShopContainer");
                    if (shopButton != null)
                    {
                        shopButton.SetActive(false);
                    }
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

                    //Exit level - F4
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

                    //Spawn Seed - F9
                    if (Input.GetKeyDown(KeyCode.F9))
                    {
                        int xPos = Data.random.Next(100, 650);
                        int yPos = Data.random.Next(60, 500);
                        Coin droppedSeed = __instance.m_board.AddCoin(xPos, yPos, CoinType.UsableSeedPacket, CoinMotion.FromPlant);

                        droppedSeed.mUsableSeedType = Data.GetFreeSeedType(board);
                        __instance.m_audioService.PlaySample(Il2CppReloaded.Constants.Sound.SOUND_SEEDLIFT);
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
                                        if (!__instance.m_seedChooserScreen.SeedNotAllowedToPick(seedType))
                                        {
                                            ChosenSeed seed = __instance.m_seedChooserScreen.GetChosenSeedFromType(seedType);
                                            __instance.m_seedChooserScreen.ClickedSeedInChooser(seed, 0);
                                            __instance.m_seedChooserScreen.LandAllFlyingSeeds();
                                        }
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
                if (APClient.rechargeTimes.Count > 0) //Show tooltips in gameplay
                {
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance);
                    if (levelId != -1 && (board.ChooseSeedsOnCurrentLevel() || APClient.conveyorMap.ContainsKey(levelId.ToString())))
                    {
                        GameObject seedBankObject;
                        bool hasConveyor = board.HasConveyorBeltSeedBank();
                        if (hasConveyor)
                        {
                            seedBankObject = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center/ConveyorSeedBank/ConveyorContainerP1/P_ConveyorSeedBank/Mask");
                        }
                        else
                        {
                            seedBankObject = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center/TopLeftLayout/SeedBankContainer/SeedBank/SeedPacks_Layout");
                        }
                        if (seedBankObject != null)
                        {
                            Transform seedBank = seedBankObject.transform;
                            Camera worldCamera = seedBank.GetComponentInParent<Canvas>().worldCamera;
                            Vector3 mousePos = Input.mousePosition;

                            int seedIndex = 0;
                            for (int i = 0; i < seedBank.childCount; i++)
                            {
                                Transform seedTransform = seedBank.GetChild(i);
                                if (!seedTransform.name.Contains("P_GamePlay_SeedChooser_Item(Clone)"))
                                    continue;

                                RectTransform rt = seedTransform.Find("Offset/SeedBackground").GetComponent<RectTransform>();
                                GameObject tooltipObject = seedTransform.Find("Offset/ToolTip").gameObject;
                                bool isGamepad = board.SeedBanks[0].SeedPackets[seedIndex].IsGamepadSelected;
                                bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(rt, mousePos, worldCamera);
                                if (isGamepad || isHovering)
                                {
                                    SeedType theSeedType = board.SeedBanks[0].SeedPackets[seedIndex].mPacketType;
                                    if (!isHovering && displayingSeedStatsIndex == seedIndex && Time.time - displayingSeedStatsTime > 4)
                                    {
                                        tooltipObject.SetActive(false);
                                    }

                                    if (displayingSeedStatsIndex != seedIndex && Data.seedTypes.Contains(theSeedType))
                                    {
                                        int plantIndex = Array.FindIndex(Data.seedTypes, seedType => seedType == theSeedType);
                                        if (Data.plantStats.ContainsKey(Data.seedTypes[plantIndex]))
                                        {
                                            tooltipObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = Data.plantNames[plantIndex];
                                            if (hasConveyor)
                                            {
                                                tooltipObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = Data.plantStats[Data.seedTypes[plantIndex]].ConveyorStatsString;
                                            }
                                            else
                                            {
                                                tooltipObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = Data.plantStats[Data.seedTypes[plantIndex]].StatsString;
                                            }
                                            tooltipObject.SetActive(true);
                                        }
                                        displayingSeedStatsTime = Time.time;
                                        displayingSeedStatsIndex = seedIndex;
                                    }
                                }
                                else if (tooltipObject.activeSelf)
                                {
                                    displayingSeedStatsIndex = -1;
                                    tooltipObject.SetActive(false);
                                }
                                seedIndex++;
                            }
                        }
                    }
                }

                if (APClient.queuedUpItemEffects.Count > 0 && __instance.GameScene == GameScenes.Playing && !board.HasLevelAwardDropped() && !__instance.IsIZombieLevel() && (board.mBackground == BackgroundType.Day || board.mBackground == BackgroundType.Night || board.mBackground == BackgroundType.Pool || board.mBackground == BackgroundType.Fog || board.mBackground == BackgroundType.Roof || board.mBackground == BackgroundType.China || board.mBackground == BackgroundType.Boss))
                {
                    foreach (int itemId in APClient.queuedUpItemEffects)
                    {
                        if (itemId == 64) //Random seed packet
                        {
                            int xPos = Data.random.Next(100, 650);
                            int yPos = Data.random.Next(60, 500);
                            Coin droppedSeed = __instance.m_board.AddCoin(xPos, yPos, CoinType.UsableSeedPacket, CoinMotion.FromPlant);

                            droppedSeed.mUsableSeedType = Data.GetFreeSeedType(board);
                            __instance.m_audioService.PlaySample(Il2CppReloaded.Constants.Sound.SOUND_SEEDLIFT);
                        }
                        else if (itemId == 71) //Seed Packet Cooldown Trap
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
                        else if (itemId == 70) //Mower Deploy Trap
                        {
                            for (int i = 0; i < board.m_lawnMowers.Count; i++)
                            {
                                board.m_lawnMowers[i].StartMower();
                            }
                        }
                        else if (itemId == 72) //Zombie Ambush Trap
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
                        if (itemId == 50) //mustache
                        {
                            __instance.UserService.ActiveUserProfile.mMustacheModeActive = true;
                            board.SetMustacheMode(true);
                        }
                        else if (itemId == 51) //future
                        {
                            __instance.UserService.ActiveUserProfile.mFutureModeActive = true;
                            board.SetFutureMode(true);
                        }
                        else if (itemId == 52) //trickedout
                        {
                            __instance.UserService.ActiveUserProfile.mTrickedOutModeActive = true;
                            board.SetSuperMowerMode(true);
                        }
                        else if (itemId == 53) //daisies
                        {
                            __instance.UserService.ActiveUserProfile.mDaisesModeActive = true;
                            board.SetDaisyMode(true);
                        }
                        else if (itemId == 54) //pinata
                        {
                            __instance.UserService.ActiveUserProfile.mPinataModeActive = true;
                            board.SetPinataMode(true);
                        }
                        else if (itemId == 55) //sukhbir
                        {
                            __instance.UserService.ActiveUserProfile.mSukhbirModeActive = true;
                            board.SetSukhbirMode(true);
                        }
                        else if (itemId == 56) //dance
                        {
                            __instance.UserService.ActiveUserProfile.mDanceModeActive = true;
                            board.SetDanceMode(true);
                        }
                    }
                    APClient.queuedUpItemEffects.Clear();
                }

                if (__instance.GameScene == GameScenes.Playing && Main.QueuedIngameMessages.Count > 0 && (board.mAdvice.mDuration == 0 || board.mAdvice.mMessageStyle == MessageStyle.BigMiddle) && !board.mLevelComplete) //If there are queued up AP messages to display
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

        [HarmonyPatch(typeof(Board), nameof(Board.AddCoin))] //Triggers when a Coin/loot is spawned
        public static class AddCoinPatch
        {
            private static bool Prefix(Board __instance, ref float theX, ref float theY, ref CoinType theCoinType, ref CoinMotion theCoinMotion)
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
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                    if (levelId != -1 && APClient.scoutedLocations != null && Data.AllLevelLocations.ContainsKey(levelId) && !APClient.apSession.Locations.AllLocationsChecked.Contains(Data.AllLevelLocations[levelId].ClearLocation) && !Data.SkipAwardScreen)
                    {
                        ItemInfo itemInfo = APClient.scoutedLocations[Data.AllLevelLocations[levelId].ClearLocation];
                        bool isForMe = itemInfo.Player.Slot == APClient.apSession.Players.ActivePlayer.Slot;
                        if (isForMe)
                        {
                            if (Data.awardCoinTypes.ContainsKey(itemInfo.ItemId))
                            {
                                theCoinType = Data.awardCoinTypes[itemInfo.ItemId];
                            }
                            else if (itemInfo.ItemId >= 100 && itemInfo.ItemId < 200)
                            {
                                theCoinType = CoinType.FinalSeedPacket;
                            }
                            else
                            {
                                theCoinType = CoinType.Taco;
                            }
                        }
                        else if (!isForMe)
                        {
                            theCoinType = CoinType.Taco;
                        }
                        showAwardScreen = true;
                        return true;
                    }
                    else
                    {
                        __instance.FadeOutLevel();
                        return false;
                    }
                }
                else if ((theCoinType == CoinType.PresentMinigames && queuedFlagItems.Count == 0) || theCoinType == CoinType.PresentPuzzleMode || theCoinType == CoinType.PresentSurvivalMode || theCoinType == CoinType.Chocolate)
                {
                    return false;
                }
                else if (theCoinMotion == CoinMotion.LawnmowerCoin)
                {
                    if (queuedMowerCoins > 0) //This is our custom coin! Ignore it!
                    {
                        queuedMowerCoins -= 1; //...but get ready to pay attention to the next one
                    }
                    else if (APClient.receivedItems.Contains(19))
                    {
                        int coinsToAdd = APClient.receivedItems.Count(itemId => itemId == 19);
                        queuedMowerCoins += coinsToAdd; //Ignore this number of mower coins
                        for (int i = 0; i < coinsToAdd; i++)
                        {
                            __instance.AddCoin(theX + 40 + (40 * i), theY, CoinType.Gold, CoinMotion.LawnmowerCoin);
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Coin), nameof(Coin.Update))] //Change the image drawn for award coin
        public static class CoinUpdatePatch
        {
            private static void Postfix(Coin __instance)
            {
                if (__instance.mType == CoinType.PresentMinigames && __instance.mCoinAge >= 1000) //Auto-collect flag drops
                {
                    __instance.UpdateCollected();
                }

                if (__instance.mType == CoinType.PresentMinigames || __instance.mType == CoinType.Taco) //Update arrow position
                {
                    GameObject bouncyArrow = GameObject.Find("P_AwardPickupArrow(Clone)");
                    if (bouncyArrow != null)
                    {
                        bouncyArrow.transform.Find("BackGlow").localPosition = new Vector3(0, -185, 0);
                        bouncyArrow.transform.Find("Arrow").localPosition = new Vector3(0, -185, 0);
                    }
                }

                if (__instance.mUsableSeedType == SeedType.BeghouledButtonShuffle) //Already modified
                {
                    return;
                }
                else if (__instance.mType == CoinType.FinalSeedPacket) //Seed Packet
                {
                    ItemInfo itemInfo = APClient.GetLevelCompleteAward(__instance.mApp);
                    if (itemInfo.ItemId >= 100 && itemInfo.ItemId < 200 && Graphics.itemIdSpriteName.ContainsKey(itemInfo.ItemId))
                    {
                        __instance.mController.m_plantImage.sprite = Graphics.GetGraphic(Graphics.itemIdSpriteName[itemInfo.ItemId]);
                    }
                }
                else if (__instance.mType == CoinType.Taco)
                {
                    var (desiredSprite, scaler) = Graphics.GetSpriteAndScaleForItemDrop(APClient.GetLevelCompleteAward(__instance.mApp));

                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    var potentialTacos = allObjects.Where(obj => obj.name == "Coin_Taco").ToArray();
                    GameObject tacoObject = potentialTacos.FirstOrDefault(potentialTaco => potentialTaco.activeSelf);
                    if (tacoObject != null && tacoObject.activeSelf)
                    {
                        SpriteRenderer spriteRenderer = tacoObject.GetComponent<SpriteRenderer>();
                        spriteRenderer.sprite = desiredSprite;
                        tacoObject.transform.localScale = new Vector3(scaler, scaler, 1);
                        __instance.mUsableSeedType = SeedType.BeghouledButtonShuffle; //Marks the coin as adjusted so we don't need to update it again
                    }
                }
                else if (__instance.mType == CoinType.PresentMinigames && queuedFlagItems.Count > 0 && __instance.mUsableSeedType != SeedType.BeghouledButtonShuffle) //Flag items
                {
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                    int queuedFlagNumber = queuedFlagItems[queuedFlagItems.Count - 1];
                    long locationId = Data.AllLevelLocations[levelId].FlagLocations[queuedFlagNumber - 1];
                    var (desiredSprite, scaler) = Graphics.GetSpriteAndScaleForItemDrop(APClient.scoutedLocations[locationId]);

                    if (desiredSprite != null)
                    {
                        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                        var potentialPresents = allObjects.Where(obj => obj.name == "Coin_Present").ToArray();
                        GameObject presentObject = potentialPresents.FirstOrDefault(potentialPresent => potentialPresent.activeSelf);
                        if (presentObject != null && presentObject.activeSelf)
                        {
                            SpriteRenderer spriteRenderer = presentObject.GetComponent<SpriteRenderer>();
                            spriteRenderer.sprite = desiredSprite;
                            presentObject.transform.localScale = new Vector3(scaler, scaler, 1);
                            __instance.mUsableSeedType = SeedType.BeghouledButtonShuffle;
                            queuedFlagItems.Remove(queuedFlagNumber);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Coin), nameof(Coin.UpdateCollected))] //When clicking a flag drop reward, send the check
        public static class CoinUpdateCollectedPatch
        {
            private static bool Prefix(Coin __instance)
            {
                if (__instance.mType == CoinType.PresentMinigames)
                {
                    int clearedWaves = __instance.mBoard.mCurrentWave;
                    int wavesPerFlag = __instance.mBoard.GetNumWavesPerFlag();
                    int completedFlags = clearedWaves / wavesPerFlag;
                    APClient.SendWaveLocation(__instance.mApp.LevelData, completedFlags - 1);
                    __instance.Die();
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.FadeOutLevel))] //Triggers on level complete
        public static class FadeOutPatch
        {
            private static void Prefix(Board __instance)
            {
                if ((!__instance.mApp.IsScaryPotterLevel() || __instance.IsFinalScaryPotterStage()) && (!__instance.mApp.IsSurvivalMode() || __instance.IsFinalSurvivalStage()) && (__instance.mApp.GameMode != GameMode.ChallengeLastStand || __instance.IsLastStandFinalStage()))
                {
                    APClient.CompletedLevel(Data.GetLevelIdFromGameplayActivity(__instance.mApp));
                }
                else if ((__instance.mApp.IsSurvivalMode() && !__instance.IsFinalSurvivalStage()) || (__instance.mApp.GameMode == GameMode.ChallengeLastStand && !__instance.IsLastStandFinalStage()))
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
                if (APClient.musicMap.Count > 0 && __instance.m_currentMusicTune != MusicTune.None && __instance.m_currentMusicTune != MusicTune.ZenGarden)
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

        [HarmonyPatch(typeof(Zombie), nameof(Zombie.UpdateZombieWalking))]
        public static class ZombieStartWalkPatch
        {
            private static void Postfix(Zombie __instance)
            {
                if (__instance.mZombieType == ZombieType.TrashCan && !__instance.mIsEating && __instance.mIceTrapCounter == 0 && __instance.mButteredCounter == 0)
                {
                    float speedIncrease = 0.02f;
                    if (__instance.mPosX > 500 && !__instance.IsWalkingBackwards())
                    {
                        speedIncrease = 0.02f + (0.07f * ((__instance.mPosX - 500) / 300)); //Gradually slows down as it gets towards the middle
                    }

                    if (__instance.IsMovingAtChilledSpeed())
                    {
                        speedIncrease *= 0.5f;
                    }
                    if (__instance.IsWalkingBackwards())
                    {
                        __instance.mPosX += speedIncrease;
                    }
                    else
                    {
                        __instance.mPosX -= speedIncrease;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Zombie), nameof(Zombie.DieNoLoot))] //Triggers when Zombie is killed
        public static class ZombieDiePatch
        {
            private static void Postfix(Zombie __instance)
            {
                if (APClient.scoutedLocations.ContainsKey(2000)) //Flag locations are enabled
                {
                    int clearedWaves = __instance.mBoard.mCurrentWave;
                    int wavesPerFlag = __instance.mBoard.GetNumWavesPerFlag();
                    if (clearedWaves >= wavesPerFlag && clearedWaves < __instance.mBoard.mNumWaves && __instance.mApp.GameMode != GameMode.ChallengeLastStand && !__instance.mApp.IsSurvivalMode())
                    {
                        int completedFlags = clearedWaves / wavesPerFlag;
                        if (!spawnedFlagItems.Contains(completedFlags)) //First, check if we already spawned the item in this session
                        {
                            foreach (var coin in __instance.mBoard.m_coins.m_list) //Next, check if any presents already exist on the lawn
                            {
                                if (coin != null && coin.mItem != null && coin.mItem.mType != null && coin.mItem.mType == CoinType.PresentMinigames) //A present is already on the lawn
                                {
                                    return;
                                }
                            }

                            long locationId = Data.AllLevelLocations[currentLevelId].FlagLocations[completedFlags - 1]; //Finally, check if the location has already been sent
                            if (APClient.apSession.Locations.AllMissingLocations.Contains(locationId))
                            {
                                queuedFlagItems.Add(completedFlags);
                                spawnedFlagItems.Add(completedFlags);
                                Rect zombieRect = __instance.GetZombieRect();
                                __instance.mBoard.AddCoin(zombieRect.center[0], zombieRect.center[1], CoinType.PresentMinigames, CoinMotion.Coin);
                            }
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
                displayingSeedStatsTime = 0;
                displayingSeedStatsIndex = -1;
                currentLevelId = Data.GetLevelIdFromGameplayActivity(__instance);
                Main.Log("Re-cached GameplayActivity.");
                spawnedFlagItems.Clear();
                queuedFlagItems.Clear();
                showAwardScreen = false;
                if (__instance.GameMode == GameMode.ChallengeZenGarden)
                {
                    if (!APClient.receivedItems.Contains(2))
                    {
                        GameObject shopButton = GameObject.Find("Panels/P_ZenGarden_MainHUD/Canvas/Layout/Center/P_Zen_TopBar/KMBButtons/TutorialShopContainer");
                        shopButton.SetActive(false);
                        __instance.m_zenGarden._setTutorialDataState(false, false, false);
                    }
                }
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

                if (__instance.mBoard.ChooseSeedsOnCurrentLevel())
                {
                    __instance.mBoard.AddSunMoney(APClient.GetSunUpgradeAmount(), 0);
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

        [HarmonyPatch(typeof(Board), nameof(Board.CanZombieSpawnOnLevel))] //Randomise Zombies for Adventure Mode
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

        [HarmonyPatch(typeof(Challenge), nameof(Challenge.InitZombieWaves))] //Randomise Zombies for other modes
        public static class InitZombieWavesPatch
        {
            private static bool Prefix(Challenge __instance)
            {
                int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                if (levelId != -1 && APClient.zombieMap.ContainsKey(levelId.ToString()))
                {
                    foreach (int zombieIndex in APClient.zombieMap[levelId.ToString()])
                    {
                        __instance.mBoard.mZombieAllowed[(int)Data.zombieTypes[zombieIndex]] = true;
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Challenge), nameof(Challenge.InitLevel))] //Used to put starting seeds into conveyor belt
        public static class InitLevelPatch
        {
            private static void Postfix(Challenge __instance)
            {
                int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                if (levelId != -1 && APClient.conveyorMap.ContainsKey(levelId.ToString()))
                {
                    JToken defaultSeeds = APClient.conveyorMap[levelId.ToString()]["default"];
                    for (int i = 0; i < defaultSeeds.Count(); i++)
                    {
                        Main.Log($"{defaultSeeds[i]}");
                        int seedIndex = (int)defaultSeeds[i];
                        Main.Log($"{seedIndex}");
                        __instance.mBoard.SeedBanks[0].mSeedPackets[i].mPacketType = Data.seedTypes[seedIndex];
                    }
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
                    }
                    else
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
                        if (seedPacket.mImitaterType == SeedType.None && APClient.HasSeedType(seedPacket.PacketType))
                        {
                            APClient.preferredSeeds.Add(seedPacket.mPacketType);
                        }
                    }
                }
                if ((currentLevelId == 63 || currentLevelId == 69 || currentLevelId == 104) && APClient.zombieMap.ContainsKey(currentLevelId.ToString()))
                {
                    if (APClient.zombieMap[currentLevelId.ToString()].Any(includedZombie => includedZombie.Value<int>() == 23)) //Gargantuar
                    {
                        if (currentLevelId == 104)
                        {
                            __instance.mZombieCountDown = 18000;
                        }
                        else
                        {
                            __instance.mZombieCountDown = 12000;
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
                if (__result != null && APClient.sunPrices.Count > 0 && __instance.Board != null)
                {
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance);
                    SeedType theSeedType = __result.m_seedType;
                    if (Data.plantStats.ContainsKey(theSeedType))
                    {
                        Data.PlantStats theStats = Data.plantStats[theSeedType].OldStats;
                        if (__instance.Board.ChooseSeedsOnCurrentLevel() || APClient.conveyorMap.ContainsKey(levelId.ToString()))
                        {
                            theStats = Data.plantStats[theSeedType];
                        }
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
                if (__result != null && APClient.projectileDamages.Count > 0 && __instance.Board != null)
                {
                    ProjectileType theProjectileType = __result.m_projectileType;
                    if (Data.projectileTypes.Contains(theProjectileType))
                    {
                        int levelId = Data.GetLevelIdFromGameplayActivity(__instance);
                        if (__instance.Board.ChooseSeedsOnCurrentLevel() || APClient.conveyorMap.ContainsKey(levelId.ToString()))
                        {
                            string projectileIndex = Array.FindIndex(Data.projectileTypes, projectileType => projectileType == theProjectileType).ToString();
                            if (APClient.projectileDamages.ContainsKey(projectileIndex))
                            {
                                __result.m_damage = (int)APClient.projectileDamages[projectileIndex];
                            }
                            else if ((theProjectileType == ProjectileType.Fireball || theProjectileType == ProjectileType.PeashooterFireball) && APClient.projectileDamages.ContainsKey("0"))
                            {
                                __result.m_damage = ((int)APClient.projectileDamages["0"]) * 2;
                            }
                            else if (theProjectileType == ProjectileType.PeashooterPea && APClient.projectileDamages.ContainsKey("0"))
                            {
                                __result.m_damage = (int)APClient.projectileDamages["0"];
                            }
                        }
                        else if (Data.defaultProjectileDamages.ContainsKey(theProjectileType))
                        {
                            __result.m_damage = Data.defaultProjectileDamages[theProjectileType];
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.PlantInitialize))]
        public static class PlantInitializePatch
        {
            private static void Postfix(Plant __instance)
            {
                if (APClient.plantHealths.Count > 0 && __instance.mBoard != null && __instance.mSeedType != null)
                {
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                    if (__instance.mBoard.ChooseSeedsOnCurrentLevel() || APClient.conveyorMap.ContainsKey(levelId.ToString()))
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

        [HarmonyPatch(typeof(Challenge), nameof(Challenge.UpdateConveyorBelt))] //Conveyor Rando
        public static class UpdateConveyorBeltPatch
        {
            private static bool Prefix(Challenge __instance)
            {
                int levelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
                if (__instance.mConveyorBeltCounter > 1 || !APClient.conveyorMap.ContainsKey(levelId.ToString()))
                {
                    return true;
                }
                if (!__instance.mBoard.HasLevelAwardDropped())
                {
                    float conveyorSpeedMultiplier = 1;
                    if (__instance.mApp.IsFinalBossLevel())
                    {
                        conveyorSpeedMultiplier = 0.875f;
                    }
                    else if (__instance.mApp.IsShovelLevel() || __instance.mApp.GameMode == GameMode.ChallengePortalCombat)
                    {
                        conveyorSpeedMultiplier = 1.5f;
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeInvisighoul)
                    {
                        conveyorSpeedMultiplier = 2.0f;
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeColumn)
                    {
                        conveyorSpeedMultiplier = 3.0f;
                    }

                    int numSeedsOnConveyor = __instance.mBoard.mSeedBank.GetNumSeedsOnConveyorBelt();
                    float conveyorBeltCounter = conveyorSpeedMultiplier * (numSeedsOnConveyor > 8 ? 1000 : numSeedsOnConveyor > 6 ? 500 : numSeedsOnConveyor > 4 ? 425 : 400);
                    __instance.mConveyorBeltCounter = (int)conveyorBeltCounter;

                    JObject conveyorMap = (JObject)APClient.conveyorMap[levelId.ToString()]["weights"];
                    TodWeightedArray[] customSeeds = new TodWeightedArray[conveyorMap.Count];

                    int index = 0;
                    foreach (var conveyorMapSeed in conveyorMap)
                    {
                        int seedIndex = conveyorMapSeed.Key.ToInt32();
                        int seedWeight = (int)conveyorMapSeed.Value;
                        customSeeds[index].Item = (int)Data.seedTypes[seedIndex];
                        customSeeds[index].Weight = seedWeight;
                        index++;
                    }

                    for (int i = 0; i < customSeeds.Length; i++)
                    {
                        TodWeightedArray customSeed = customSeeds[i];
                        SeedType seedType = (SeedType)customSeed.Item;
                        int aCountInBank = __instance.mBoard.SeedBanks[0].CountOfTypeOnConveyorBelt(seedType);
                        int aTotalCount = __instance.mBoard.CountPlantByType(seedType) + aCountInBank;

                        if (seedType == SeedType.Gravebuster)
                        {
                            if (__instance.mBoard.GetGraveStoneCount() <= aTotalCount)
                            {
                                customSeeds[i].Weight = 0;
                            }
                        }
                        else if (seedType == SeedType.Lilypad)
                        {
                            customSeeds[i].Weight = Common.TodAnimateCurve(0, 18, aTotalCount, customSeed.Weight, 1, TodCurves.Linear);
                        }
                        else if (seedType == SeedType.Flowerpot)
                        {
                            customSeeds[i].Weight = Common.TodAnimateCurve(0, __instance.mApp.GameMode == GameMode.ChallengeColumn ? 45 : 35, aTotalCount, customSeed.Weight, 1, TodCurves.Linear);
                        }

                        if (__instance.mApp.IsFinalBossLevel())
                        {
                            if (seedType != SeedType.Jalapeno && seedType != SeedType.Iceshroom && seedType != SeedType.Flowerpot)
                            {
                                int emptyPots = __instance.mBoard.CountEmptyPotsOrLilies(SeedType.Flowerpot);
                                if (emptyPots <= 2)
                                {
                                    customSeeds[i].Weight /= 5;
                                }
                                else if (emptyPots <= 5)
                                {
                                    customSeeds[i].Weight /= 3;
                                }
                            }

                            if (seedType == SeedType.Flowerpot && __instance.mApp.IsFinalBossLevel())
                            {
                                Zombie boss = __instance.mBoard.GetBossZombie();
                                if (boss.mZombiePhase == ZombiePhase.BossDropRV)
                                {
                                    customSeeds[i].Weight = 500;
                                }
                            }
                        }

                        if (customSeeds.Length > 2)
                        {
                            if (aCountInBank >= 4)
                            {
                                customSeeds[i].Weight = 1;
                            }
                            else if (aCountInBank >= 3)
                            {
                                customSeeds[i].Weight = 5;
                            }
                            else if (seedType == __instance.mLastConveyorSeedType)
                            {
                                customSeeds[i].Weight /= 2;
                            }
                        }
                    }

                    SeedType theSeedType = (SeedType)Common.TodPickFromWeightedArray(customSeeds, customSeeds.Length);
                    __instance.mBoard.SeedBanks[0].AddSeed(theSeedType, false);
                    __instance.mLastConveyorSeedType = theSeedType;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.FindTargetZombie))]
        public static class FindTargetZombiePatch
        {
            private static bool Prefix(Plant __instance, ref Zombie __result)
            {
                if (__instance.mApp.GameMode == GameMode.ChallengePortalCombat && (__instance.mSeedType == SeedType.Scaredyshroom || __instance.mSeedType == SeedType.Snowpea || __instance.mSeedType == SeedType.Puffshroom || __instance.mSeedType == SeedType.Threepeater || __instance.mSeedType == SeedType.Gatlingpea))
                {
                    __result = null;
                    int zombieIndex = 0;
                    while (true)
                    {
                        try
                        {
                            Zombie testZombie = __instance.mBoard.m_zombies[zombieIndex];
                            if (__instance.mBoard.mChallenge.CanTargetZombieWithPortals(__instance, testZombie))
                            {
                                __result = testZombie;
                                break;
                            }
                        }
                        catch
                        {
                            break;
                        }
                        zombieIndex++;
                    }
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.AddPlant))]
        public static class AddPlantPatch
        {
            private static void Postfix(Board __instance)
            {
                if (__instance.HasConveyorBeltSeedBank())
                {
                    displayingSeedStatsIndex = -1;
                }
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.CheckForGameEnd))] //Checks if the level is over - if it is, decides what happens next
        public class GameEndPatch
        {
            private static bool Prefix(GameplayActivity __instance)
            {
                if (__instance.m_board != null && __instance.m_board.mLevelComplete)
                {
                    if (__instance.IsSurvivalMode() && !__instance.m_board.IsFinalSurvivalStage())
                    {
                        return true;
                    }

                    APClient.CompletedLevel(Data.GetLevelIdFromGameplayActivity(__instance)); //Re-send the completion check in case it was somehow missed until now

                    __instance.KillBoard();
                    if (showAwardScreen && !Data.SkipAwardScreen)
                    {
                        __instance.ShowAwardScreen();
                    }
                    else
                    {
                        StateTransitionUtils.Transition(Data.GetTransitionNameFromLevelId(Data.GetLevelIdFromGameplayActivity(__instance)));
                    }
                }
                return false;
            }
        }
    }
}
