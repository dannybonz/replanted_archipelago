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
using MelonLoader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static ReplantedArchipelago.Data;

namespace ReplantedArchipelago.Patches
{
    public class Gameplay
    {
        public static float displayingSeedStatsTime = 0;
        public static int displayingSeedStatsIndex = -1;
        public static int queuedMowerCoins = 0;
        public static bool showAwardScreen = false;
        public static int receivedRingLinkAmount = 0;
        public static int previousSunAmount = -1;
        public static bool redSunText = false;
        public static List<int> availableWaveLocations = new List<int>();
        public static List<int> allWavesanityLocations = new List<int>();
        public static List<SeedLink> queuedSeedLinks = new List<SeedLink>();
        public static List<LawnLink> queuedLawnLinks = new List<LawnLink>();
        public static DateTime resetLinkMessageAt;
        public static bool linkMessageActive = false;
        public static bool lawnLinkBlocked = false;
        public static int wavePointsAdjusted = -1;
        public static List<ZombieGrowingInformation> growingZombies = new List<ZombieGrowingInformation>();

        private static void StopJackInTheBoxFoley(Task task, object state)
        {
            ((GameplayActivity)state).m_audioService.StopFoley(FoleyType.JackInThebox);
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.ActiveUpdate))] //Runs every frame during gameplay
        public class GameplayActivityUpdatePatch
        {
            private static void Postfix(GameplayActivity __instance)
            {
                if (Main.currentScene == "Gameplay" && __instance != null && __instance.m_board == null)
                {
                    AwardScreen.EditAwardScreen(__instance);
                }

                if (__instance.GameMode == GameMode.ChallengeZenGarden)
                {
                    GameObject actionHud = GameObject.Find("Panels/P_ZenGarden_MainHUD/Canvas/Layout/Center/P_Zen_TopBar/ActionHud");
                    if (actionHud != null && actionHud.activeSelf)
                    {
                        actionHud.transform.GetChild(7).gameObject.SetActive(true); //Enable the MoneySign
                    }
                }

                if (Main.currentScene != "Gameplay" || __instance == null || __instance.m_board == null || !(__instance.GameScene == GameScenes.Playing || __instance.GameScene == GameScenes.LevelIntro) || __instance.m_board.mPaused)
                {
                    return;
                }

                if (APClient.chooserRefreshState == "toggle" && __instance.m_crazyDaveService.CrazyDaveState == CrazyDaveState.Off)
                {
                    __instance.ShowSeedChooserScreen();
                    APClient.chooserRefreshState = "none";
                }

                if (APClient.deathLinkEnabled && APClient.receivedDeathLink != null)
                {
                    string deathMessage = $"DeathLink sent by {APClient.receivedDeathLink.Source}";
                    if (APClient.receivedDeathLink.Cause != "")
                    {
                        deathMessage = $"DeathLink: {APClient.receivedDeathLink.Cause}";
                    }
                    __instance.m_board.mCutScene.StartZombiesLost(deathMessage);
                    TimeUtil.SetFlowingTimeScale(0f);
                    APClient.receivedDeathLink = null;
                    return;
                }

                if (__instance.GameMode == GameMode.TreeOfWisdom && !(APClient.receivedItems.Contains(2) || APClient.receivedItems.Contains(28)))
                {
                    GameObject shopButton = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center/P_Zen_TopBar/KMBButtons/TutorialShopContainer");
                    if (shopButton != null)
                    {
                        shopButton.SetActive(false);
                    }
                }

                if (APClient.queuedUpCoins > 0 || APClient.queuedUpPurchaseItems.Count > 0)
                {
                    Profile.ProcessUserService();
                }

                //Fix controller not picking up random seeds
                if (__instance.m_board.IsGamepadEnabled(0))
                {
                    bool touchingUsableSeedPacket = false;
                    for (int coinIndex = 0; coinIndex < __instance.m_board.m_coins.Count; coinIndex++)
                    {
                        Coin coin = __instance.m_board.m_coins[coinIndex];
                        if (coin.mType == CoinType.UsableSeedPacket)
                        {
                            if (Math.Abs(coin.mPosX - __instance.m_board.CursorObjects[0].mX) < 50 && Math.Abs(coin.mPosY - __instance.m_board.CursorObjects[0].mY) < 50)
                            {
                                touchingUsableSeedPacket = true;
                            }
                        }
                    }
                    if (touchingUsableSeedPacket == true)
                    {
                        if (__instance.m_board.CursorObjects[0].mCursorType == CursorType.PlantFromBank)
                        {
                            __instance.m_board.CursorObjects[0].mCursorType = CursorType.Normal;
                        }
                    }
                }

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

                    //Display LinkMessage - F3
                    if (Input.GetKeyDown(KeyCode.F3))
                    {
                        DisplayLinkMessage("Displaying Link Message", new UnityEngine.Color(1f, 1f, 1f), __instance);
                    }

                    //Delete top left plant - F4
                    if (Input.GetKeyDown(KeyCode.F4))
                    {
                        Plant plant = board.GetTopPlantAt(0, 0, PlantPriority.EatingOrder);
                        if (plant != null)
                        {
                            plant.Die();
                        }
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
                    if (APClient.plantStatRandomisationEnabled)
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

                if (APClient.plantStatRandomisationEnabled) //Show tooltips in gameplay
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
                                        if (PlantStatRando.plantStats.ContainsKey(Data.seedTypes[plantIndex]))
                                        {
                                            tooltipObject.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = Data.plantNames[plantIndex];
                                            if (hasConveyor)
                                            {
                                                tooltipObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = PlantStatRando.plantStats[Data.seedTypes[plantIndex]].ConveyorStatsString;
                                            }
                                            else
                                            {
                                                tooltipObject.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = PlantStatRando.plantStats[Data.seedTypes[plantIndex]].StatsString;
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

                //Ring Link
                if (__instance.GameScene == GameScenes.Playing && APClient.ringLinkEnabled)
                {
                    if (previousSunAmount != board.mSunMoney[0].Amount)
                    {
                        int changedSun = 0;
                        if (previousSunAmount == -1)
                        {
                            previousSunAmount = board.mSunMoney[0].Amount;
                        }
                        else
                        {
                            changedSun = board.mSunMoney[0].Amount - previousSunAmount;
                            previousSunAmount = board.mSunMoney[0].Amount;
                        }
                        if (changedSun != 0)
                        {
                            APClient.SendRingLinkPacket(changedSun);
                        }
                    }

                    if (receivedRingLinkAmount != 0)
                    {
                        if (receivedRingLinkAmount < (board.mSunMoney[0].Amount * -1)) //Prevent negative sun amount
                        {
                            receivedRingLinkAmount = board.mSunMoney[0].Amount * -1;
                        }
                        board.AddSunMoney(receivedRingLinkAmount, 0);
                        previousSunAmount = board.mSunMoney[0].Amount;
                        receivedRingLinkAmount = 0;
                    }
                }

                //Seed Link
                if (APClient.seedLinkEnabled && queuedSeedLinks.Count > 0)
                {
                    if (APClient.seedLinkEnabled && __instance.GameScene == GameScenes.Playing)
                    {
                        if (!__instance.m_board.HasConveyorBeltSeedBank() && !__instance.IsSlotMachineLevel() && !(__instance.GameMode == GameMode.ChallengeLastStand && __instance.m_board.mChallenge.mChallengeState != ChallengeState.LastStandOnslaught))
                        {
                            foreach (SeedLink queuedSeedLink in queuedSeedLinks)
                            {
                                SeedType theSeedType = queuedSeedLink.Seed;
                                foreach (SeedPacket seedPacket in board.SeedBanks[0].SeedPackets)
                                {
                                    if (seedPacket != null && seedPacket.PacketType == theSeedType)
                                    {
                                        seedPacket.mRefreshCounter = 0;
                                        seedPacket.mRefreshTime = Plant.GetRefreshTime(__instance, seedPacket.mPacketType, seedPacket.mImitaterType);
                                        seedPacket.mRefreshing = true;
                                        seedPacket.mActive = false;
                                        DisplayLinkMessage($"{APClient.apSession.Players.GetPlayerName(queuedSeedLink.Source)} used {Plant.GetNameString(__instance, seedPacket.mPacketType, seedPacket.mImitaterType)}", new UnityEngine.Color(1f, 1f, 1f), __instance);
                                    }
                                }
                            }
                        }
                    }
                    queuedSeedLinks.Clear();
                }

                //Lawn Link
                if (APClient.lawnLinkEnabled && queuedLawnLinks.Count > 0)
                {
                    lawnLinkBlocked = true; //Don't send new lawn links from these actions
                    if (__instance.GameScene == GameScenes.Playing && !__instance.IsWallnutBowlingLevel() && !(__instance.GameMode == GameMode.ChallengeLastStand && __instance.m_board.mChallenge.mChallengeState != ChallengeState.LastStandOnslaught) && __instance.GameScene == GameScenes.Playing && (__instance.m_board.ChooseSeedsOnCurrentLevel() || __instance.m_board.HasConveyorBeltSeedBank()))
                    {
                        foreach (LawnLink queuedLawnLink in queuedLawnLinks)
                        {
                            ReceivedLawnLink(queuedLawnLink, __instance, __instance.m_board);
                        }
                    }
                    queuedLawnLinks.Clear();
                    lawnLinkBlocked = false;
                }

                if (linkMessageActive && (DateTime.Now > resetLinkMessageAt))
                {
                    GameObject.Find("LinkMessage/Canvas/Layout/Center/Message/MessageText").GetComponent<TextMeshProUGUI>().text = "";
                    linkMessageActive = false;
                }

                //Sun Capacity
                if (APClient.sunCapacityItems)
                {
                    if (board.mSunMoney[0].Amount >= APClient.maximumSunCapacity && !Data.ignoreLockedTileLevelIds.Contains(Data.GetLevelIdFromGameplayActivity(__instance))) //Exceeded sun capacity
                    {
                        board.mSunMoney[0].Amount = APClient.maximumSunCapacity;
                        if (!redSunText)
                        {
                            GameObject sunLabel = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center/TopLeftLayout/SeedBankContainer/SeedBank/SunAmount_Background/SunAmountLabel");
                            if (sunLabel != null)
                            {
                                sunLabel.transform.GetComponent<TextMeshProUGUI>().color = new UnityEngine.Color(0.7058823529411765f, 0f, 0f);
                                redSunText = true;
                            }
                        }
                    }
                    else if (redSunText) //Sun text is red without exceeding sun cap
                    {
                        GameObject sunLabel = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center/TopLeftLayout/SeedBankContainer/SeedBank/SunAmount_Background/SunAmountLabel");
                        sunLabel.transform.GetComponent<TextMeshProUGUI>().color = UnityEngine.Color.black;
                        redSunText = false;
                    }
                }

                if (APClient.queuedUpItemEffects.Count > 0 && __instance.GameScene == GameScenes.Playing && !board.HasLevelAwardDropped() && !__instance.IsIZombieLevel() && (board.mBackground == BackgroundType.Day || board.mBackground == BackgroundType.Night || board.mBackground == BackgroundType.Pool || board.mBackground == BackgroundType.Fog || board.mBackground == BackgroundType.Roof || board.mBackground == BackgroundType.China || board.mBackground == BackgroundType.Boss))
                {
                    foreach (int itemId in APClient.queuedUpItemEffects)
                    {
                        Main.Log($"Processing Item Effect: #{itemId}");
                        if (itemId == 64) //Random seed packet
                        {
                            if (__instance.GameMode != GameMode.ChallengeBeghouled && __instance.GameMode != GameMode.ChallengeBeghouledTwist && !__instance.IsWallnutBowlingLevel())
                            {
                                int xPos = Data.random.Next(100, 650);
                                int yPos = Data.random.Next(60, 500);
                                Coin droppedSeed = __instance.m_board.AddCoin(xPos, yPos, CoinType.UsableSeedPacket, CoinMotion.FromPlant);

                                droppedSeed.mUsableSeedType = Data.GetFreeSeedType(board);

                                __instance.m_audioService.PlaySample(Il2CppReloaded.Constants.Sound.SOUND_SEEDLIFT);
                            }
                        }
                        else if (itemId == 69) //Brain Freeze
                        {
                            if (__instance.GameMode != GameMode.ChallengeZombiquarium && !__instance.IsIZombieLevel())
                            {
                                for (int zombieIndex = 0; zombieIndex < board.m_zombies.Count; zombieIndex++)
                                {
                                    board.m_zombies[zombieIndex].HitIceTrap();
                                }
                            }
                        }
                        else if (itemId == 79) //Zombie Hypnosis
                        {
                            if (__instance.GameMode != GameMode.ChallengeZombiquarium && !__instance.IsIZombieLevel())
                            {
                                for (int zombieIndex = 0; zombieIndex < board.m_zombies.Count; zombieIndex++)
                                {
                                    if (board.m_zombies[zombieIndex].mZombieType != ZombieType.Boss)
                                    {
                                        board.m_zombies[zombieIndex].StartMindControlled();
                                    }
                                }
                            }
                        }
                        else if (itemId == 81) //Sun Burst
                        {
                            if (board.ChooseSeedsOnCurrentLevel())
                            {
                                int xPos = Data.random.Next(100, 650);
                                int yPos = Data.random.Next(60, 500);

                                for (int i = 0; i < UnityEngine.Random.Range(4, 6); i++)
                                {
                                    board.AddCoin(xPos + UnityEngine.Random.Range(-10, 10), yPos + UnityEngine.Random.Range(-10, 10), CoinType.Sun, CoinMotion.FromPlant);
                                }
                                __instance.m_audioService.PlayFoley(FoleyType.Throw);
                            }
                        }
                        else if (itemId == 74) //Zen Garden sprout
                        {
                            ZenGard.AddRandomZenGardenPlant(__instance.m_zenGarden);
                        }
                        else if (itemId == 17)
                        {
                            Profile.AddConsumablePurchase(__instance.m_userService, (int)StoreItem.Rake, 1);
                            board.PlaceRake();
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
                                        if (APClient.seedLinkEnabled)
                                        {
                                            APClient.SendSeedLinkPacket(seedPacket.mPacketType);
                                        }
                                    }
                                }
                                if (ButterAbility.ButterAllowed())
                                {
                                    board.mPottedPlantsCollected = ButterAbility.butterCooldownLength;
                                    ButterAbility.ToggleCustomButterObjects(true);
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
                        else if (itemId == 73) //Zombie Shuffle Trap
                        {
                            for (int zombieIndex = 0; zombieIndex < board.m_zombies.Count; zombieIndex++)
                            {
                                board.m_zombies[zombieIndex].mYuckyFace = true;
                                board.m_zombies[zombieIndex].mYuckyFaceCounter = 169;
                            }
                        }
                        else if (itemId == 75 && !board.mApp.IsFinalBossLevel()) //RV Trap
                        {
                            Zombie bossZombie = board.AddZombie(ZombieType.Boss, -1, false);
                            bossZombie.BossRVAttack();
                            bossZombie.mZombieFade = 300;
                            bossZombie.mFireballRow = 999;
                        }
                        else if (itemId == 76) //Lawn Flip Trap
                        {
                            if (__instance.GameMode != GameMode.ChallengeBeghouled && __instance.GameMode != GameMode.ChallengeZenGarden && __instance.GameMode != GameMode.ChallengeBeghouledTwist)
                            {
                                for (int plantIndex = 0; plantIndex < board.m_plants.Count; plantIndex++)
                                {
                                    if (!(APClient.individualTileUnlockItems && !Data.ignoreLockedTileLevelIds.Contains(Data.GetLevelIdFromGameplayActivity(__instance)) && !APClient.receivedItems.Contains(1000 + (board.m_plants[plantIndex].mRow * 10) + (8 - board.m_plants[plantIndex].mPlantCol))))
                                    {
                                        board.m_plants[plantIndex].mPlantCol = 8 - board.m_plants[plantIndex].mPlantCol;
                                        board.m_plants[plantIndex].mX = board.GridToPixelX(board.m_plants[plantIndex].mPlantCol, board.m_plants[plantIndex].mRow);
                                        board.m_plants[plantIndex].mY = board.GridToPixelY(board.m_plants[plantIndex].mPlantCol, board.m_plants[plantIndex].mRow);
                                    }
                                }
                                __instance.m_audioService.PlayFoley(FoleyType.Floop);
                            }
                        }
                        else if (itemId == 77) //Lawn Randomiser Trap
                        {
                            if (__instance.GameMode != GameMode.ChallengeBeghouled && __instance.GameMode != GameMode.ChallengeZenGarden && __instance.GameMode != GameMode.ChallengeBeghouledTwist)
                            {
                                for (int plantIndex = 0; plantIndex < board.m_plants.Count; plantIndex++)
                                {
                                    Plant plant = board.m_plants[plantIndex];
                                    if (plant.mSeedType != SeedType.Cobcannon && plant.mSeedType != SeedType.Flowerpot && plant.mSeedType != SeedType.Lilypad && plant.mSeedType != SeedType.Pumpkinshell)
                                    {
                                        plant.RemoveEffects();
                                        plant.mController.Die();
                                        plant.PlantInitialize(plant.mPlantCol, plant.mRow, Data.GetFreeSeedType(board, true, Data.aquaticPlants.Contains(plant.mSeedType)), plant.mImitaterType);
                                    }
                                }
                                __instance.m_audioService.PlayFoley(FoleyType.Floop);
                            }
                        }
                        else if (itemId == 78) //Zombie Caffeine Trap
                        {
                            if (__instance.GameMode != GameMode.ChallengeZombiquarium && !__instance.IsIZombieLevel())
                            {
                                for (int zombieIndex = 0; zombieIndex < board.m_zombies.Count; zombieIndex++)
                                {
                                    if (board.m_zombies[zombieIndex].mZombieType != ZombieType.Boss)
                                    {
                                        board.m_zombies[zombieIndex].mVelX *= UnityEngine.Random.Range(6, 10);
                                        board.m_zombies[zombieIndex].UpdateAnimSpeed();
                                    }
                                }
                                __instance.m_audioService.PlayFoley(FoleyType.Wakeup);
                            }
                        }
                        else if (itemId == 80) //Crater Trap
                        {
                            if (__instance.GameMode != GameMode.ChallengeZombiquarium && !__instance.IsIZombieLevel())
                            {
                                List<int[]> eligibleSpots = new List<int[]>();
                                for (int column = 0; column < 9; column++)
                                {
                                    for (int row = 0; row < board.GetNumRows(); row++)
                                    {
                                        if (board.CanPlantAt(column, row, SeedType.Flowerpot) == PlantingReason.Ok || board.CanPlantAt(column, row, SeedType.Lilypad) == PlantingReason.Ok)
                                        {
                                            eligibleSpots.Add(new int[] { column, row });
                                        }
                                    }
                                }

                                int cratersToSpawn = Math.Min(eligibleSpots.Count, 3);
                                for (int crater = 0; crater < cratersToSpawn; crater++)
                                {
                                    int spotToUse = Data.random.Next(eligibleSpots.Count);
                                    board.AddACrater(eligibleSpots[spotToUse][0], eligibleSpots[spotToUse][1]).mGridItemCounter = 18000;
                                    eligibleSpots.RemoveAt(spotToUse);
                                }
                                __instance.m_audioService.PlayFoley(FoleyType.LimbsPop);
                            }
                        }
                        else if (itemId == 98) //Invisighoul Trap
                        {
                            for (int zombieIndex = 0; zombieIndex < board.m_zombies.Count; zombieIndex++)
                            {
                                if (board.m_zombies[zombieIndex].mZombieType != ZombieType.Boss)
                                {
                                    board.m_zombies[zombieIndex].mController.m_scale = new float2(0, 1);
                                    board.m_zombies[zombieIndex].mController.m_shadowController.m_obj.mVisible = false;
                                }
                            }
                            __instance.m_audioService.PlaySample(Il2CppReloaded.Constants.Sound.SOUND_MINDCONTROLLED);
                        }
                        else if (itemId == 99) //Randomised Seed Bank Trap
                        {
                            if (__instance.Board.ChooseSeedsOnCurrentLevel())
                            {
                                List<SeedType> alreadyChosenPackets = new List<SeedType>();
                                foreach (SeedPacket seedPacket in board.SeedBanks[0].SeedPackets)
                                {
                                    if (seedPacket != null && seedPacket.PacketType != SeedType.None)
                                    {
                                        seedPacket.mPacketType = Data.GetFreeSeedType(board, true);
                                        while (alreadyChosenPackets.Contains(seedPacket.mPacketType))
                                        {
                                            seedPacket.mPacketType = Data.GetFreeSeedType(board, true);
                                        }
                                        alreadyChosenPackets.Add(seedPacket.mPacketType);

                                        seedPacket.mRefreshCounter = 0;
                                        seedPacket.mRefreshTime = Plant.GetRefreshTime(__instance, seedPacket.mPacketType, seedPacket.mImitaterType);
                                        seedPacket.mRefreshing = true;
                                        seedPacket.mActive = false;
                                    }
                                }
                            }
                        }
                        else if (itemId == 2001) //Paranoia Trap
                        {
                            __instance.m_audioService.PlayFoley(FoleyType.JackInThebox);
                            Task.Delay(UnityEngine.Random.Range(7000, 20000)).ContinueWith(StopJackInTheBoxFoley, __instance);
                        }
                        else if (itemId == 2002) //Ladder Trap
                        {
                            if (!__instance.IsIZombieLevel())
                            {
                                for (int plantIndex = 0; plantIndex < board.m_plants.Count; plantIndex++)
                                {
                                    Plant plant = board.m_plants[plantIndex];
                                    if (board.GetGridItemAt(GridItemType.Ladder, plant.mPlantCol, plant.mRow) == null)
                                    {
                                        board.AddALadder(plant.mPlantCol, plant.mRow);
                                    }
                                }
                                __instance.m_audioService.PlaySample(Il2CppReloaded.Constants.Sound.SOUND_LADDER_ZOMBIE);
                            }
                        }
                        else if (itemId == 2003) //Zombie Growth Serum Trap
                        {
                            if (__instance.GameMode != GameMode.ChallengeZombiquarium && !__instance.IsIZombieLevel())
                            {
                                for (int zombieIndex = 0; zombieIndex < board.m_zombies.Count; zombieIndex++)
                                {
                                    if (!dontGrowThese.Contains(board.m_zombies[zombieIndex].mZombieType))
                                    {
                                        if (board.m_zombies[zombieIndex].mScaleZombie == 1)
                                        {
                                            ZombieGrowingInformation zombieGrowingInformation = new ZombieGrowingInformation
                                            {
                                                dataID = board.m_zombies[zombieIndex].DataID,
                                                defaultOffset = board.m_zombies[zombieIndex].mController.m_visualOffset,
                                                defaultScale = 1,
                                                targetScale = 1.5f,
                                                duration = 30,
                                            };
                                            growingZombies.Add(zombieGrowingInformation);

                                            board.m_zombies[zombieIndex].mBodyMaxHealth = board.m_zombies[zombieIndex].mBodyMaxHealth * 2;
                                            board.m_zombies[zombieIndex].mBodyHealth = board.m_zombies[zombieIndex].mBodyHealth * 2;

                                            board.m_zombies[zombieIndex].mHelmMaxHealth = board.m_zombies[zombieIndex].mHelmMaxHealth * 2;
                                            board.m_zombies[zombieIndex].mHelmHealth = board.m_zombies[zombieIndex].mHelmHealth * 2;

                                            board.m_zombies[zombieIndex].mShieldMaxHealth = board.m_zombies[zombieIndex].mShieldMaxHealth * 2;
                                            board.m_zombies[zombieIndex].mShieldHealth = board.m_zombies[zombieIndex].mShieldHealth * 2;

                                            __instance.m_audioService.PlayFoley(FoleyType.PlantGrow);
                                        }
                                    }
                                }
                            }
                        }
                        else if (itemId == 50) //mustache
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

                //Update butter display
                if (__instance.GameScene == GameScenes.Playing)
                {
                    if (ButterAbility.butter != null)
                    {
                        ButterAbility.butter.SetActive(ButterAbility.ButterAllowed());
                    }
                }

                if (APClient.lockIZombieZombies && __instance.IsIZombieLevel())
                {
                    for (int i = board.SeedBanks[0].mSeedPackets.Count - 1; i >= 0; i--)
                    {
                        if (!APClient.HasSeedType(board.SeedBanks[0].mSeedPackets[i].mPacketType))
                        {
                            board.SeedBanks[0].mSeedPackets[i].mActive = false;
                        }
                    }
                }
            }
        }

        public static int GetCurrentWaveNumber(Board board)
        {
            int currentWave = board.mCurrentWave;
            if (board.mApp.IsSurvivalMode() || board.mApp.GameMode == GameMode.ChallengeLastStand)
            {
                currentWave += ((board.mChallenge.mSurvivalStage) * board.GetNumWavesPerSurvivalStage());
            }
            return currentWave;
        }

        [HarmonyPatch(typeof(Board), nameof(Board.AddSunMoney))] //Triggers when Sun is added; if too much is being added, clamp it down
        public static class AddSunMoneyPatch
        {
            private static void Prefix(Board __instance, ref int theAmount, ref int playerIndex)
            {
                if (APClient.sunCapacityItems && __instance.mSunMoney[0].Amount + theAmount > APClient.maximumSunCapacity && !Data.ignoreLockedTileLevelIds.Contains(Data.GetLevelIdFromGameplayActivity(__instance.mApp)))
                {
                    theAmount = APClient.maximumSunCapacity - __instance.mSunMoney[0].Amount;
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.Update))]
        public static class BoardUpdatePatch
        {
            private static void Postfix(Board __instance)
            {
                if (__instance.mApp.GameScene == GameScenes.Playing)
                {
                    if (ButterAbility.ButterAllowed())
                    {
                        if (ButterAbility.butter == null)
                        {
                            ButterAbility.CreateButterUI();
                        }
                        else
                        {
                            ButterAbility.UpdateButterCooldown(__instance);
                        }
                    }

                    if (growingZombies.Count > 0)
                    {
                        List<ZombieGrowingInformation> stillGrowing = new List<ZombieGrowingInformation>();
                        for (int growingIndex = 0; growingIndex < growingZombies.Count; growingIndex++)
                        {
                            for (int zombieIndex = 0; zombieIndex < __instance.m_zombies.Count; zombieIndex++)
                            {
                                if (__instance.m_zombies[zombieIndex].DataID == growingZombies[growingIndex].dataID)
                                {
                                    growingZombies[growingIndex].progress += 1;
                                    float progress = growingZombies[growingIndex].progress / growingZombies[growingIndex].duration;
                                    if (progress >= 1)
                                    {
                                        progress = 1;
                                    }
                                    else
                                    {
                                        stillGrowing.Add(growingZombies[growingIndex]);
                                    }

                                    float targetScaleAddon = growingZombies[growingIndex].targetScale - growingZombies[growingIndex].defaultScale;
                                    __instance.m_zombies[zombieIndex].mScaleZombie = growingZombies[growingIndex].defaultScale + (progress * targetScaleAddon);
                                    __instance.m_zombies[zombieIndex].mController.m_visualOffset = growingZombies[growingIndex].defaultOffset + new Vector3(60 * progress, -190 * progress, 0);
                                }
                            }
                        }
                        growingZombies = stillGrowing;
                    }
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
                else if ((theCoinType == CoinType.PresentMinigames && (availableWaveLocations.Count == 0 || availableWaveLocations[0] > GetCurrentWaveNumber(__instance))) || theCoinType == CoinType.PresentPuzzleMode || theCoinType == CoinType.PresentSurvivalMode) //If it's a present and we don't have queued up wavesanity checks, delete it
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

                if (__instance.mType == CoinType.PresentMinigames && (__instance.mCoinAge >= 1000 || __instance.mBoard.mLevelComplete)) //Auto-collect flag drops
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
                else if (__instance.mType == CoinType.PresentMinigames && __instance.mUsableSeedType != SeedType.BeghouledButtonShuffle) //Wavesanity items
                {
                    foreach (int waveNumber in allWavesanityLocations)
                    {
                        int currentWave = GetCurrentWaveNumber(__instance.mBoard);

                        long locationId = (Data.GetLevelIdFromGameplayActivity(__instance.mApp) * 10000) + waveNumber;
                        if (waveNumber <= currentWave && APClient.apSession.Locations.AllMissingLocations.Contains(locationId))
                        {
                            var (desiredSprite, scaler) = Graphics.GetSpriteAndScaleForItemDrop(APClient.scoutedLocations[locationId]);
                            if (desiredSprite != null)
                            {
                                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                                var potentialPresents = allObjects.Where(obj => obj.name == "Coin_Present").ToArray();
                                GameObject presentObject = potentialPresents.FirstOrDefault(potentialPresent => potentialPresent.activeSelf);
                                if (presentObject != null && presentObject.activeSelf)
                                {
                                    //Correct present located - hide the present image, replace the Note image and show that instead
                                    presentObject.SetActive(false);
                                    Transform coinParent = presentObject.transform.parent;
                                    Transform noteObject = coinParent.Find("Coin_Note");
                                    SpriteRenderer spriteRenderer = noteObject.GetComponent<SpriteRenderer>();
                                    spriteRenderer.sprite = desiredSprite;
                                    noteObject.transform.localScale = new Vector3(scaler, scaler, 1);
                                    __instance.mUsableSeedType = SeedType.BeghouledButtonShuffle;
                                    noteObject.gameObject.SetActive(true);
                                    return;
                                }
                            }
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
                    foreach (int waveNumber in allWavesanityLocations)
                    {
                        int currentWave = GetCurrentWaveNumber(__instance.mBoard);

                        int locationId = (Data.GetLevelIdFromGameplayActivity(__instance.mApp) * 10000) + waveNumber;
                        if (waveNumber <= currentWave && APClient.apSession.Locations.AllMissingLocations.Contains(locationId))
                        {
                            APClient.SendLocation(locationId, true);
                            __instance.Die();
                            return false;
                        }
                    }
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
                else
                {
                    foreach (var coin in __instance.m_coins.m_list) //Auto collect any check items if the round finished too fast
                    {
                        if (coin != null && coin.mItem != null && coin.mItem.mType != null && coin.mItem.mType == CoinType.PresentMinigames)
                        {
                            coin.mItem.UpdateCollected();
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

        [HarmonyPatch(typeof(Zombie), nameof(Zombie.BossDie))] //Triggers when Zombie is killed
        public static class BossDiePatch
        {
            private static bool Prefix(Zombie __instance)
            {
                if (__instance.mFireballRow == 999) //Spawned to do an RV attack, then leave
                {
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Zombie), nameof(Zombie.DieNoLoot))] //Triggers when Zombie is killed
        public static class ZombieDiePatch
        {
            private static void Postfix(Zombie __instance)
            {
                int currentWave = GetCurrentWaveNumber(__instance.mBoard);

                if (availableWaveLocations.Count > 0 && availableWaveLocations[0] <= currentWave) //Wave location available
                {
                    int waveNumber = availableWaveLocations[0];
                    foreach (var coin in __instance.mBoard.m_coins.m_list) //Next, check if any presents already exist on the lawn
                    {
                        if (coin != null && coin.mItem != null && coin.mItem.mType != null && coin.mItem.mType == CoinType.PresentMinigames) //A present is already on the lawn
                        {
                            coin.mItem.UpdateCollected(); //Auto-collect any existing presents
                        }
                    }

                    long locationId = (Data.GetLevelIdFromGameplayActivity(__instance.mApp) * 10000) + waveNumber;
                    if (APClient.apSession.Locations.AllMissingLocations.Contains(locationId))
                    {
                        Rect zombieRect = __instance.GetZombieRect();
                        __instance.mBoard.AddCoin(zombieRect.center[0], zombieRect.center[1], CoinType.PresentMinigames, CoinMotion.Coin);
                        availableWaveLocations.RemoveAt(0);
                    }
                    else
                    {
                        availableWaveLocations.RemoveAt(0);
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
                Main.Log("Re-cached GameplayActivity.");

                int currentLevelId = Data.GetLevelIdFromGameplayActivity(__instance);

                displayingSeedStatsTime = 0; //Reset custom tooltip timers
                displayingSeedStatsIndex = -1;

                //Set up wavesanity
                availableWaveLocations = new List<int>();
                if (APClient.wavesanityMap.ContainsKey(currentLevelId.ToString())) //This level has checks for waves
                {
                    foreach (string waveNumber in APClient.wavesanityMap[currentLevelId.ToString()])
                    {
                        long waveLocationId = (currentLevelId * 10000) + Convert.ToInt64(waveNumber);
                        if (APClient.apSession.Locations.AllMissingLocations.Contains(waveLocationId))
                        {
                            availableWaveLocations.Add(Convert.ToInt32(waveNumber));
                        }
                    }
                }
                availableWaveLocations.Sort();
                allWavesanityLocations = availableWaveLocations.ToList();

                previousSunAmount = -1; //Used for ringlink
                showAwardScreen = false;
                redSunText = false; //Used for maximum sun capacity

                if (__instance.GameMode == GameMode.ChallengeZenGarden)
                {
                    if (!(APClient.receivedItems.Contains(2) || APClient.receivedItems.Contains(28)))
                    {
                        GameObject shopButton = GameObject.Find("Panels/P_ZenGarden_MainHUD/Canvas/Layout/Center/P_Zen_TopBar/KMBButtons/TutorialShopContainer");
                        shopButton.SetActive(false);
                        __instance.m_zenGarden._setTutorialDataState(false, false, false);
                    }
                }
                else
                {
                    if (APClient.individualTileUnlockItems && !Data.ignoreLockedTileLevelIds.Contains(currentLevelId)) //Add custom tile lock graphics
                    {
                        int numberOfRows = 5;
                        if (__instance.m_board.StageHasPool())
                        {
                            numberOfRows = 6;
                        }
                        for (int rowIndex = 0; rowIndex < numberOfRows; rowIndex++)
                        {
                            if (!(Data.MissingRows.ContainsKey(currentLevelId) && Data.MissingRows[currentLevelId].Contains(rowIndex))) //Skip unsodded rows
                            {
                                for (int columnIndex = 0; columnIndex < 9; columnIndex++)
                                {
                                    if (!APClient.receivedItems.Contains(1000 + (rowIndex * 10) + columnIndex))
                                    {
                                        CreateLockedTileSprite(rowIndex, columnIndex, __instance.m_board);
                                    }
                                }
                            }
                        }
                    }
                }

                //Create text element for Lawn Link / Seed Link
                GameObject messageTemplate = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(o => o.name == "P_MessageWidget");
                GameObject messageWidget = GameObject.Instantiate(messageTemplate, messageTemplate.transform.parent);
                messageWidget.name = "LinkMessage";
                messageWidget.SetActive(true);
                MelonCoroutines.Start(InitLinkMessageObject());

                Graphics.LoadCustomGraphics();

                //Reset growing zombies list
                growingZombies.Clear();

                //Apply plant stats
                if (__instance.Board.ChooseSeedsOnCurrentLevel() || APClient.conveyorMap.ContainsKey(currentLevelId.ToString()) || APClient.vasebreakerPlantMap.ContainsKey(currentLevelId.ToString()))
                {
                    PlantStatRando.ApplyPlantStats();
                }
                else
                {
                    Main.Log("Randomised plant stats do not apply to this level.");
                }

                ButterAbility.butter = null;
            }
        }

        public static void CreateLockedTileSprite(int row, int column, Board board)
        {
            GameObject tile = new GameObject($"LockedTile_{row}_{column}");
            GameObject gridOffset = GameObject.Find("GridOffset");
            tile.transform.SetParent(gridOffset.transform, false);

            var renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = Graphics.GetGraphic("ArchipelagoShadow");
            tile.transform.localScale = new Vector3(8, 8, 8);
            renderer.sortingLayerID = -932889863;
            renderer.sortingOrder = 6;
            renderer.color = new UnityEngine.Color(1f, 1f, 1f, 0.4f);

            int x = 218 + (227 * column);
            int y = -355 + (-285 * row);

            if (board.StageHasRoof())
            {
                y = -357 + (-240 * row);
                if (column < 5)
                {
                    y -= (5 - column) * 60;
                }
            }
            else if (board.StageHasPool())
            {
                y = -355 + (-255 * row);
            }

            tile.transform.localPosition = new Vector3(x, y, 0);
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
        public class GetNumSeedsInBankPatch
        {
            private static bool Prefix(Board __instance, ref int __result)
            {
                if (!__instance.mApp.IsCoopMode() && !__instance.mApp.IsVersusMode() && !__instance.mApp.IsIZombieLevel() && !__instance.mApp.IsScaryPotterLevel() && !__instance.mApp.IsWhackAZombieLevel() && !__instance.mApp.IsChallengeWithoutSeedBank() && !__instance.HasConveyorBeltSeedBank() && __instance.mApp.GameMode != GameMode.ChallengeBeghouled && __instance.mApp.GameMode != GameMode.ChallengeBeghouledTwist && __instance.mApp.GameMode != GameMode.ChallengeZombiquarium && __instance.mApp.GameMode != GameMode.ChallengeSlotMachine)
                {
                    List<SeedType> forcedPlants = new List<SeedType>();

                    if (__instance.mApp.GameMode == GameMode.ChallengeArtChallenge1)
                    {
                        forcedPlants.Add(SeedType.Wallnut);
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeArtChallenge2)
                    {
                        forcedPlants.Add(SeedType.Wallnut);
                        forcedPlants.Add(SeedType.Umbrella);
                        forcedPlants.Add(SeedType.Starfruit);
                    }
                    else if (__instance.mApp.GameMode == GameMode.ChallengeSeeingStars)
                    {
                        forcedPlants.Add(SeedType.Starfruit);
                    }

                    List<SeedType> bannedPlants = new List<SeedType>();

                    if (__instance.mApp.GameMode == GameMode.ChallengeLastStand)
                    {
                        bannedPlants.Add(SeedType.Sunflower);
                        bannedPlants.Add(SeedType.Twinsunflower);
                        bannedPlants.Add(SeedType.Sunshroom);
                    }
                    else if (__instance.mApp.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                    {
                        bannedPlants.Add(SeedType.Twinsunflower);
                        bannedPlants.Add(SeedType.Sunshroom);
                    }

                    String levelId = GetLevelIdFromGameplayActivity(__instance.mApp).ToString();
                    foreach (int plantIndex in APClient.plantBanlist[levelId.ToString()])
                    {
                        if (!bannedPlants.Contains(Data.seedTypes[plantIndex]))
                        {
                            bannedPlants.Add(Data.seedTypes[plantIndex]);
                        }
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
                __instance.m_canShovel = __instance.m_canShovel && APClient.HasShovel();
                __instance.m_canButter = __instance.m_canButter && ButterAbility.CanButterRightNow(Main.cachedGameplayActivity.m_board);
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

                //Forces Cloudy Day - use for a future feature?
                //                __instance.mApp.m_cloudyDayMode = new CloudyDayMode(__instance.mApp, ReloadedGameMode.CloudyDay);
                //                __instance.mApp.m_cloudyDayMode.GenerateWeatherForecast();
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
                if (APClient.lockIZombieZombies && __instance.mApp.IsIZombieLevel() && !APClient.HasSeedType(theSeedType) && !Data.seedTypes.Contains(theSeedType))
                {
                    __result = false;
                    return false;
                }
                else if (APClient.easyUpgradePlants)
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
                if (APClient.individualTileUnlockItems && !Data.ignoreLockedTileLevelIds.Contains(Data.GetLevelIdFromGameplayActivity(__instance.mApp)) && !APClient.receivedItems.Contains(1000 + (theGridY * 10) + theGridX))
                {
                    __result = PlantingReason.NotHere;
                }
                else if (APClient.easyUpgradePlants && __result == PlantingReason.Ok)
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
                if (APClient.deathLinkEnabled && APClient.deathLinkService != null && APClient.receivedDeathLink == null)
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
                if (APClient.deathLinkEnabled && APClient.deathLinkService != null)
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
                        int seedIndex = (int)defaultSeeds[i];
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
                ZombieType theZombieType = __result.m_zombieType;
                __result.m_decemberChance100 = 0; //We deal with this ourselves instead
                if (theZombieType == ZombieType.BackupDancer && APClient.costumeChances.ContainsKey("Backup Dancer (Original)"))
                {
                    __result.m_easterEggChance100 = (int)APClient.costumeChances["Backup Dancer (Original)"] / 100;
                }

                //Weight randomisation
                if (Data.zombieTypeWeights.ContainsKey(theZombieType))
                {
                    int levelId = Data.GetLevelIdFromGameplayActivity(__instance);
                    __result.m_weight = Data.zombieTypeWeights[__result.m_zombieType];
                    __result.m_firstLevel = -1;

                    if (theZombieType == ZombieType.PeaHead)
                    {
                        if (__instance.GameMode != GameMode.ChallengeWarAndPeas && __instance.GameMode != GameMode.ChallengeWarAndPeas2)
                        {
                            __result.m_value = 2; //Re-values Peahead to not be so pervasive with Zombie rando enabled
                            __result.m_firstWave = 3;
                        }
                        else
                        {
                            __result.m_value = 1;
                            __result.m_firstWave = 1;
                        }
                    }
                    else if (theZombieType == ZombieType.Imp)
                    {
                        __result.m_value = 1;
                    }

                    if (APClient.zombieWeightRandomisation != 0 && !Data.cantHard.Contains(levelId))
                    {
                        string zombieIndex = Array.FindIndex(Data.zombieTypes, zombieType => zombieType == theZombieType).ToString();
                        if (APClient.zombieWeightRandomisation == 1 && APClient.zombieWeightMap.ContainsKey(zombieIndex))
                        {
                            __result.m_weight = (int)APClient.zombieWeightMap[zombieIndex];
                        }
                        else if (APClient.zombieWeightRandomisation == 2)
                        {
                            if (APClient.zombieWeightMap.ContainsKey(levelId.ToString()) && ((JObject)APClient.zombieWeightMap[levelId.ToString()]).ContainsKey(zombieIndex))
                            {
                                __result.m_weight = (int)APClient.zombieWeightMap[levelId.ToString()][zombieIndex];
                            }
                        }
                    }
                }

                //Hard Mode
                if (APClient.harderZombieSpawns)
                {
                    if (__result.m_firstWave > 1)
                    {
                        __result.m_firstWave = 1;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.StartLevel))]
        public static class StartLevelPatch
        {
            private static void Postfix(Board __instance)
            {
                int currentLevelId = Data.GetLevelIdFromGameplayActivity(__instance.mApp);
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
                else if (APClient.lockConveyorPlants && __instance.HasConveyorBeltSeedBank())
                {
                    for (int i = __instance.SeedBanks[0].mSeedPackets.Count - 1; i >= 0; i--)
                    {
                        SeedType theSeedType = __instance.SeedBanks[0].mSeedPackets[i].mPacketType;
                        if (theSeedType != SeedType.None && !APClient.HasSeedType(theSeedType))
                        {
                            __instance.SeedBanks[0].RemoveSeed(i);
                            DisplayLinkMessage($"Locked Conveyor Plant: {Plant.GetNameString(__instance.mApp, theSeedType, theSeedType)}", new UnityEngine.Color(1f, 1f, 1f), __instance.mApp);
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
                else if (APClient.harderZombieSpawns && !(Data.cantHard.Contains(GetLevelIdFromGameplayActivity(__instance.mApp))))
                {
                    __instance.mZombieCountDown = (int)(__instance.mZombieCountDown * 0.3);
                }
                receivedRingLinkAmount = 0;
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.GetPlantDefinition))]
        public static class GetPlantDefinitionPatch
        {
            private static void Postfix(GameplayActivity __instance, ref PlantDefinition __result)
            {
                SeedType theSeedType = __result.m_seedType;

                //Costumes
                if (theSeedType == SeedType.Cabbagepult && APClient.costumeChances.ContainsKey("Cabbage-pult (PvZ2)"))
                {
                    __result.m_easterEggChance100 = (int)APClient.costumeChances["Cabbage-pult (PvZ2)"] / 100;
                }
                else if (theSeedType == SeedType.Flowerpot && APClient.costumeChances.ContainsKey("Flower Pot (China)"))
                {
                    if (!Data.plantPrefabs.ContainsKey(SeedType.Flowerpot))
                    {
                        Data.plantPrefabs[SeedType.Flowerpot] = __result.m_prefab;
                    }

                    if (Data.random.Next(10000) < (int)APClient.costumeChances["Flower Pot (China)"])
                    {
                        __result.m_prefab = __result.m_chinaGameObject;
                    }
                    else
                    {
                        __result.m_prefab = Data.plantPrefabs[SeedType.Flowerpot];
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
                if (__instance.mConveyorBeltCounter > 1 || !(APClient.conveyorMap.ContainsKey(levelId.ToString()) || APClient.lockConveyorPlants))
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

                    TodWeightedArray[] customSeeds;
                    if (APClient.conveyorMap.ContainsKey(levelId.ToString()))
                    {
                        JObject conveyorMap = (JObject)APClient.conveyorMap[levelId.ToString()]["weights"];
                        customSeeds = new TodWeightedArray[conveyorMap.Count];
                        int index = 0;
                        foreach (var conveyorMapSeed in conveyorMap)
                        {
                            int seedIndex = conveyorMapSeed.Key.ToInt32();
                            int seedWeight = (int)conveyorMapSeed.Value;
                            customSeeds[index].Item = (int)Data.seedTypes[seedIndex];
                            customSeeds[index].Weight = seedWeight;
                            index++;
                        }
                    }
                    else
                    {
                        Dictionary<SeedType, int> conveyorMap = Data.defaultConveyorMaps[levelId];
                        customSeeds = new TodWeightedArray[conveyorMap.Count];
                        int index = 0;
                        foreach (KeyValuePair<SeedType, int> conveyorEntry in conveyorMap)
                        {
                            customSeeds[index].Item = (int)conveyorEntry.Key;
                            customSeeds[index].Weight = conveyorEntry.Value;
                            index++;
                        }
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
                    __instance.mLastConveyorSeedType = theSeedType;
                    if (!(APClient.lockConveyorPlants && !APClient.HasSeedType(theSeedType)))
                    {
                        __instance.mBoard.SeedBanks[0].AddSeed(theSeedType, false);
                    }
                    else
                    {
                        DisplayLinkMessage($"Locked Conveyor Plant: {Plant.GetNameString(__instance.mApp, theSeedType, theSeedType)}", new UnityEngine.Color(1f, 1f, 1f), __instance.mApp);
                    }
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
            private static void Postfix(Board __instance, ref Plant __result)
            {
                if (__instance.HasConveyorBeltSeedBank())
                {
                    displayingSeedStatsIndex = -1;
                }
                if (APClient.lawnLinkEnabled && !lawnLinkBlocked && !__instance.mApp.IsWallnutBowlingLevel() && !(__instance.mApp.GameMode == GameMode.ChallengeLastStand && __instance.mChallenge.mChallengeState != ChallengeState.LastStandOnslaught) && __instance.mApp.GameScene == GameScenes.Playing && (__instance.ChooseSeedsOnCurrentLevel() || __instance.HasConveyorBeltSeedBank()))
                {
                    LawnLink lawnLink = new LawnLink();
                    lawnLink.Action = 0;
                    lawnLink.Row = __result.mRow;
                    lawnLink.Column = __result.mPlantCol;
                    lawnLink.Seed = __result.mSeedType;
                    lawnLink.Conveyor = (__instance.HasConveyorBeltSeedBank());
                    APClient.SendLawnLinkPacket(lawnLink);
                }
            }
        }

        [HarmonyPatch(typeof(Plant), nameof(Plant.Die))]
        public static class PlantDiePatch
        {
            private static void Prefix(Plant __instance)
            {
                if (!Data.suicidalPlants.Contains(__instance.mSeedType) && APClient.lawnLinkEnabled && !lawnLinkBlocked && !__instance.mApp.IsWallnutBowlingLevel() && __instance.mApp.GameScene == GameScenes.Playing && (__instance.mBoard.ChooseSeedsOnCurrentLevel() || __instance.mBoard.HasConveyorBeltSeedBank()))
                {
                    LawnLink lawnLink = new LawnLink();
                    lawnLink.Action = 1;
                    lawnLink.Row = __instance.mRow;
                    lawnLink.Column = __instance.mPlantCol;
                    lawnLink.Seed = __instance.mSeedType;
                    lawnLink.Conveyor = (__instance.mBoard.HasConveyorBeltSeedBank());
                    APClient.SendLawnLinkPacket(lawnLink);
                }
            }
        }

        [HarmonyPatch(typeof(SeedPacket), nameof(SeedPacket.WasPlanted))]
        public static class WasPlantedPatch
        {
            private static void Postfix(SeedPacket __instance)
            {
                if (APClient.seedLinkEnabled && __instance.mApp.GameScene == GameScenes.Playing && !__instance.mBoard.HasConveyorBeltSeedBank() && !__instance.mApp.IsSlotMachineLevel() && !(__instance.mApp.GameMode == GameMode.ChallengeLastStand && __instance.mBoard.mChallenge.mChallengeState != ChallengeState.LastStandOnslaught))
                {
                    APClient.SendSeedLinkPacket(__instance.mPacketType);
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

        [HarmonyPatch(typeof(Challenge), nameof(Challenge.ScaryPotterOpenPot))] //Checks if the level is over - if it is, decides what happens next
        public class ScaryPotterOpenPotPatch
        {
            private static bool Prefix(Challenge __instance, ref GridItem theScaryPot)
            {
                if (APClient.lockVasebreakerPlants && theScaryPot.mSeedType != SeedType.None && !APClient.HasSeedType(theScaryPot.mSeedType))
                {
                    theScaryPot.GridItemDie();

                    //Visual and sound
                    __instance.mApp.m_audioService.PlayFoley(FoleyType.VaseBreaking);
                    int particleX = __instance.mBoard.GridToPixelX(theScaryPot.mGridX, theScaryPot.mGridY) + 20;
                    int particleY = __instance.mBoard.GridToPixelY(theScaryPot.mGridX, theScaryPot.mGridY);
                    __instance.mApp.AddTodParticle(particleX, particleY, (int)RenderLayer.Particle, ParticleEffect.VaseShatter);

                    //Level clear
                    if (__instance.ScaryPotterIsCompleted())
                    {
                        if (__instance.mApp.IsScaryPotterLevel() && !__instance.mBoard.IsFinalScaryPotterStage())
                        {
                            int mGridY4 = theScaryPot.mGridY;
                            int mGridX3 = theScaryPot.mGridX;
                            __instance.PuzzlePhaseComplete(mGridX3, mGridY4);
                        }
                        int mGridY5 = theScaryPot.mGridY;
                        int mGridX4 = theScaryPot.mGridX;
                        __instance.SpawnLevelAward(mGridX4, mGridY5);
                    }

                    if (theScaryPot.mSeedType == SeedType.Leftpeater)
                    {
                        DisplayLinkMessage($"Locked Vasebreaker Plant: Backwards Repeater", new UnityEngine.Color(1f, 1f, 1f), __instance.mApp);
                    }
                    else
                    {
                        DisplayLinkMessage($"Locked Vasebreaker Plant: {Plant.GetNameString(__instance.mApp, theScaryPot.mSeedType, theScaryPot.mSeedType)}", new UnityEngine.Color(1f, 1f, 1f), __instance.mApp);
                    }
                    return false;
                }
                return true;
            }
        }

        public static void DisplayLinkMessage(string message, UnityEngine.Color color, GameplayActivity gameplayActivity)
        {
            Transform messageText = GameObject.Find("LinkMessage/Canvas/Layout/Center/Message/MessageText").transform;

            if (gameplayActivity.m_board.HasConveyorBeltSeedBank())
            {
                messageText.position = new Vector3(2480, -394, -248);
            }
            else
            {
                messageText.position = new Vector3(2165, -394, -248);
            }

            TextMeshProUGUI textMeshProUGUI = messageText.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = message;
            textMeshProUGUI.color = color;

            linkMessageActive = true;
            resetLinkMessageAt = DateTime.Now.AddSeconds(4);
        }

        public static IEnumerator InitLinkMessageObject()
        {
            yield return null; // wait 1 frame
            UnityEngine.Object.Destroy(GameObject.Find("LinkMessage").GetComponentInChildren<Il2CppTekly.DataModels.Binders.BinderContainer>());
            UnityEngine.Object.Destroy(GameObject.Find("LinkMessage").GetComponentInChildren<Il2CppSource.UI.TMProUGUIColorFlasher>());
            UnityEngine.Object.Destroy(GameObject.Find("LinkMessage/Canvas/Layout/Center/Message/Background"));

            Transform messageText = GameObject.Find("LinkMessage/Canvas/Layout/Center/Message/MessageText").transform;
            TextMeshProUGUI textMeshProUGUI = GameObject.Find("LinkMessage/Canvas/Layout/Center/Message/MessageText").GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.alignment = TextAlignmentOptions.Left;
            textMeshProUGUI.fontSizeMax = 70;
            textMeshProUGUI.text = "";

        }


        [HarmonyPatch(typeof(Challenge), nameof(Challenge.IZombieMouseDownWithZombie))] //Checks if the level is over - if it is, decides what happens next
        public class IZombieMouseDownWithZombiePatch
        {
            private static bool Prefix(Challenge __instance, ref int playerIndex)
            {
                if (APClient.lockIZombieZombies && !APClient.HasSeedType(__instance.mBoard.CursorObjects[playerIndex].mType))
                {
                    return false;
                }
                return true;
            }
        }

        public static void ReceivedLawnLink(LawnLink receivedLawnLink, GameplayActivity app, Board board)
        {
            if (receivedLawnLink.Conveyor == board.HasConveyorBeltSeedBank())
            {
                if (receivedLawnLink.Action == 0) //Planting
                {
                    if (board.CanPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed) == PlantingReason.Ok) //Add plant
                    {
                        if (APClient.lawnLinkChances.ContainsKey("add_plant") && Data.random.Next(100) <= (int)APClient.lawnLinkChances["add_plant"])
                        {
                            board.AddPlant(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed, receivedLawnLink.Seed);
                            DisplayLinkMessage($"{Plant.GetNameString(app, receivedLawnLink.Seed, receivedLawnLink.Seed)} planted by {APClient.apSession.Players.GetPlayerName(receivedLawnLink.Source)}", new UnityEngine.Color(1f, 1f, 0.3f), app);
                        }
                    }
                    else if (board.GetTopPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, PlantPriority.DiggingOrder) != null && board.CanPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed) == PlantingReason.NotHere) //Can't plant because there's already something there - so overwrite
                    {
                        if (APClient.lawnLinkChances.ContainsKey("overwrite_plant") && Data.random.Next(100) <= (int)APClient.lawnLinkChances["overwrite_plant"])
                        {
                            if (((receivedLawnLink.Seed == SeedType.Spikeweed || receivedLawnLink.Seed == SeedType.Spikerock) && (board.mBackground == BackgroundType.Roof || board.mBackground == BackgroundType.Boss)) || //No Spikeweed on the roof
                                (receivedLawnLink.Seed == SeedType.Gravebuster) ||
                                (receivedLawnLink.Seed == SeedType.InstantCoffee) ||
                                (receivedLawnLink.Seed == SeedType.Pumpkinshell) ||
                                (APClient.easyUpgradePlants == false && Data.upgradePlants.Contains(receivedLawnLink.Seed)))
                            {
                                return;
                            }

                            if ((receivedLawnLink.Row == 2 || receivedLawnLink.Row == 3) && (board.mBackground == BackgroundType.Pool || board.mBackground == BackgroundType.Fog)) //Water lanes
                            {
                                if (Data.aquaticPlants.Contains(receivedLawnLink.Seed)) //This is an aquatic plant, so it must be an empty tile in order to use it
                                {
                                    SeedType overwrittenPlant = SeedType.None;
                                    while (true) //Loop until broken
                                    {
                                        Plant topPlant = board.GetTopPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, PlantPriority.DiggingOrder); //Get top plant
                                        if (receivedLawnLink.Seed == SeedType.Lilypad && !Data.aquaticPlants.Contains(topPlant.mSeedType)) //Lawnlink receiving a Lily Pad onto a tile with a Lily Pad already, just do nothing
                                        {
                                            return;
                                        }
                                        if (topPlant == null) //If there is no plant there anymore, break the loop
                                        {
                                            break;
                                        }
                                        else //If there is still a plant there, we need to get rid of it
                                        {
                                            overwrittenPlant = topPlant.mSeedType;
                                            topPlant.Die();
                                        }
                                    }
                                    if (board.CanPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed) == PlantingReason.Ok)
                                    {
                                        board.AddPlant(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed, receivedLawnLink.Seed); //Add your plant
                                    }
                                    DisplayLinkMessage($"{APClient.apSession.Players.GetPlayerName(receivedLawnLink.Source)} replaced your {Plant.GetNameString(app, overwrittenPlant, overwrittenPlant)} with {Plant.GetNameString(app, receivedLawnLink.Seed, receivedLawnLink.Seed)}", new UnityEngine.Color(1f, 0.5f, 0.5f), app);
                                }
                                else //We can't plant there, we've got a non-aquatic plant - so there must be either an aquatic plant already there, or there's just a plant on a Lily Pad OR it's an impossible lily pad plant
                                {
                                    if (receivedLawnLink.Seed == SeedType.Potatomine || receivedLawnLink.Seed == SeedType.Spikerock || receivedLawnLink.Seed == SeedType.Spikeweed || receivedLawnLink.Seed == SeedType.Flowerpot) //Impossible lily pad plants
                                    {
                                        return;
                                    }
                                    Plant topPlant = board.GetTopPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, PlantPriority.DiggingOrder); //Get top plant
                                    if (!Data.aquaticPlants.Contains(topPlant.mSeedType)) //If it's an aquatic plant, just give up as you'd have to spawn in a Lily Pad as well which is cheating >:(
                                    {
                                        SeedType overwrittenPlant = topPlant.mSeedType;
                                        topPlant.Die(); //Remove plant on the Lily Pad
                                        if (board.CanPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed) == PlantingReason.Ok)
                                        {
                                            board.AddPlant(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed, receivedLawnLink.Seed); //Add your plant
                                        }
                                        DisplayLinkMessage($"{APClient.apSession.Players.GetPlayerName(receivedLawnLink.Source)} replaced your {Plant.GetNameString(app, overwrittenPlant, overwrittenPlant)} with {Plant.GetNameString(app, receivedLawnLink.Seed, receivedLawnLink.Seed)}", new UnityEngine.Color(1f, 0.5f, 0.5f), app);
                                    }
                                }
                            }
                            else if (!Data.aquaticPlants.Contains(receivedLawnLink.Seed) && !(receivedLawnLink.Seed == SeedType.Flowerpot && board.GetFlowerPotAt(receivedLawnLink.Column, receivedLawnLink.Row) != null)) //Planting a non-aquatic plant
                            {
                                SeedType overwrittenPlant = SeedType.None;
                                while (true) //Loop until broken
                                {
                                    Plant topPlant = board.GetTopPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, PlantPriority.DiggingOrder); //Get top plant
                                    if (board.CanPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed) == PlantingReason.Ok || topPlant == null) //If you can now plant there, plant it - otherwise keep on deleting!
                                    {
                                        break;
                                    }
                                    else //If there is still a plant there, we need to get rid of it
                                    {
                                        overwrittenPlant = topPlant.mSeedType;
                                        topPlant.Die();
                                    }
                                }
                                if (board.CanPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed) == PlantingReason.Ok)
                                {
                                    board.AddPlant(receivedLawnLink.Column, receivedLawnLink.Row, receivedLawnLink.Seed, receivedLawnLink.Seed); //Add your plant
                                }
                                DisplayLinkMessage($"{APClient.apSession.Players.GetPlayerName(receivedLawnLink.Source)} replaced your {Plant.GetNameString(app, overwrittenPlant, overwrittenPlant)} with {Plant.GetNameString(app, receivedLawnLink.Seed, receivedLawnLink.Seed)}", new UnityEngine.Color(1f, 0.5f, 0.5f), app);
                            }
                        }
                    }
                }
                else if (receivedLawnLink.Action == 1 && APClient.lawnLinkChances.ContainsKey("remove_plant") && Data.random.Next(100) <= (int)APClient.lawnLinkChances["remove_plant"]) //Digging
                {
                    Plant plant = board.GetTopPlantAt(receivedLawnLink.Column, receivedLawnLink.Row, PlantPriority.EatingOrder);
                    if (plant != null)
                    {
                        plant.Die();
                        DisplayLinkMessage($"{APClient.apSession.Players.GetPlayerName(receivedLawnLink.Source)} removed your {Plant.GetNameString(app, plant.mSeedType, plant.mImitaterType)}", new UnityEngine.Color(1f, 0.5f, 0.5f), app);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Board), nameof(Board.PutZombieInWave))]
        public static class PutZombieInWavePatch
        {
            private static void Prefix(Board __instance, ref int theWaveNumber, ref Board.ZombiePicker theZombiePicker)
            {
                if (APClient.harderZombieSpawns && wavePointsAdjusted != theWaveNumber && !(Data.cantHard.Contains(GetLevelIdFromGameplayActivity(__instance.mApp))))
                {
                    if (__instance.mApp.ReloadedGameMode == ReloadedGameMode.CloudyDay)
                    {
                        theZombiePicker.ZombiePoints = (int)(theZombiePicker.ZombiePoints * 1.2);
                    }
                    else
                    {
                        theZombiePicker.ZombiePoints = (int)(theZombiePicker.ZombiePoints * 1.5);
                    }
                    wavePointsAdjusted = theWaveNumber;
                }
            }
        }

        [HarmonyPatch(typeof(SeedChooserScreen), nameof(SeedChooserScreen.SeedNotAllowedToPick))]
        public static class SeedNotAllowedToPickPatch
        {
            private static bool Prefix(SeedChooserScreen __instance, ref SeedType theSeedType, ref bool __result)
            {
                if (APClient.HasSeedType(theSeedType) && APClient.plantBanlist.Count > 0)
                {
                    String levelId = GetLevelIdFromGameplayActivity(__instance.mApp).ToString();
                    SeedType chosenSeedType = theSeedType;
                    if (APClient.plantBanlist.ContainsKey(levelId))
                    {
                        int plantIndex = Array.FindIndex(Data.seedTypes, seedType => seedType == chosenSeedType);
                        if (APClient.plantBanlist[levelId.ToString()].Any(bannedSeed => (int)bannedSeed == plantIndex))
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GameplayActivity), nameof(GameplayActivity.IsFirstTimeAdventureMode))]
        public static class IsFirstTimeAdventureModePatch
        {
            private static bool Prefix(GameplayActivity __instance, ref bool __result)
            {
                if (__instance.IsWallnutBowlingLevel() && __instance.GameScene == GameScenes.Playing)
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}
