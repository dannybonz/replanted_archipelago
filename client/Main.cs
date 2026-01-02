using Archipelago.MultiClient.Net.Enums;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using MelonLoader;
using ReplantedArchipelago.Patches;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(ReplantedArchipelago.Main), "Replanted Archipelago", "1.0.3", "dannybonz")]
[assembly: MelonGame("PopCap Games", "PvZ Replanted")]

namespace ReplantedArchipelago
{
    public class Main : MelonMod
    {
        public static HarmonyLib.Harmony harmony;

        //Init variables for login
        public static string defaultHost;
        public static string defaultSlot;
        public static string defaultPassword;

        //Init variables for profile validation
        public static UserService userService;
        public static GameplayActivity gameplayActivity;
        public static bool creatingProfile = false;
        public static bool switchingProfile = false;
        public static bool profileValidated = false;

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
            harmony.Patch(storeGetter, postfix: new HarmonyLib.HarmonyMethod(typeof(Store).GetMethod(nameof(Store.PatchStoreEntryData))));

            APSettings.Init();
            defaultHost = APSettings.Host.Value;
            defaultSlot = APSettings.SlotName.Value;
            defaultPassword = APSettings.Password.Value;

            Application.runInBackground = true;
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
            Main.Log("Refreshing profile...");
            System.Collections.Generic.List<string> desiredGuids = new System.Collections.Generic.List<string>() { userService.ActiveUserProfile.mGuid };
            System.Collections.Generic.List<UserProfile> profiles = GetUserProfiles(userService);
            System.Collections.Generic.List<int> matchingProfileIndexes = GetMatchingProfiles(profiles, desiredGuids); //Finds the profile that matches the currently used profile
            if (matchingProfileIndexes.Count > 0)
            {
                userService.SetActiveProfile(matchingProfileIndexes[0]); //Set current profile to that profile
            }
            Main.Log("Refreshed profile.");
        }

        public static void DoProfileCheck() //Profile validation - ensures you are using the correct profile for AP
        {
            if (userService != null && APClient.currentlyConnected) //User service has been located
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
                                APClient.currentlyConnected = false;
                                Menu.ShowErrorPanel("Too Many Profiles", "There is no space to create a new profile. Please load the game with the Archipelago mod removed and delete one of your current saved profiles before trying again.");
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
}