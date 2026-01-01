using Archipelago.MultiClient.Net.Enums;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using Il2CppSource.DataModels;
using MelonLoader;
using ReplantedArchipelago.Patches;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyPatch = HarmonyLib.HarmonyPatch;

[assembly: MelonInfo(typeof(ReplantedArchipelago.Main), "Replanted Archipelago", "1.0.2", "dannybonz")]
[assembly: MelonGame("PopCap Games", "PvZ Replanted")]

namespace ReplantedArchipelago
{
    public class Main : MelonMod
    {
        public static HarmonyLib.Harmony harmony;

        //Init variables for login box
        private string hostText = "";
        private string slotText = "";
        private string passwordText = "";
        private int activeField = 0; // 0 = None, 1 = Host, 2 = Slot, 3 = Password
        private GUIStyle textStyle;
        public static bool readyToConnect = false;

        //Init variables for profile validation
        public static UserService userService;
        public static GameplayActivity gameplayActivity;
        public static bool creatingProfile = false;
        public static bool switchingProfile = false;
        public static bool profileValidated = false;
        public static bool profileRefreshRequired = false;

        public static System.Collections.Generic.Queue<Data.QueuedIngameMessage> QueuedIngameMessages = new System.Collections.Generic.Queue<Data.QueuedIngameMessage>();
        public static string currentMessage;
        public static string currentScene;

        private void OnSceneUnloaded(Scene scene)
        {
            currentScene = null;
            Main.Log($"Scene Unload: {scene.name}");
            gameplayActivity = null;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Main.Log($"Scene Load: {scene.name}");
            currentScene = scene.name;
        }

