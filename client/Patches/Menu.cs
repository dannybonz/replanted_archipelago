using HarmonyLib;
using Il2Cpp;
using Il2CppBest.HTTP.Shared.Extensions;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using Il2CppReloaded.UI;
using Il2CppSource.Binders;
using Il2CppSource.DataModels;
using Il2CppSource.TreeStateActivities;
using Il2CppTekly.DataModels.Binders;
using Il2CppTekly.DataModels.Models;
using Il2CppTekly.Localizations;
using Il2CppTMPro;
using Il2CppUI.Scripts;
using Newtonsoft.Json.Linq;
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
        private static bool darkerLog = false; //keep track of odd/even entries
        public static GameObject EnergyLinkPanel;
        public static GameObject EnergyAmountInput;
        public static GameObject EnergyLinkText;
        public static bool menuLoaded = false;
        public static bool refreshRequired = false;

        public static GameObject RemoveUnwantedComponents(GameObject gameObject, bool aggressive)
        {
            foreach (TextLocalizer localizer in gameObject.GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(localizer);
            if (aggressive)
            {
                foreach (InputBinder binder in gameObject.GetComponentsInChildren<InputBinder>())
                    GameObject.Destroy(binder);
                foreach (Selectable selectable in gameObject.GetComponentsInChildren<Selectable>())
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

                GameObject achievementsPot = __instance.transform.Find("Canvas/Layout/Center/Main/BG_Tree/AchievementsButton").gameObject;
                if (achievementsPot != null)
                {
                    achievementsPot.transform.Find("AchievementsTextTop").GetComponent<TextMeshProUGUI>().text = "To China!";
                    achievementsPot.transform.Find("AchievementsTextbottom").GetComponent<TextMeshProUGUI>().text = "To China!";
                    achievementsPot.transform.Find("AchievementsText").GetComponent<TextMeshProUGUI>().text = "To China!";

                    RemoveUnwantedComponents(achievementsPot.transform.Find("AchievementsTextTop").gameObject, false);
                    RemoveUnwantedComponents(achievementsPot.transform.Find("AchievementsTextbottom").gameObject, false);
                    RemoveUnwantedComponents(achievementsPot.transform.Find("AchievementsText").gameObject, false);

                    Button achievementsButton = achievementsPot.GetComponent<Button>();
                    achievementsButton.onClick.RemoveAllListeners();

                    Button chinaButton = __instance.transform.Find("Canvas/Layout/Center/Achievements/ScrollView/Viewport/Content/BackgroundTile (34)/DiscoverChina!/ButtonContainer/P_DiscoverChinaButton_kbm").GetComponent<Button>();
                    achievementsButton.onClick.AddListener((UnityEngine.Events.UnityAction)(() =>
                    {
                        chinaButton.onClick.Invoke();
                    }));

                    achievementsPot.SetActive(false);
                }

                Data.panelTemplate = __instance.transform.parent.Find("P_UsersPanel_Rename").gameObject;
                Data.errorTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel_InvalidName").gameObject, false);
                Data.clientTemplate = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                Data.inputTemplate = __instance.transform.parent.Find("P_UsersPanel_Rename/Canvas/Layout/Center/Rename/NameInputField").gameObject;
                Data.logTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel/Canvas/Layout/Center/Main/InsetWindow/P_UsersPanel_UserEntry").gameObject, true);
                Data.buttonTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel/Canvas/Layout/Center/Main/Buttons/P_BacicButton_Rename").gameObject, false);
                Data.subheaderTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel_InvalidName/Canvas/Layout/Center/NameConflict/SubheadingText").gameObject, false);
                Data.headerTemplate = RemoveUnwantedComponents(__instance.transform.parent.Find("P_UsersPanel_InvalidName/Canvas/Layout/Center/NameConflict/HeaderText").gameObject, false);

                GameObject apSettingsButton = CreateButton("Text Client", __instance.transform.Find("Canvas/Layout/Center/Main/Menu"), ShowClientPanel);
                RectTransform apSettingsRect = apSettingsButton.GetComponent<RectTransform>();
                apSettingsRect.anchorMin = new Vector2(0, 1);
                apSettingsRect.anchorMax = new Vector2(0, 1);
                apSettingsRect.pivot = new Vector2(0, 1);
                apSettingsRect.anchoredPosition = new Vector2(310, -20);

                GameObject apGoalButton = CreateButton("View Goal", __instance.transform.Find("Canvas/Layout/Center/Main/Menu"), ShowGoalPanel);
                RectTransform apGoalRect = apGoalButton.GetComponent<RectTransform>();
                apGoalRect.anchorMin = new Vector2(0, 1);
                apGoalRect.anchorMax = new Vector2(0, 1);
                apGoalRect.pivot = new Vector2(0, 1);
                apGoalRect.anchoredPosition = new Vector2(310, -200);

                if (APClient.energyLinkEnabled)
                {
                    GameObject energyLinkButton = CreateButton("Energy Link", __instance.transform.Find("Canvas/Layout/Center/Main/Menu"), ShowEnergyLinkPanel);
                    RectTransform energyLinkRect = energyLinkButton.GetComponent<RectTransform>();
                    energyLinkRect.anchorMin = new Vector2(0, 1);
                    energyLinkRect.anchorMax = new Vector2(0, 1);
                    energyLinkRect.pivot = new Vector2(0, 1);
                    energyLinkRect.anchoredPosition = new Vector2(310, -380);
                }

                if (!APClient.currentlyConnected)
                {
                    ShowConnectionPanel();
                }
                else if (APClient.HasBoss() && !APClient.clearedLevels.Contains(50))
                {
                    ShowErrorPanel("Goal Unlocked", "You have unlocked the final battle with Dr. Zomboss! Go fight him to complete your game!");
                }

                Main.Log("Main Menu Panel View modified.");
                menuLoaded = true;
                refreshRequired = true;
            }
        }

        [HarmonyPatch(typeof(MainMenuActivity), nameof(MainMenuActivity.ActiveStarted))]
        public class MainMenuActivityPatch
        {
            private static void Postfix(MainMenuActivity __instance)
            {
                Main.Log("MainMenuActivity started.");
                Profile.ProcessUserService();
            }
        }

        [HarmonyPatch(typeof(MainMenuPanelView), nameof(MainMenuPanelView.Update))]
        public class MainMenuUpdatePatch
        {
            private static void Postfix(MainMenuPanelView __instance)
            {
                if (Main.currentScene == "Frontend" && menuLoaded)
                {
                    if (refreshRequired)
                    {
                        Profile.ProcessUserService();
                        refreshRequired = false;
                    }

                    GameObject usersPanel = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                    if (usersPanel != null)
                    {
                        usersPanel.SetActive(false);
                    }

                    if (APClient.receivedItems.Contains(25))
                    {
                        GameObject achievementsPot = __instance.transform.Find("Canvas/Layout/Center/Main/BG_Tree/AchievementsButton").gameObject;
                        if (achievementsPot != null)
                        {
                            achievementsPot.SetActive(true);
                        }
                    }

                    if ((!APClient.currentlyConnected || APClient.apSession.Socket == null || !APClient.apSession.Socket.Connected) && (ConnectionPanel == null || !ConnectionPanel.active))
                    {
                        Main.Log("Lost connection to server.");
                        ShowConnectionPanel();
                        ShowErrorPanel("Lost Connection", "You have been disconnected from the server.");
                    }

                    if (APClient.currentlyConnected && ClientPanel != null && APClient.receivedMessages.Count > cachedMessageCount)
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
            foreach (TextLocalizer localizer in center.Find("Buttons/P_BacicButton_OK").GetComponentsInChildren<TextLocalizer>())
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
            if (APClient.tacoGoal > 0)
            {
                goalText += $"Taco Hunt: {APClient.receivedItems.Count(receivedItem => receivedItem == 27)}/{APClient.tacoGoal}<br>";
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
            if (ClientPanel != null)
            {
                ClientPanel.SetActive(true);
            }
            else
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

                VerticalLayoutGroup layout = clientLogsContainer.AddComponent<VerticalLayoutGroup>();
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;

                ContentSizeFitter fitter = clientLogsContainer.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                ScrollRect scrollRect = main.gameObject.AddComponent<ScrollRect>();
                scrollRect.viewport = insetRect;
                scrollRect.content = clientLogsRect;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Clamped;
                scrollRect.scrollSensitivity = 20f;

                //Delete old buttons
                GameObject.DestroyImmediate(main.Find("Buttons").gameObject);

                //Add text input
                messageInput = GameObject.Instantiate(Data.inputTemplate, main);
                messageInput.name = "MessageInput";
                messageInput.SetActive(true);
                messageInput.GetComponent<TMP_InputField>().onValueChanged = new TMP_InputField.OnChangeEvent();
                messageInput.GetComponent<TMP_InputField>().characterLimit = 200;

                //Position text input
                RectTransform inputRect = messageInput.GetComponent<RectTransform>();
                inputRect.anchoredPosition = new Vector2(-220, -490);
                inputRect.anchorMin = new Vector2(0.5f, 0.5f); //Anchor in the center
                inputRect.anchorMax = new Vector2(0.5f, 0.5f);
                inputRect.pivot = new Vector2(0.5f, 0.5f);     //Pivot in the center

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
                closeRect.anchorMin = new Vector2(0.5f, 0.5f); //Anchor in the center
                closeRect.anchorMax = new Vector2(0.5f, 0.5f);
                closeRect.pivot = new Vector2(0.5f, 0.5f);     //Pivot in the center
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
        }

        public static void ShowEnergyLinkPanel()
        {
            if (EnergyLinkPanel != null)
            {
                EnergyLinkPanel.SetActive(true);
            }
            else
            {
                EnergyLinkPanel = GameObject.Instantiate(Data.clientTemplate, Data.clientTemplate.transform.parent);
                EnergyLinkPanel.name = "EnergyLinkPanel";
                EnergyLinkPanel.SetActive(true);

                Transform main = EnergyLinkPanel.transform.Find("Canvas/Layout/Center/Main");
                GameObject headerObject = RemoveUnwantedComponents(main.Find("HeaderText").gameObject, true);
                headerObject.GetComponent<TextMeshProUGUI>().text = "Energy Link";
                headerObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 415);

                //Delete old buttons
                GameObject.DestroyImmediate(main.Find("Buttons").gameObject);

                //Add number input
                EnergyAmountInput = GameObject.Instantiate(Data.inputTemplate, main);
                EnergyAmountInput.name = "EnergyAmountInput";
                EnergyAmountInput.SetActive(true);
                EnergyAmountInput.GetComponent<TMP_InputField>().characterLimit = 6;
                EnergyAmountInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.IntegerNumber; //Numbers only
                EnergyAmountInput.GetComponent<TMP_InputField>().onValueChanged = new TMP_InputField.OnChangeEvent();
                EnergyAmountInput.GetComponent<TMP_InputField>().onValueChanged.AddListener((UnityEngine.Events.UnityAction<string>)OnEnergyInputChanged);
                EnergyAmountInput.GetComponent<TMP_InputField>().placeholder.GetComponent<TextMeshProUGUI>().text = "Enter amount...";
                RectTransform inputRect = EnergyAmountInput.GetComponent<RectTransform>();
                inputRect.anchoredPosition = new Vector2(-610, -305);
                inputRect.anchorMin = new Vector2(0.5f, 0.5f);
                inputRect.anchorMax = new Vector2(0.5f, 0.5f);
                inputRect.pivot = new Vector2(0.5f, 0.5f);
                inputRect.sizeDelta = new Vector2(640, inputRect.sizeDelta.y);

                //Add Deposit button
                GameObject sendButton = CreateButton("Deposit", main, DepositEnergy);
                RectTransform sendRect = sendButton.GetComponent<RectTransform>();
                sendRect.anchoredPosition = new Vector2(20, -300);
                sendRect.anchorMin = new Vector2(0.5f, 0.5f);
                sendRect.anchorMax = new Vector2(0.5f, 0.5f);
                sendRect.pivot = new Vector2(0.5f, 0.5f);
                sendRect.sizeDelta = new Vector2(600, sendRect.sizeDelta.y);

                //Add Withdraw button
                GameObject withdrawButton = CreateButton("Withdraw", main, WithdrawEnergy);
                RectTransform withdrawRect = withdrawButton.GetComponent<RectTransform>();
                withdrawRect.anchoredPosition = new Vector2(640, -300);
                withdrawRect.anchorMin = new Vector2(0.5f, 0.5f);
                withdrawRect.anchorMax = new Vector2(0.5f, 0.5f);
                withdrawRect.pivot = new Vector2(0.5f, 0.5f);
                withdrawRect.sizeDelta = new Vector2(600, withdrawRect.sizeDelta.y);

                //Add Close button
                GameObject closeButton = CreateButton("Close", main, HideEnergyLinkPanel);
                RectTransform closeRect = closeButton.GetComponent<RectTransform>();
                closeRect.anchoredPosition = new Vector2(0, -470);
                closeRect.anchorMin = new Vector2(0.5f, 0.5f);
                closeRect.anchorMax = new Vector2(0.5f, 0.5f);
                closeRect.pivot = new Vector2(0.5f, 0.5f);
                closeRect.sizeDelta = new Vector2(1880f, closeRect.sizeDelta.y);

                //Add Text
                GameObject.DestroyImmediate(main.Find("InsetWindow").gameObject);
                EnergyLinkText = CreateSubheader("EnergyLinkText", main);
                UpdateEnergyLinkText();
                RectTransform energyLinkBalanceRect = EnergyLinkText.GetComponent<RectTransform>();
                energyLinkBalanceRect.anchoredPosition = new Vector2(0, 100);
                energyLinkBalanceRect.anchorMin = new Vector2(0.5f, 0.5f);
                energyLinkBalanceRect.anchorMax = new Vector2(0.5f, 0.5f);
                energyLinkBalanceRect.pivot = new Vector2(0.5f, 0.5f);

                //Resize Window
                RectTransform windowRect = main.Find("P_Dialog").gameObject.GetComponent<RectTransform>();
                windowRect.sizeDelta = new Vector2(2187, 1250);
            }
        }

        public static void OnEnergyInputChanged(string value)
        {
            if (value != "")
            {
                UpdateEnergyLinkText();
            }
        }

        public static void UpdateEnergyLinkText()
        {
            string inputText = EnergyAmountInput.GetComponent<TMP_InputField>().text;
            long coinAmount = 1;
            if (inputText != "")
            {
                coinAmount = inputText.ToInt64() / 10;
            }

            EnergyLinkText.GetComponentInChildren<TextMeshProUGUI>().text = $"Your Balance: ${Profile.FindUserService().GetCoins() * 10}<br>Energy Link: {Data.FormatEnergyString(APClient.energyLinkBalance)}<br><br>Deposit ${coinAmount * 10} = +{Data.FormatEnergyString(coinAmount * Data.EnergyLinkDepositMultiplier)}<br>Withdraw ${coinAmount * 10} = -{Data.FormatEnergyString(coinAmount * Data.EnergyLinkWithdrawMultiplier)}";
        }

        public static void HideEnergyLinkPanel()
        {
            if (EnergyLinkPanel != null)
            {
                EnergyLinkPanel.SetActive(false);
            }
        }

        public static void WithdrawEnergy()
        {
            if (EnergyAmountInput.GetComponent<TMP_InputField>().text != "")
            {
                long coinsToWithdraw = Convert.ToInt64(EnergyAmountInput.GetComponent<TMP_InputField>().text) / 10;
                if (coinsToWithdraw > 0)
                {
                    long energyAmount = coinsToWithdraw * Data.EnergyLinkWithdrawMultiplier;
                    if (energyAmount > APClient.energyLinkBalance)
                    {
                        ShowErrorPanel("Insufficient Energy", "There isn't enough energy to do that.");
                    }
                    else
                    {
                        APClient.apSession.DataStorage[$"EnergyLink{APClient.apSession.Players.ActivePlayer.Team}"] -= energyAmount;
                        Profile.FindUserService().AddCoins((int)coinsToWithdraw);
                        ShowErrorPanel("Success!", $"You withdrew {Data.FormatEnergyString(energyAmount)} and gained ${coinsToWithdraw * 10}.");
                        UpdateEnergyLinkText();
                    }
                }
            }
        }

        public static void DepositEnergy()
        {
            if (EnergyAmountInput.GetComponent<TMP_InputField>().text != "")
            {
                long coinsDeposited = Convert.ToInt64(EnergyAmountInput.GetComponent<TMP_InputField>().text) / 10;
                if (coinsDeposited > 0)
                {
                    if (coinsDeposited > Profile.FindUserService().GetCoins())
                    {
                        ShowErrorPanel("Insufficient Balance", "You don't have enough money to do that.");
                    }
                    else
                    {
                        long energyAmount = coinsDeposited * Data.EnergyLinkDepositMultiplier;
                        APClient.apSession.DataStorage[$"EnergyLink{APClient.apSession.Players.ActivePlayer.Team}"] += energyAmount;
                        Profile.FindUserService().AddCoins((int)coinsDeposited * -1);
                        ShowErrorPanel("Success!", $"You deposited ${coinsDeposited * 10} and generated {Data.FormatEnergyString(energyAmount)}.");
                        UpdateEnergyLinkText();
                    }
                }
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
            foreach (Image img in entry.GetComponentsInChildren<Image>())
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

            Button buttonComponent = button.GetComponent<Button>();
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(onClick);
            return button;
        }

        public static GameObject CreateSubheader(string label, Transform parent)
        {
            GameObject subheading = GameObject.Instantiate(Data.subheaderTemplate, parent);
            subheading.name = label;
            subheading.GetComponentInChildren<TextMeshProUGUI>().text = label;
            return subheading;
        }

        [HarmonyPatch(typeof(GameplayService), nameof(GameplayService.OnActiveProfileChanged))] //Triggers when changing active profile
        public class ProfileChangedPatch
        {
            private static void Postfix(GameplayService __instance)
            {
                __instance.HasWatchedIntro = true;
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
                    if (APClient.easyUpgradePlants || !APClient.imitaterOpen)
                    {
                        Il2CppSystem.Collections.Generic.List<ModelReference> imitaterEntries = __instance.m_imitaterEntriesModel.m_models;
                        for (int i = imitaterEntries.Count - 1; i >= 0; i--)
                        {
                            SeedChooserEntryModel entry = imitaterEntries[i].Model.Cast<SeedChooserEntryModel>();

                            if ((!APClient.imitaterOpen && !APClient.HasSeedType(entry.m_chosenSeed.mSeedType)) || (APClient.easyUpgradePlants && Data.upgradePlants.Contains(entry.m_chosenSeed.mSeedType)))
                            {
                                imitaterEntries.RemoveAt(i);
                            }
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

        [HarmonyPatch(typeof(SeedChooserEntryModel), nameof(SeedChooserEntryModel.UpdateModelData))]
        public static class SeedChooserEntryUpdateModelDataPatch
        {
            private static void Postfix(SeedChooserEntryModel __instance)
            {
                SeedType theSeedType = __instance.m_chosenSeed.mSeedType;
                if (Data.plantStats.ContainsKey(theSeedType))
                {
                    __instance.m_sunCostModel.Value = Data.plantStats[theSeedType].Cost;
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

        [HarmonyPatch(typeof(SeedChooserScreen), nameof(SeedChooserScreen.OnStartButton))] //Triggers when displaying a warning after seed selection
        public static class OnStartButtonPatch
        {
            private static void Postfix()
            {
                RepickUI.Hide();
            }
        }

        [HarmonyPatch(typeof(TransitionWhenFocusLostActivity), nameof(TransitionWhenFocusLostActivity.OnPlatformFocusChanged))] //Prevent focus change from pausing game
        public static class IgnoreFocusChangePatch
        {
            private static bool Prefix()
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
                                foreach (JProperty property in APClient.cloudyDayUnlocks.Properties())
                                {
                                    if ((int)property.Value == levelId - 109)
                                    {
                                        levelId = property.Name.ToInt32();
                                        break;
                                    }
                                }
                            }

                            //Update Clear icon
                            if (APClient.clearedLevels.Contains(levelId) && child.Find("ClearIndicator") == null)
                            {
                                GameObject clearIndicator = new GameObject("ClearIndicator");
                                clearIndicator.transform.SetParent(child, false);

                                Image clearIndicatorImage = clearIndicator.AddComponent<Image>();
                                Sprite clearIndicatorSprite = Graphics.GetGraphic("SPR_Challenges_ItemWindow_Trophy");
                                clearIndicatorImage.sprite = clearIndicatorSprite;
                                if (x >= 5) //For some reason, Cloudy Day and Bonus levels have their frames at a different offset
                                {
                                    clearIndicator.transform.localPosition = new Vector3(-145, 208, 0);
                                }
                                else
                                {
                                    clearIndicator.transform.localPosition = new Vector3(-145, -227, 0);
                                }
                                clearIndicator.transform.localScale = new Vector3(2.28f, 2.28f, 2.28f);

                                clearIndicator.transform.SetSiblingIndex(4);
                            }

                            levelPosition += 1;
                        }
                    }
                }
                Main.Log("Added custom clear indicators.");
            }
        }

        [HarmonyPatch(typeof(LevelSelectScreen), nameof(LevelSelectScreen.ShowReachedCarouselIfInLevelSelect))]
        public class ShowReachedCarouselPrePatch
        {
            private static void Prefix(LevelSelectScreen __instance)
            {
                __instance.m_selectedCarouselGroup = __instance.CarouselGroups[4];
            }
        }

        [HarmonyPatch(typeof(LevelSelectScreen), nameof(LevelSelectScreen.ShowReachedCarouselIfInLevelSelect))]
        public class ShowReachedCarouselPatch
        {
            private static void Postfix(LevelSelectScreen __instance)
            {
                //Automatically focus what the user (probably) wants to see
                int carouselNumber = (Profile.focusedLevelId - 1) / 10;
                int positionInCarousel = (Profile.focusedLevelId - 1) % 10;
                if (Profile.focusedLevelId >= 109 && Profile.focusedLevelId < 121) //Cloudy Day
                {
                    carouselNumber = 5;
                    int requiredUnlocks = (int)APClient.cloudyDayUnlocks[Profile.focusedLevelId.ToString()];
                    positionInCarousel = requiredUnlocks;
                }
                else if (Profile.focusedLevelId >= 99 && Profile.focusedLevelId < 109) //Bonus Levels
                {
                    carouselNumber = 6;
                    positionInCarousel = Profile.focusedLevelId - 99;
                }

                __instance.m_selectedCarouselGroup = __instance.CarouselGroups[carouselNumber];
                __instance.CarouselGroups[carouselNumber].levelButton.Select();
                __instance.ShowCarousel(__instance.CarouselGroups[carouselNumber]);
                __instance.SelectLevel(positionInCarousel, true);
            }
        }

        public static class RepickUI
        {
            public static GameObject repickButton;
            public static bool repickRequested = false;

            public static void Create()
            {
                GameObject layout = GameObject.Find("Panels/P_Gameplay_MainHUD/Canvas/Layout/Center");

                if (layout != null)
                {
                    repickButton = new GameObject("RepickButton");
                    repickButton.transform.SetParent(layout.transform, false);

                    Image repickImage = repickButton.AddComponent<Image>();
                    repickImage.sprite = Graphics.GetGraphic("Repick");

                    Button buttonComponent = repickButton.AddComponent<Button>();
                    buttonComponent.onClick.AddListener(new Action(OnClick));

                    RectTransform rectTransform = repickButton.GetComponent<RectTransform>();
                    rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(-20f, -20f);
                    rectTransform.sizeDelta = new Vector2(200f, 200f);
                }
            }

            private static void OnClick()
            {
                repickRequested = true;
            }

            public static void Activate()
            {
                if (repickButton == null)
                {
                    Create();
                }
                else if (!repickButton.activeSelf)
                {
                    repickButton.SetActive(true);
                }
            }

            public static void Hide()
            {
                if (repickButton != null && repickButton.activeSelf)
                {
                    repickButton.SetActive(false);
                }
            }
        }

        public static void AddCustomTooltips()
        {
            GameObject grid = GameObject.Find("Panels/SeedChooserPanels/P_SeedChooser/Canvas/Layout/Center/Panel/SeedChooser/Grid");
            if (grid != null)
            {
                Transform transform = grid.transform;
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform plant = transform.GetChild(i);
                    if (!plant.name.Contains("P_GamePlay_SeedChooser_Item"))
                        continue;

                    string thePlantName = plant.Find("Offset/ToolTip/Name").GetComponent<TextMeshProUGUI>().text;
                    int plantIndex = System.Array.FindIndex(Data.plantNames, plantName => plantName == thePlantName);

                    TMP_Text plantDescription = plant.Find("Offset/ToolTip/Description").GetComponent<TextMeshProUGUI>();
                    if (plantDescription.text == Data.plantStats[Data.seedTypes[plantIndex]].StatsString)
                    {
                        return;
                    }
                    plantDescription.text = Data.plantStats[Data.seedTypes[plantIndex]].StatsString;

                    Transform controllerDescription = plant.Find("Offset/ControllerTipContainer/ToolTipController/Description");
                    RemoveUnwantedComponents(controllerDescription.gameObject, false);
                    controllerDescription.GetComponent<TextMeshProUGUI>().text = Data.plantStats[Data.seedTypes[plantIndex]].StatsString;
                }
            }
        }

        [HarmonyPatch(typeof(AlmanacPlantBinder), nameof(AlmanacPlantBinder.SetupPlant))]
        public class AlmanacModelPatch
        {
            private static void Postfix(AlmanacPlantBinder __instance)
            {
                if (APClient.plantStatRandomisationEnabled)
                {
                    GameObject description = GameObject.Find("GlobalPanels(Clone)/P_Almanac_Plants/Canvas/Layout/Center/Panel/SelectedItem/Scroll View/Viewport/SelectedItemInfoBox/SelectedItemInfoLabel");
                    if (description != null && Data.plantStats.ContainsKey(__instance.m_plant.mSeedType))
                    {
                        string originalText = description.GetComponent<TextMeshProUGUI>().text;
                        string tooltipText = originalText.Substring(0, originalText.IndexOf("<color=#cc241d>"));
                        description.GetComponent<TextMeshProUGUI>().text = tooltipText + "<color=#cc241d>" + "\n" + Data.plantStats[__instance.m_plant.mSeedType].StatsString; //Set stat multiplier text
                        GameObject.Find("GlobalPanels(Clone)/P_Almanac_Plants/Canvas/Layout/Center/Panel/SelectedItem/Scroll View/Viewport/SelectedItemInfoBox/InfoBox/SelectedItemCostLabel").GetComponent<TextMeshProUGUI>().text = $"Cost: {Data.plantStats[__instance.m_plant.mSeedType].Cost}"; //Set sun price
                        GameObject.Find("GlobalPanels(Clone)/P_Almanac_Plants/Canvas/Layout/Center/Panel/SelectedItem/Scroll View/Viewport/SelectedItemInfoBox/InfoBox/SelectedItemRechargeLabel").SetActive(false); //Hide the OG recharge text
                    }
                }
                else if (APClient.sunPrices.Count > 0 && Data.plantStats.ContainsKey(__instance.m_plant.mSeedType))
                {
                    GameObject sunCost = GameObject.Find("GlobalPanels(Clone)/P_Almanac_Plants/Canvas/Layout/Center/Panel/SelectedItem/Scroll View/Viewport/SelectedItemInfoBox/InfoBox/SelectedItemCostLabel");
                    if (sunCost != null)
                    {
                        sunCost.GetComponent<TextMeshProUGUI>().text = $"Cost: {Data.plantStats[__instance.m_plant.mSeedType].Cost}";
                    }
                }
            }
        }
    }
}
