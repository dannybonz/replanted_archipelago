using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Il2CppReloaded.Data;
using Il2CppReloaded.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ReplantedArchipelago.Patches
{
    public class Profile
    {
        public static IUserService cachedUserService;
        public static bool profileValidated = false;
        public static bool refreshRequired = true;
        public static int focusedLevelId = 1;

        public static void ProcessIUserService(IUserService userService = null) //Checks with APClient to see if anything has to be done related to the given userService
        {
            if (userService == null)
            {
                if (cachedUserService == null)
                {
                    Main.Log("No User Service has been found.");
                    return;
                }
                Main.Log("Using cached User Service.");
                userService = cachedUserService;
            }
            else
            {
                cachedUserService = userService;
            }

            if (profileValidated == false)
            {
                DoProfileCheck(userService);
            }

            if (APClient.queuedUpCoins > 0)
            {
                userService.AddCoins(APClient.queuedUpCoins);
                APClient.queuedUpCoins = 0;
            }

            if (APClient.queuedUpPurchases.Count > 0)
            {
                foreach (int purchaseId in APClient.queuedUpPurchases)
                {
                    userService.ActiveUserProfile.mPurchases[purchaseId] = 1;
                }
                APClient.queuedUpPurchases = new List<int>();
            }

            if (refreshRequired)
            {
                Main.Log("Refreshing profile...");
                userService.SetActiveProfile(userService.ActiveUserIndex);
                refreshRequired = false;
            }
        }

        public static void DoProfileCheck(IUserService userService = null) //Profile validation - ensures you are using the correct profile for AP
        {
            if (APClient.currentlyConnected)
            {
                if (userService == null)
                {
                    if (cachedUserService == null)
                    {
                        Main.Log("No user service has been found.");
                        return;
                    }
                    userService = cachedUserService;
                }

                List<string> desiredGuids = APClient.apSession.DataStorage[Scope.Slot, "profileGuids"]; //Previously used profiles in this run

                if (desiredGuids.Contains(userService.ActiveUserProfile.mGuid)) //If currently playing as an acceptable profile
                {
                    Main.Log("Profile Validation: Passed.");
                    profileValidated = true;
                    userService.ActiveUserProfile.mZenGardenTutorialCompleted = true; //Skip Zen Garden tutorial
                    userService.ActiveUserProfile.mLevel = 50; //Set level to 50
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