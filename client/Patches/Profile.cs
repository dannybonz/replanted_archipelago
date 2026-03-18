using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Il2CppReloaded.Data;
using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ReplantedArchipelago.Patches
{
    public class Profile
    {
        public static bool profileValidated = false;
        public static int focusedLevelId = 1;

        public static IUserService FindUserService()
        {
            Main.Log("Finding UserServiceActivity...");

            UserServiceActivity userServiceActivity = UnityEngine.Object.FindObjectOfType<UserServiceActivity>();
            if (userServiceActivity != null)
            {
                Main.Log("UserServiceActivity found!");
                return userServiceActivity.m_service;
            }

            Main.Log("Failed to find UserServiceActivity.");
            return null;
        }

        public static void ProcessUserService()
        {
            IUserService userService = FindUserService();
            if (userService != null)
            {
                if (profileValidated == false)
                {
                    Main.Log("Profile not validated.");
                    DoProfileCheck(userService);
                }

                if (profileValidated)
                {
                    if (Main.currentScene == "Frontend" && Menu.menuLoaded)
                    {
                        Main.Log("Refreshing profile...");
                        userService.SetActiveProfile(userService.ActiveUserIndex);
                        userService.ActiveUserProfile.mLevel = 41; //Set level back to 41 (prevents the forced 1-1?)
                    }

                    if (APClient.queuedUpCoins > 0)
                    {
                        Main.Log($"Adding {APClient.queuedUpCoins} coins...");
                        userService.AddCoins(APClient.queuedUpCoins);
                        APClient.queuedUpCoins = 0;
                    }

                    if (APClient.queuedUpPurchaseItems.Count > 0)
                    {
                        foreach (int itemId in APClient.queuedUpPurchaseItems)
                        {
                            Main.Log($"Adding purchase #{itemId}...");
                            if (Data.itemIdToPurchaseId.ContainsKey(itemId))
                            {
                                userService.SetPurchases(Data.itemIdToPurchaseId[itemId], 1);
                            }
                            else if (Data.itemIdToConsumablePurchaseId.ContainsKey(itemId))
                            {
                                AddConsumablePurchase(userService, Data.itemIdToConsumablePurchaseId[itemId], 1);
                            }
                        }
                        APClient.queuedUpPurchaseItems = new List<long>(); //Clear list
                    }

                    Main.Log("Processed UserService.");
                }
            }
        }

        public static void AddConsumablePurchase(IUserService userService, int purchaseId, int amount)
        {
            int currentAmount = userService.GetPurchases(purchaseId);
            if (currentAmount <= 1000)
            {
                userService.SetPurchases(purchaseId, 1000 + amount);
            }
            else
            {
                userService.SetPurchases(purchaseId, currentAmount + amount);
            }
        }

        public static void DoProfileCheck(IUserService userService) //Profile validation - ensures you are using the correct profile for AP
        {
            if (APClient.currentlyConnected)
            {
                List<string> desiredGuids = APClient.apSession.DataStorage[Scope.Slot, "profileGuids"]; //Previously used profiles in this run

                if (desiredGuids.Contains(userService.ActiveUserProfile.mGuid)) //If currently playing as an acceptable profile
                {
                    Main.Log("Profile Validation: Passed.");
                    profileValidated = true;
                    userService.ActiveUserProfile.mZenGardenTutorialCompleted = true; //Skip Zen Garden tutorial
                    userService.ActiveUserProfile.mLevel = 41; //Set level to 41
                    userService.SetPurchases(25, 1); //Activate other Zen Garden types
                    userService.SetPurchases(27, 1);
                    userService.SetPurchases(18, 1);
                }
                else //Current profile does not match desired guids
                {
                    profileValidated = false;

                    List<UserProfile> profiles = GetUserProfiles(userService); //Get all profiles
                    List<int> matchingProfileIndexes = GetMatchingProfiles(profiles, desiredGuids); //Of the previously accessed profiles, get any profiles that have been previously used in this AP

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
                            userService.CreateProfile(APClient.slot); //Create a new save file
                            profiles = GetUserProfiles(userService); //Update profile list
                            RegisterNewestProfile(profiles.Last());
                            userService.SetActiveProfile(profiles.IndexOf(profiles.Last()));
                            userService.ActiveUserProfile.mZenGardenTutorialCompleted = true; //Skip Zen Garden tutorial
                            userService.ActiveUserProfile.mLevel = 41; //Set level to 41
                            userService.SetPurchases(25, 1); //Activate other Zen Garden types
                            userService.SetPurchases(27, 1);
                            userService.SetPurchases(18, 1);
                            profileValidated = true;
                        }
                    }
                    else
                    {
                        Main.Log("Profile Validation: Found profile. Auto-switching...");
                        userService.SetActiveProfile(matchingProfileIndexes[0]); //Swap to correct save file
                    }
                }
            }
        }

        public static List<UserProfile> GetUserProfiles(IUserService theService) //Returns a list of all UserProfile objects
        {
            Main.Log("Profile Validation: Checking for matching profiles...");
            List<UserProfile> profiles = new List<UserProfile>();

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

        public static void RegisterNewestProfile(UserProfile profile)
        {
            List<string> profileGuids = APClient.apSession.DataStorage[Scope.Slot, "profileGuids"];
            profileGuids.Add(profile.mGuid);
            APClient.apSession.DataStorage[Scope.Slot, "profileGuids"] = JArray.FromObject(profileGuids);
            Main.Log("Profile Validation: Registered new profile.");
        }

        public static List<int> GetMatchingProfiles(List<UserProfile> profiles, List<string> profileGuids) //Returns a list of profiles that have a guid that match one of the guids in a given list
        {
            List<int> matchingGuids = new List<int>();
            foreach (UserProfile profile in profiles)
            {
                if (profileGuids.Contains(profile.mGuid))
                {
                    matchingGuids.Add(profiles.IndexOf(profile));
                }
            }
            return matchingGuids;
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.IsCompleted))] //Checks if a level is completed or not
        public static class LevelIsCompletedPatch
        {
            private static bool Prefix(UserService __instance, LevelEntryData levelEntryData, ref bool __result)
            {

                int levelId = Data.GetLevelIdFromEntryData(levelEntryData);

                if (APClient.clearedLevels.Contains(levelId))
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
    }
}