        public override void OnInitializeMelon()
        {
            Main.Log("Initialising...");

            harmony = new HarmonyLib.Harmony("com.dannybonz.pvzrandomiser");

            SceneManager.sceneUnloaded += new Action<Scene>(OnSceneUnloaded);
            SceneManager.sceneLoaded += new Action<Scene, LoadSceneMode>(OnSceneLoaded);

            //Patch DataService getters
            var storeEntryProperty = typeof(DataService).GetProperty("StoreEntryData", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo storeGetter = storeEntryProperty.GetGetMethod();
            harmony.Patch(storeGetter, postfix: new HarmonyLib.HarmonyMethod(typeof(DataPatch).GetMethod(nameof(DataPatch.StoreEntryDataPostfix))));

            var minigamesDataProperty = typeof(DataService).GetProperty("MiniGamesData", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo minigamesGetter = minigamesDataProperty.GetGetMethod();
            harmony.Patch(minigamesGetter, postfix: new HarmonyLib.HarmonyMethod(typeof(DataPatch).GetMethod(nameof(DataPatch.MiniGamesDataPostfix))));

            var survivalDataProperty = typeof(DataService).GetProperty("SurvivalEntriesData", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo survivalGetter = survivalDataProperty.GetGetMethod();
            harmony.Patch(survivalGetter, postfix: new HarmonyLib.HarmonyMethod(typeof(DataPatch).GetMethod(nameof(DataPatch.SurvivalDataPostfix))));

            var puzzleDataProperty = typeof(DataService).GetProperty("PuzzleEntryData", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo puzzleGetter = puzzleDataProperty.GetGetMethod();
            harmony.Patch(puzzleGetter, postfix: new HarmonyLib.HarmonyMethod(typeof(DataPatch).GetMethod(nameof(DataPatch.PuzzleDataPostfix))));

            textStyle = new GUIStyle();
            textStyle.fontSize = 18;
            textStyle.normal.textColor = UnityEngine.Color.white;
            APSettings.Init();

            hostText = APSettings.Host.Value;
            slotText = APSettings.SlotName.Value;
            passwordText = APSettings.Password.Value;

            Application.runInBackground = true;
        }

        public override void OnGUI()
        {
            if (APClient.connectionStatus == 1 || !readyToConnect) //If already connected or still loading the game, don't do anything 
            {
                return;
            }

            Time.timeScale = 0f; //Pause the game

            float uiWidth = 400;
            float uiHeight = 220;
            float centerX = (Screen.width - uiWidth) / 2;
            float centerY = (Screen.height - uiHeight) / 2;

            //Draw three times for a darker background
            GUI.Box(new Rect(centerX, centerY, uiWidth, uiHeight), "Archipelago Connection Setup");
            GUI.Box(new Rect(centerX, centerY, uiWidth, uiHeight), "Archipelago Connection Setup");
            GUI.Box(new Rect(centerX, centerY, uiWidth, uiHeight), "Archipelago Connection Setup");

            if (APClient.connectionStatus == 0) //Currently not connected
            {
                //Draw input boxes
                GUI.Box(new Rect(centerX + 115, centerY + 40, 270, 30), ""); //Host
                GUI.Box(new Rect(centerX + 115, centerY + 80, 270, 30), ""); //Slot
                GUI.Box(new Rect(centerX + 115, centerY + 120, 270, 30), ""); //Password

                //Draw text
                GUI.Label(new Rect(centerX + 10, centerY + 45, 100, 30), "Host:", textStyle);
                GUI.Label(new Rect(centerX + 10, centerY + 85, 100, 30), "Slot Name:", textStyle);
                GUI.Label(new Rect(centerX + 10, centerY + 125, 100, 30), "Password:", textStyle);

                //Draw text input display
                GUI.Label(new Rect(centerX + 120, centerY + 45, 240, 30), hostText + (activeField == 1 ? "|" : ""), textStyle);
                GUI.Label(new Rect(centerX + 120, centerY + 85, 240, 30), slotText + (activeField == 2 ? "|" : ""), textStyle);
                GUI.Label(new Rect(centerX + 120, centerY + 125, 240, 30), passwordText + (activeField == 3 ? "|" : ""), textStyle);

                //Draw and check "Connect" button
                if (GUI.Button(new Rect(centerX + 140, centerY + 170, 120, 30), "Connect"))
                {
                    //Update saved values
                    APSettings.Host.Value = hostText;
                    APSettings.SlotName.Value = slotText;
                    APSettings.Password.Value = passwordText;
                    APSettings.Save();

                    //Defocus inputs and change UI page
                    activeField = 0;
                    APClient.connectionStatus = 2;

                    //Attempt connection
                    APClient.AttemptConnection(hostText, slotText, passwordText);
                }

                //Update currently selected field
                if (Event.current.type == EventType.MouseDown)
                {
                    Rect hostRect = new Rect(centerX + 10, centerY + 40, 350, 30);
                    Rect slotRect = new Rect(centerX + 10, centerY + 80, 350, 30);
                    Rect passwordRect = new Rect(centerX + 10, centerY + 120, 350, 30);

                    Vector2 mousePosition = Event.current.mousePosition;

                    if (hostRect.Contains(mousePosition))
                    {
                        activeField = 1;
                    }
                    else if (slotRect.Contains(mousePosition))
                    {
                        activeField = 2;
                    }
                    else if (passwordRect.Contains(mousePosition))
                    {
                        activeField = 3;
                    }
                    else
                    {
                        activeField = 0;
                    }

                }
            }
            else if (APClient.connectionStatus == 2) //Connection failed
            {
                GUI.Label(new Rect(centerX + 20, centerY + 40, 100, 30), "Connection failed.", textStyle);
                GUI.Label(new Rect(centerX + 20, centerY + 70, 100, 30), "Ensure your connection information is", textStyle);
                GUI.Label(new Rect(centerX + 20, centerY + 90, 100, 30), "correct and try again.", textStyle);

                if (GUI.Button(new Rect(centerX + 140, centerY + 170, 120, 30), "Try Again"))
                {
                    APClient.connectionStatus = 0;
                }
            }
            else
            {
                if (APClient.connectionStatus == 3)
                {
                    GUI.Label(new Rect(centerX + 20, centerY + 40, 100, 30), "Too many profiles.", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 70, 100, 30), "There is no space to create a new profile.", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 90, 100, 30), "Please load the game with the Archipelago", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 110, 100, 30), "mod removed and delete one of your current", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 130, 100, 30), "saved profiles before trying again.", textStyle);
                }
                else if (APClient.connectionStatus == 4)
                {
                    GUI.Label(new Rect(centerX + 20, centerY + 40, 100, 30), "Version mismatch.", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 70, 100, 30), $"You are using version {Data.GenVersion} to connect", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 90, 100, 30), $"to a world generated with version {APClient.genVersion}.", textStyle);
                }
                else if (APClient.connectionStatus == 5)
                {
                    GUI.Label(new Rect(centerX + 20, centerY + 40, 100, 30), "Missing dependency.", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 70, 100, 30), $"Archipelago.MultiClient.Net.dll is missing", textStyle);
                    GUI.Label(new Rect(centerX + 20, centerY + 90, 100, 30), $"from your mods folder.", textStyle);
                }

