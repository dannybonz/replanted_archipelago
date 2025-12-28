using HarmonyLib;
using Il2CppReloaded.Services;
using System.Linq;

namespace ReplantedArchipelago.Patches
{
    public class Profile
    {
        [HarmonyPatch(typeof(UserService), nameof(UserService.LoadProfileData))] //Triggers when you boot up the game to get default profile data
        public class LoadProfilePatch
        {
            private static void Postfix(UserService __instance)
            {
                Main.GetUserProfiles(__instance);
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.SetActiveProfile))] //Changes profile
        public class ChangeProfilePatch
        {
            private static void Postfix(UserService __instance, ref bool __result)
            {
                Main.DoProfileCheck();
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.CreateProfile))] //Triggers when you create a profile
        public class CreateProfilePatch
        {
            private static void Postfix(UserService __instance, ref bool __result)
            {
                System.Collections.Generic.List<UserProfile> profiles = Main.GetUserProfiles(__instance);
                UserProfile newProfile = profiles.Last();
                APClient.RegisterProfile(newProfile);
            }
        }
    }

    [HarmonyPatch(typeof(GameplayService), nameof(GameplayService.OnActiveProfileChanged))] //Triggers when changing active profile
    public class ProfileChangedPatch
    {
        private static void Postfix(GameplayService __instance)
        {
            __instance.HasWatchedIntro = true;
        }
    }
}
