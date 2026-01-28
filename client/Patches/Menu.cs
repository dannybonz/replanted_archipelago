using HarmonyLib;
using Il2Cpp;
using Il2CppBest.HTTP.Shared.Extensions;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using Il2CppReloaded.UI;
using Il2CppSource.DataModels;
using Il2CppSource.TreeStateActivities;
using Il2CppSource.Utils;
using Il2CppTekly.DataModels.Binders;
using Il2CppTekly.DataModels.Models;
using Il2CppTekly.Localizations;
using Il2CppTMPro;
using Il2CppUI.Scripts;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ReplantedArchipelago.Patches
{
    public class Menu
    {
        public static GameObject ConnectionPanel;
        public static GameObject hostInput;
        public static GameObject slotInput;
        public static GameObject passwordInput;
        public static GameObject ErrorPanel;
        public static GameObject ClientPanel;
        public static GameObject messageInput;
        public static int cachedMessageCount = 0;
        private static bool darkerLog = false; // keep track of odd/even entries
        public static bool showAwardScreen = true;

        public static GameObject RemoveUnwantedComponents(GameObject gameObject, bool aggressive)
        {
            foreach (var localizer in gameObject.GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(localizer);
            if (aggressive)
            {
                foreach (var binder in gameObject.GetComponentsInChildren<InputBinder>())
                    GameObject.Destroy(binder);
                foreach (var selectable in gameObject.GetComponentsInChildren<Selectable>())
                    GameObject.Destroy(selectable);
            }
            return gameObject;
        }

        [HarmonyPatch(typeof(MainMenuPanelView), nameof(MainMenuPanelView.Start))]
        public class MainMenuStartPatch
        {
            private static void Postfix(MainMenuPanelView __instance)
            {
                Main.Log("Main Menu Panel View found.");

                TimeUtil.SetFlowingTimeScale(1f);

                GameObject accountSign = __instance.transform.Find("Canvas/Layout/Center/Main/AccountSign").gameObject;
                if (accountSign != null)
                {
                    accountSign.SetActive(false);
                }

                Data.panelTemplate = __instance.transform.parent.Find("P_UsersPanel_Rename").gameObject;
                Data.errorTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel_InvalidName").gameObject, false);
                Data.clientTemplate = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                Data.inputTemplate = __instance.transform.parent.Find("P_UsersPanel_Rename/Canvas/Layout/Center/Rename/NameInputField").gameObject;
                Data.logTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel/Canvas/Layout/Center/Main/InsetWindow/P_UsersPanel_UserEntry").gameObject, true);
                Data.buttonTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel/Canvas/Layout/Center/Main/Buttons/P_BacicButton_Rename").gameObject, false);

                GameObject apSettingsButton = CreateButton("Text Client", __instance.transform.Find("Canvas/Layout/Center/Main/Menu"), ShowClientPanel);
                RectTransform apSettingsRect = apSettingsButton.GetComponent<RectTransform>();
                apSettingsRect.anchorMin = new Vector2(0, 1);
                apSettingsRect.anchorMax = new Vector2(0, 1);
                apSettingsRect.pivot = new Vector2(0, 1);
                apSettingsRect.anchoredPosition = new Vector2(310f, -20f);

                GameObject apGoalButton = CreateButton("View Goal", __instance.transform.Find("Canvas/Layout/Center/Main/Menu"), ShowGoalPanel);
                RectTransform apGoalRect = apGoalButton.GetComponent<RectTransform>();
                apGoalRect.anchorMin = new Vector2(0, 1);
                apGoalRect.anchorMax = new Vector2(0, 1);
                apGoalRect.pivot = new Vector2(0, 1);
                apGoalRect.anchoredPosition = new Vector2(310f, -200f);

                if (!APClient.currentlyConnected)
                {
                    ShowConnectionPanel();
                }
                else if (APClient.HasBoss() && !APClient.clearedLevels.Contains(50))
                {
                    ShowErrorPanel("Goal Unlocked", "You have unlocked the final battle with Dr. Zomboss! Go fight him to complete your game!");
                }

                Profile.ProcessIUserService();
                if (Profile.cachedUserService != null)
                {
                    Main.Log("Adjusting Level...");
                    Profile.cachedUserService.ActiveUserProfile.mLevel = 40; //Set level back to 40 (prevents the forced 1-1?)
                }

                Main.Log("Main Menu Panel View modified.");
            }
        }

        [HarmonyPatch(typeof(MainMenuActivity), nameof(MainMenuActivity.ActiveStarted))]
        public class MainMenuActivityPatch
        {
            private static void Postfix(MainMenuActivity __instance)
            {
                Profile.ProcessIUserService(__instance.m_userService);
            }
        }

        [HarmonyPatch(typeof(MainMenuPanelView), nameof(MainMenuPanelView.Update))]
        public class MainMenuUpdatePatch
        {
            private static void Postfix(MainMenuPanelView __instance)
            {
                if (Main.currentScene == "Frontend")
                {
                    GameObject usersPanel = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                    if (usersPanel != null)
                    {
                        usersPanel.SetActive(false);
                    }

                    if ((!APClient.currentlyConnected || APClient.apSession.Socket == null || !APClient.apSession.Socket.Connected) && (ConnectionPanel == null || !ConnectionPanel.active))
                    {
                        Main.Log("Lost connection to server.");
                        ShowConnectionPanel();
                        ShowErrorPanel("Lost Connection", "You have been disconnected from the server.");
                    }

                    if (APClient.currentlyConnected && ClientPanel != null && ClientPanel.active && APClient.receivedMessages.Count > cachedMessageCount)
                    {
                        for (int i = cachedMessageCount; i < APClient.receivedMessages.Count; i++)
                        {
                            AddClientMessage(APClient.receivedMessages[i]);
                        }
                        cachedMessageCount = APClient.receivedMessages.Count;
                    }

                    APClient.chooserRefreshState = "none"; //No need to refresh seed chooser
                }
            }
        }

        public static void ShowConnectionPanel()
        {
            ConnectionPanel = GameObject.Instantiate(Data.panelTemplate, Data.panelTemplate.transform.parent);
            ConnectionPanel.name = "ConnectionPanel";
            ConnectionPanel.SetActive(true);

            Transform center = ConnectionPanel.transform.Find("Canvas/Layout/Center/Rename");
            center.Find("HeaderText").GetComponent<TextMeshProUGUI>().text = "Archipelago Setup";
            RemoveUnwantedComponents(center.Find("HeaderText").gameObject, true);

            center.Find("SubheadingText").GetComponent<TextMeshProUGUI>().text = "Host:";
            GameObject originalSubheader = RemoveUnwantedComponents(center.Find("SubheadingText").gameObject, true);
            GameObject originalInput = center.Find("NameInputField").gameObject;

            hostInput = GameObject.Instantiate(Data.inputTemplate, center);
            hostInput.name = "hostInput";
            hostInput.GetComponent<TMP_InputField>().onValueChanged = new TMP_InputField.OnChangeEvent();
            hostInput.GetComponent<TMP_InputField>().text = Main.defaultHost;

            GameObject slotHeader = GameObject.Instantiate(originalSubheader, center);
            slotHeader.name = "slotHeader";
            slotHeader.GetComponent<TextMeshProUGUI>().text = "Slot Name:";

            slotInput = GameObject.Instantiate(Data.inputTemplate, center);
            slotInput.name = "slotInput";
            slotInput.GetComponent<TMP_InputField>().onValueChanged = new TMP_InputField.OnChangeEvent();
            slotInput.GetComponent<TMP_InputField>().text = Main.defaultSlot;

            GameObject passwordHeader = GameObject.Instantiate(originalSubheader, center);
            passwordHeader.name = "passwordHeader";
            passwordHeader.GetComponent<TextMeshProUGUI>().text = "Password:";

            passwordInput = GameObject.Instantiate(Data.inputTemplate, center);
            passwordInput.name = "passwordInput";
            passwordInput.GetComponent<TMP_InputField>().onValueChanged = new TMP_InputField.OnChangeEvent();
            passwordInput.GetComponent<TMP_InputField>().text = Main.defaultPassword;

            center.Find("Buttons/P_BacicButton_OK/Label").GetComponent<TextMeshProUGUI>().text = "Connect";
            Button okButton = center.Find("Buttons/P_BacicButton_OK").GetComponent<Button>();
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener((Action)ConnectButtonPressed);
            foreach (var localizer in center.Find("Buttons/P_BacicButton_OK").GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(localizer);

            GameObject.Destroy(center.Find("Buttons/P_BacicButton_Cancel").gameObject);
            GameObject.Destroy(originalInput);
        }

        public static void HideConnectionPanel()
        {
            if (ConnectionPanel != null)
            {
                ConnectionPanel.SetActive(false);
            }
        }

        public static void ConnectButtonPressed()
        {
            string host = hostInput.GetComponent<TMP_InputField>().text;
            string slot = slotInput.GetComponent<TMP_InputField>().text;
            string password = passwordInput.GetComponent<TMP_InputField>().text;

            APSettings.Host.Value = host;
            APSettings.SlotName.Value = slot;
            APSettings.Password.Value = password;

            APClient.AttemptConnection(host, slot, password);
        }

        public static void ShowGoalPanel()
        {
            string goalText = "";
            if (APClient.adventureLevelsGoal > 0)
            {
                goalText += $"Adventure Levels: {APClient.clearedLevels.Count(levelId => levelId < 51)}/{APClient.adventureLevelsGoal}<br>";
            }
            if (APClient.adventureAreasGoal > 0)
            {
                goalText += $"Adventure Areas: {APClient.GetClearedAreaCount()}/{APClient.adventureAreasGoal}<br>";
            }
            if (APClient.minigameLevelsGoal > 0)
            {
                goalText += $"Mini-Game Levels: {APClient.clearedLevels.Count(levelId => levelId >= 51 && levelId < 71)}/{APClient.minigameLevelsGoal}<br>";
            }
            if (APClient.puzzleLevelsGoal > 0)
            {
                goalText += $"Puzzle Levels: {APClient.clearedLevels.Count(levelId => levelId >= 71 && levelId < 89)}/{APClient.puzzleLevelsGoal}<br>";
            }
            if (APClient.survivalLevelsGoal > 0)
            {
                goalText += $"Survival Levels: {APClient.clearedLevels.Count(levelId => levelId >= 89 && levelId < 99)}/{APClient.survivalLevelsGoal}<br>";
            }
            if (APClient.cloudyDayLevelsGoal > 0)
            {
                goalText += $"Cloudy Day Levels: {APClient.clearedLevels.Count(levelId => levelId >= 109 && levelId < 121)}/{APClient.cloudyDayLevelsGoal}<br>";
            }
            if (APClient.bonusLevelsGoal > 0)
            {
                goalText += $"Bonus Levels: {APClient.clearedLevels.Count(levelId => levelId >= 99 && levelId < 109)}/{APClient.bonusLevelsGoal}<br>";
            }
            if (APClient.overallLevelsGoal > 0)
            {
                goalText += $"Total Levels: {APClient.clearedLevels.Count()}/{APClient.overallLevelsGoal}<br>";
            }

            if (APClient.fastGoal == false)
            {
                if (APClient.adventureProgression == 0 || APClient.adventureProgression == 1)
                {
                    if (APClient.clearedLevels.Contains(49))
                    {
                        goalText += $"Roof: Level 5-9 Cleared";
                    }
                    else
                    {
                        goalText += $"Roof: Level 5-9 Uncleared";
                    }
                }
                else if (APClient.adventureProgression == 2)
                {
                    if (APClient.receivedItems.Contains(24)) //Roof access
                    {
                        goalText += $"Roof Access Obtained";
                    }
                    else
                    {
                        goalText += $"Roof Access Needed";
                    }
                }
            }

            ShowErrorPanel("Goal", goalText);
        }


        public static void ShowErrorPanel(string header, string text)
        {
            ErrorPanel = GameObject.Instantiate(Data.errorTemplate, Data.errorTemplate.transform.parent);
            ErrorPanel.name = "ErrorPanel";
            ErrorPanel.SetActive(true);

            Transform center = ErrorPanel.transform.Find("Canvas/Layout/Center/NameConflict");

            center.Find("HeaderText").GetComponent<TextMeshProUGUI>().text = header;
            center.Find("SubheadingText").GetComponent<TextMeshProUGUI>().text = text;

            Button okButton = center.Find("Buttons/P_BacicButton_OK").GetComponent<Button>();
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener((Action)HideErrorPanel);
        }

        public static void HideErrorPanel()
        {
            if (ErrorPanel != null)
            {
                ErrorPanel.SetActive(false);
            }
        }

        public static void ShowClientPanel()
        {
            ClientPanel = GameObject.Instantiate(Data.clientTemplate, Data.clientTemplate.transform.parent);
            ClientPanel.name = "ClientPanel";
            ClientPanel.SetActive(true);

            Transform main = ClientPanel.transform.Find("Canvas/Layout/Center/Main");
            GameObject headerObject = RemoveUnwantedComponents(main.Find("HeaderText").gameObject, true);
            headerObject.GetComponent<TextMeshProUGUI>().text = "Text Client";

            //Delete old entries
            Transform insetWindow = main.Find("InsetWindow");
            for (int i = insetWindow.childCount - 1; i >= 0; i--)
            {
                Transform child = insetWindow.GetChild(i);
                if (child.name.StartsWith("P_UsersPanel"))
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }

            //Create scrolling container for log entries
            RectTransform insetRect = insetWindow.GetComponent<RectTransform>();
            if (insetRect.GetComponent<RectMask2D>() == null)
                insetRect.gameObject.AddComponent<RectMask2D>();

            GameObject clientLogsContainer = new GameObject("ClientLogs");
            RectTransform clientLogsRect = clientLogsContainer.AddComponent<RectTransform>();
            clientLogsRect.SetParent(insetRect, false);
            clientLogsRect.anchorMin = new Vector2(0, 1);
            clientLogsRect.anchorMax = new Vector2(1, 1);
            clientLogsRect.pivot = new Vector2(0.5f, 1);
            clientLogsRect.anchoredPosition = Vector2.zero;
            clientLogsRect.sizeDelta = Vector2.zero;

            var layout = clientLogsContainer.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = clientLogsContainer.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = main.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = insetRect;
            scrollRect.content = clientLogsRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            //Delete old buttons
            Transform buttonArea = main.Find("Buttons");
            GameObject.DestroyImmediate(buttonArea.Find("P_BacicButton_OK").gameObject);
            GameObject.DestroyImmediate(buttonArea.Find("P_BacicButton_Cancel").gameObject);
            GameObject.DestroyImmediate(buttonArea.Find("P_BacicButton_Delete").gameObject);
            GameObject.DestroyImmediate(buttonArea.Find("P_BacicButton_Rename").gameObject);

            //Add text input
            messageInput = GameObject.Instantiate(Data.inputTemplate, main);
            messageInput.name = "MessageInput";
            messageInput.SetActive(true);
            messageInput.GetComponent<TMP_InputField>().onValueChanged = new TMP_InputField.OnChangeEvent();
            messageInput.GetComponent<TMP_InputField>().characterLimit = 200;

            //Position text input
            RectTransform inputRect = messageInput.GetComponent<RectTransform>();
            inputRect.anchoredPosition = new Vector2(-220, -490);
            inputRect.anchorMin = new Vector2(0.5f, 0.5f); // Anchor in the center
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);     // Pivot in the center

            //Add Send button
            GameObject sendButton = CreateButton("Send", main, SendClientMessage);
            RectTransform sendRect = sendButton.GetComponent<RectTransform>();
            sendRect.anchoredPosition = new Vector2(725, -485);
            sendRect.anchorMin = new Vector2(0.5f, 0.5f);
            sendRect.anchorMax = new Vector2(0.5f, 0.5f);
            sendRect.pivot = new Vector2(0.5f, 0.5f);
            sendRect.sizeDelta = new Vector2(420f, sendRect.sizeDelta.y);

            //Add Close button
            GameObject closeButton = CreateButton("Close", main, HideClientPanel);
            RectTransform closeRect = closeButton.GetComponent<RectTransform>();
            closeRect.anchoredPosition = new Vector2(0, -650);
            closeRect.anchorMin = new Vector2(0.5f, 0.5f); // Anchor in the center
            closeRect.anchorMax = new Vector2(0.5f, 0.5f);
            closeRect.pivot = new Vector2(0.5f, 0.5f);     // Pivot in the center
            closeRect.sizeDelta = new Vector2(1880f, closeRect.sizeDelta.y);

            //Force layout rebuild
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(clientLogsRect);

            //Build previously received messages
            cachedMessageCount = APClient.receivedMessages.Count;
            foreach (string message in APClient.receivedMessages)
            {
                AddClientMessage(message);
            }
        }

        public static void SendClientMessage()
        {
            if (messageInput.GetComponent<TMP_InputField>().text != "")
            {
                APClient.apSession.Say(messageInput.GetComponent<TMP_InputField>().text);
            }
            messageInput.GetComponent<TMP_InputField>().text = "";
        }

        public static void AddClientMessage(string message)
        {
            Transform main = ClientPanel.transform.Find("Canvas/Layout/Center/Main");
            RectTransform clientLogsRect = main.Find("InsetWindow/ClientLogs").GetComponent<RectTransform>();
            ScrollRect scrollRect = main.GetComponent<ScrollRect>();

            GameObject entry = GameObject.Instantiate(Data.logTemplate, clientLogsRect);
            TextMeshProUGUI textComponent = entry.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            textComponent.text = message;
            textComponent.alignment = TextAlignmentOptions.Left;
            textComponent.enableWordWrapping = true;

            //Alternating background colour
            darkerLog = !darkerLog;
            foreach (var img in entry.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                if (darkerLog)
                {
                    img.color = new UnityEngine.Color(0f, 0f, 0f, 1f);
                }
                else
                {
                    img.color = new UnityEngine.Color(0f, 0f, 0f, 0.0f);
                }
            }

            entry.SetActive(true);

            //Force layout and scroll to bottom
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(clientLogsRect);
            scrollRect.verticalNormalizedPosition = 0f;
        }

        public static void HideClientPanel()
        {
            if (ClientPanel != null)
            {
                ClientPanel.SetActive(false);
            }
        }

        public static GameObject CreateButton(string label, Transform parent, Action onClick)
        {
            GameObject button = GameObject.Instantiate(Data.buttonTemplate, parent);
            button.name = label;
            button.GetComponentInChildren<TextMeshProUGUI>().text = label;

            var buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(onClick);
            return button;
        }

        [HarmonyPatch(typeof(GameplayService), nameof(GameplayService.OnActiveProfileChanged))] //Triggers when changing active profile
        public class ProfileChangedPatch
        {
            private static void Postfix(GameplayService __instance)
            {
                __instance.HasWatchedIntro = true;
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

                    APClient.CompletedLevel(Data.GetLevelIdFromGameplayActivity(__instance)); //Re-send the completion check in case it was somehow missed until now (might help fix the Last Stand problem people are facing?)

                    __instance.KillBoard();
                    if (showAwardScreen)
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

        [HarmonyPatch(typeof(SeedChooserDataModel), nameof(SeedChooserDataModel.OnTick))]
        public static class SeedChooserDataModelOnTickPatch
        {
            private static void Prefix(SeedChooserDataModel __instance)
            {
                if (Main.currentScene != "Gameplay")
                {
                    return;
                }

                if (APClient.HasSeedType(SeedType.Imitater))
                {
                    __instance.m_isImitaterUnlocked.m_value = true;

                    if (APClient.easyUpgradePlants || !APClient.imitaterOpen)
                    {
                        Il2CppSystem.Collections.Generic.List<ModelReference> imitaterEntries = __instance.m_imitaterEntriesModel.m_models;
                        for (int i = imitaterEntries.Count - 1; i >= 0; i--)
                        {
                            var entry = imitaterEntries[i].Model.Cast<SeedChooserEntryModel>();

                            if ((!APClient.imitaterOpen && !APClient.HasSeedType(entry.m_chosenSeed.mSeedType)) || (APClient.easyUpgradePlants && Data.easyUpgradeCostAddons.ContainsKey(entry.m_chosenSeed.mSeedType)))
                            {
                                imitaterEntries.RemoveAt(i);
                            }
                        }
                    }
                }
                else if (__instance.m_isImitaterUnlocked.m_value == true)
                {
                    __instance.m_isImitaterUnlocked.m_value = false;
                }

                if (APClient.chooserRefreshState == "update" && __instance.m_isVisibleModel.Value)
                {
                    __instance.m_entriesUnlockedModel.Clear();
                    __instance.UpdateEntries();
                    APClient.chooserRefreshState = "toggle";
                }
            }
        }

        [HarmonyPatch(typeof(SeedChooserEntryModel), nameof(SeedChooserEntryModel.UpdateModelData))]
        public static class SeedChooserEntryUpdateModelDataPatch
        {
            private static void Postfix(SeedChooserEntryModel __instance)
            {
                if (APClient.easyUpgradePlants && Data.easyUpgradeCostAddons.ContainsKey(__instance.m_chosenSeed.mSeedType))
                {
                    __instance.m_sunCostModel.Value += Data.easyUpgradeCostAddons[__instance.m_chosenSeed.mSeedType];
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


        [HarmonyPatch(typeof(TransitionWhenFocusLostActivity), nameof(TransitionWhenFocusLostActivity.OnPlatformFocusChanged))] //Prevent focus change from pausing game
        internal static class IgnoreFocusChangePatch
        {
            [HarmonyPrefix]
            private static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(LevelSelectScreen), nameof(LevelSelectScreen.ShowReachedCarouselIfInLevelSelect))]
        public class ShowReachedCarouselPatch
        {
            private static bool Prefix(LevelSelectScreen __instance)
            {
                return false;
            }
        }


        [HarmonyPatch(typeof(LevelSelectScreen), nameof(LevelSelectScreen.OnEnterLevelSelect))]
        public class LevelSelectEnterPatch
        {
            private static void Postfix(LevelSelectScreen __instance)
            {
                Main.Log("LevelSelectScreen found.");

                string[] areaCarousels = new string[] { "LevelList/P_LevelSelect_Carousel_Day/Viewport/Content", "LevelList/P_LevelSelect_Carousel_Night/Viewport/Content", "LevelList/P_LevelSelect_Carousel_Pool/Viewport/Content", "LevelList/P_LevelSelect_Carousel_Fog/Viewport/Content", "LevelList/P_LevelSelect_Carousel_Roof/Viewport/Content", "LevelList/P_LevelSelect_Carousel_CloudyDay/Viewport/Content", "LevelList/P_LevelSelect_Carousel_LimboContent/Viewport/Content" };
                int levelPosition = 1;

                for (int x = 0; x < areaCarousels.Length; x++)
                {

                    Transform carouselLevels = __instance.transform.Find(areaCarousels[x]);
                    if (x == 5) //Cloudy Day levels
                    {
                        levelPosition = 109;
                    }
                    else if (x == 6) //Bonus levels
                    {
                        levelPosition = 99;
                    }

                    for (int i = 0; i < carouselLevels.childCount; i++)
                    {
                        Transform child = carouselLevels.GetChild(i);
                        if (child.name == "P_LevelSelect_ListItem(Clone)")
                        {
                            int levelId = levelPosition;
                            if (x == 5 && APClient.cloudyDayLevels == 2)
                            {
                                foreach (var property in APClient.cloudyDayUnlocks.Properties())
                                {
                                    if ((int)property.Value == levelId - 109)
                                    {
                                        levelId = property.Name.ToInt32();
                                        break;
                                    }
                                }
                            }
                            if (APClient.clearedLevels.Contains(levelId) && child.Find("ClearIndicator") == null)
                            {
                                GameObject clearIndicator = new GameObject("ClearIndicator");
                                clearIndicator.transform.SetParent(child, false);

                                Image clearIndicatorImage = clearIndicator.AddComponent<Image>();
                                Sprite clearIndicatorSprite = Data.FindSpriteByName("SPR_Challenges_ItemWindow_Trophy");
                                clearIndicatorImage.sprite = clearIndicatorSprite;
                                if (x >= 5) //For some reason, Cloudy Day and Bonus levels have their frames at a different offset
                                {
                                    clearIndicator.transform.localPosition = new Vector3(-145, 208, 0);
                                }
                                else
                                {
                                    clearIndicator.transform.localPosition = new Vector3(-145, -227, 0);
                                }
                                clearIndicator.transform.localScale = new Vector3((float)2.28, (float)2.28, (float)2.28);

                                clearIndicator.transform.SetSiblingIndex(4);
                            }
                            levelPosition += 1;
                        }
                    }
                }

                Main.Log("Added custom clear indicators.");

                //Automatically focus what the user (probably) wants to see
                int carouselNumber = (Profile.focusedLevelId - 1) / 10;
                int positionInCarousel = (Profile.focusedLevelId - 1) % 10;
                if (Profile.focusedLevelId >= 109 && Profile.focusedLevelId < 121) //Cloudy Day
                {
                    carouselNumber = 5;
                    int requiredUnlocks = (int)APClient.cloudyDayUnlocks[Profile.focusedLevelId.ToString()];
                    positionInCarousel = requiredUnlocks;
                    Main.Log($"{positionInCarousel}");
                }
                else if (Profile.focusedLevelId >= 99 && Profile.focusedLevelId < 109) //Bonus Levels
                {
                    carouselNumber = 6;
                    positionInCarousel = Profile.focusedLevelId - 99;
                }

                __instance.ShowCarousel(__instance.CarouselGroups[carouselNumber]);
                __instance.SelectLevel(positionInCarousel, true);

            }
        }
    }
}