                if (GUI.Button(new Rect(centerX + 140, centerY + 170, 120, 30), "Quit"))
                {
                    Application.Quit();
                }
            }
        }

        public override void OnUpdate()
        {
            if (APClient.connectionStatus == 1 && (APClient.apSession.Socket == null || !APClient.apSession.Socket.Connected))
            {
                APClient.HandleDisconnect();
            }

            if (APClient.connectionStatus == 0 && activeField != 0) //Not connected yet
            {
                foreach (char character in Input.inputString)
                {
                    //Get previous contents of selected field
                    string text = "";
                    if (activeField == 1)
                    {
                        text = hostText;
                    }
                    else if (activeField == 2)
                    {
                        text = slotText;
                    }
                    else if (activeField == 3)
                    {
                        text = passwordText;
                    }

                    //Special keys
                    if (character == 22) //Paste
                    {
                        text += GUIUtility.systemCopyBuffer;
                    }
                    else if (character == '\b') //Backspace
                    {
                        if (text.Length > 0)
                        {
                            text = text.Substring(0, text.Length - 1);
                        }
                    }
                    else if (character == '\n' || character == '\r' || character == '\t') //Enter or Tab
                    {
                        activeField += 1; //Move to next field
                        if (activeField == 4)
                        {
                            activeField = 0;
                        }
                    }
                    else
                    {
                        text += character; //Append typed character
                    }

                    //Update text to match new value
                    if (activeField == 1)
                    {
                        hostText = text;
                    }
                    else if (activeField == 2)
                    {
                        slotText = text;
                    }
                    else if (activeField == 3)
                    {
                        passwordText = text;
                    }
                }
            }
            else if (APClient.connectionStatus == 1 && profileRefreshRequired && UnityEngine.Application.isFocused && userService != null)
            {
                Main.Log("Profile refresh requested.");
                profileRefreshRequired = false; //Clear requirement so we don't get stuck in a loop
                RefreshProfile(); //Refresh profile
            }
        }

        public static System.Collections.Generic.List<UserProfile> GetUserProfiles(UserService theService) //Returns a list of all UserProfile objects
        {
            Main.Log("Profile Validation: Checking for matching profiles...");
            userService = theService; //Updates the stored userService object
            System.Collections.Generic.List<UserProfile> profiles = new System.Collections.Generic.List<UserProfile>();

            int index = 0;
            while (true) //Can't Count UserProfiles, so we loop until we run out of profiles to examine
            {
                try
                {
                    profiles.Add(theService.UserProfiles[index]);
                    index += 1;
                }
                catch
                {
                    break;
                }
            }

            Main.Log("Profile Validation: Found profiles.");
            return profiles;
        }

        public static System.Collections.Generic.List<int> GetMatchingProfiles(System.Collections.Generic.List<UserProfile> profiles, System.Collections.Generic.List<string> profileGuids) //Returns a list of profiles that have a guid that match one of the guids in a given list
        {
            System.Collections.Generic.List<int> matchingGuids = new System.Collections.Generic.List<int>();
            foreach (UserProfile profile in profiles)
            {
                if (profileGuids.Contains(profile.mGuid))
                {
                    matchingGuids.Add(profiles.IndexOf(profile));
                }
            }
            return matchingGuids;
        }

        public static void RefreshProfile() //Refreshes some ingame data (e.g. menu unlocks), needs to be triggered after receiving certain items
        {
            Main.Log("Refreshed profiles.");
            System.Collections.Generic.List<string> desiredGuids = new System.Collections.Generic.List<string>() { userService.ActiveUserProfile.mGuid };
            System.Collections.Generic.List<UserProfile> profiles = GetUserProfiles(userService);
            System.Collections.Generic.List<int> matchingProfileIndexes = GetMatchingProfiles(profiles, desiredGuids); //Finds the profile that matches the currently used profile
            if (matchingProfileIndexes.Count > 0)
            {
                userService.SetActiveProfile(matchingProfileIndexes[0]); //Set current profile to that profile
            }
        }

        public static void DoProfileCheck() //Profile validation - ensures you are using the correct profile for AP
        {
            if (userService != null) //User service has been located
            {
                if (creatingProfile == false) //Not actively creating a new profile
                {
                    System.Collections.Generic.List<string> desiredGuids = APClient.apSession.DataStorage[Scope.Slot, "profileGuids"]; //Previously used profiles in this run

                    if (desiredGuids.Contains(userService.ActiveUserProfile.mGuid)) //If currently playing as an acceptable profile
                    {
                        if (switchingProfile == true)
                        {
                            Main.Log("Profile Validation: Passed.");

                            switchingProfile = false;
                            profileValidated = true;

                            //Profile modifications
                            userService.ActiveUserProfile.mZenGardenTutorialCompleted = true; //Skip Zen Garden tutorial
                            return;
                        }
                        else if (profileValidated == false)
                        {
                            Main.Log("Profile Validation: Refreshing...");

                            switchingProfile = true;
                            RefreshProfile();
                        }
                    }
                    else //Current profile does not match desired guids
                    {
                        profileValidated = false;

                        System.Collections.Generic.List<UserProfile> profiles = GetUserProfiles(userService); //Get all profiles
                        System.Collections.Generic.List<int> matchingProfileIndexes = GetMatchingProfiles(profiles, desiredGuids); //Of the previously accessed profiles, get any profiles that have been previously used in this AP

                        if (matchingProfileIndexes.Count == 0)
                        {
                            Main.Log("Profile Validation: No matching profiles found.");

                            if (userService.NumUserProfiles >= 8)
                            {
                                Main.Log("Profile Validation: Too many profiles.");
                                APClient.connectionStatus = 3;
                            }
                            else
                            {
                                Main.Log("Profile Validation: Creating new profile...");
                                creatingProfile = true;
                                userService.CreateProfile(APClient.slot); //Create a new save file
                            }
                        }
                        else
                        {
                            Main.Log("Profile Validation: Auto-switching profile...");
                            userService.SetActiveProfile(matchingProfileIndexes[0]); //Swap to correct save file
                            switchingProfile = true;
                        }
                    }
                }
            }
        }

        public static void Log(string message)
        {
            MelonLogger.Msg(message);
        }
    }

    [HarmonyPatch(typeof(LoadingDataModel), nameof(LoadingDataModel.OnTick))] //Triggered every tick during the initial game loading screen
    public class LoadPatch
    {
        private static void Postfix(LoadingDataModel __instance)
        {
            if (APClient.connectionStatus != 1) //Not yet connected
            {
                __instance.m_minLoadingTime = __instance.m_curLoadTime + 1; //Prevent loading from finishing until connection complete
                if (__instance.m_curLoadTime >= 0.5 && !Main.readyToConnect) //Wait until background art has faded in
                {
                    Main.readyToConnect = true; //Ready to display login box

                    var loaded = AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "Archipelago.MultiClient.Net");
                    if (!loaded)
                    {
                        APClient.connectionStatus = 5;
                    }
                }
            }
            else if (APClient.connectionStatus == 1)
            {
                __instance.m_minLoadingTime = 0; //Complete loading ASAP
                __instance.m_curLoadTime = 100;
                Main.readyToConnect = false; //Already connected, no need to connect anymore

                //No need to patch loading anymore - prevent crashes?
                Main.harmony.Unpatch(typeof(LoadingDataModel).GetMethod(nameof(LoadingDataModel.OnTick)), HarmonyLib.HarmonyPatchType.Postfix);
            }
        }
    }
}