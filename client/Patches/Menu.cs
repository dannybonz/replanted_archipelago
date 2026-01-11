using HarmonyLib;
using Il2Cpp;
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
using System;
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
                Data.errorTemplate = __instance.transform.parent.Find("P_UsersPanel_InvalidName").gameObject;
                Data.clientTemplate = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                Data.inputTemplate = __instance.transform.parent.Find("P_UsersPanel_Rename/Canvas/Layout/Center/Rename/NameInputField").gameObject;
                Data.logTemplate = __instance.transform.parent.Find("P_UsersPanel/Canvas/Layout/Center/Main/InsetWindow/P_UsersPanel_UserEntry").gameObject;
                Data.buttonTemplate = Data.buttonTemplate = __instance.transform.parent.Find("P_UsersPanel/Canvas/Layout/Center/Main/Buttons/P_BacicButton_Rename").gameObject;

                foreach (var binder in Data.logTemplate.GetComponentsInChildren<InputBinder>())
                    GameObject.Destroy(binder);

                foreach (var selectable in Data.logTemplate.GetComponentsInChildren<Selectable>())
                    GameObject.Destroy(selectable);

                foreach (var localizer in Data.logTemplate.GetComponentsInChildren<TextLocalizer>())
                    GameObject.Destroy(localizer);

                GameObject apSettingsButton = CreateButton("Text Client", __instance.transform.Find("Canvas/Layout/Center/Main/Menu"), ShowClientPanel);
                RectTransform apSettingsRect = apSettingsButton.GetComponent<RectTransform>();
                apSettingsRect.anchorMin = new Vector2(0, 1);
                apSettingsRect.anchorMax = new Vector2(0, 1);
                apSettingsRect.pivot = new Vector2(0, 1);
                apSettingsRect.anchoredPosition = new Vector2(310f, -20f);

                if (!APClient.currentlyConnected)
                {
                    ShowConnectionPanel();
                }
                else if (APClient.HasBoss())
                {
                    ShowErrorPanel("Goal Unlocked", "You have unlocked the final battle with Dr. Zomboss! Go fight him to complete your game!");
                }

                Profile.refreshRequired = true;
                Profile.ProcessIUserService();

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
                GameObject usersPanel = __instance.transform.parent.Find("P_UsersPanel").gameObject;
                if (usersPanel != null)
                {
                    usersPanel.SetActive(false);
                }

                if ((!APClient.currentlyConnected || APClient.apSession.Socket == null || !APClient.apSession.Socket.Connected) && !ConnectionPanel.active)
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

        public static void ShowConnectionPanel()
        {
            ConnectionPanel = GameObject.Instantiate(Data.panelTemplate, Data.panelTemplate.transform.parent);
            ConnectionPanel.name = "ConnectionPanel";
            ConnectionPanel.SetActive(true);

            Transform center = ConnectionPanel.transform.Find("Canvas/Layout/Center/Rename");
            center.Find("HeaderText").GetComponent<TextMeshProUGUI>().text = "Archipelago Setup";
            foreach (var localizer in center.Find("HeaderText").GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(localizer);

            center.Find("SubheadingText").GetComponent<TextMeshProUGUI>().text = "Host:";
            GameObject originalSubheader = center.Find("SubheadingText").gameObject;
            GameObject originalInput = center.Find("NameInputField").gameObject;
            foreach (var localizer in center.Find("SubheadingText").GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(localizer);

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
            main.Find("HeaderText").GetComponent<TextMeshProUGUI>().text = "Text Client";
            foreach (var localizer in main.Find("HeaderText").GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(localizer);

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

            if (button.TryGetComponent<TextLocalizer>(out var localiser))
                GameObject.Destroy(localiser);
            foreach (var local in button.GetComponentsInChildren<TextLocalizer>())
                GameObject.Destroy(local);
            if (button.TryGetComponent<UnityButtonBinder>(out var binder))
                GameObject.Destroy(binder);

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


        [HarmonyPatch(typeof(TransitionWhenFocusLostActivity), nameof(TransitionWhenFocusLostActivity.OnPlatformFocusChanged))] //Prevent focus change from pausing game
        internal static class IgnoreFocusChangePatch
        {
            [HarmonyPrefix]
            private static bool Prefix()
            {
                return false;
            }
        }
    }
}
