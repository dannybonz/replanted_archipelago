using HarmonyLib;
using Il2CppReloaded.Services;
using Il2CppSource.DataModels;

namespace ReplantedArchipelago.Patches
{
    public class Secrets
    {
        [HarmonyPatch(typeof(PlatformDataModel), nameof(PlatformDataModel.OnActiveProfileChanged))]
        public static class ProfileChangePatch
        {
            private static void Postfix(PlatformDataModel __instance)
            {
                UpdateSecrets(__instance);
            }
        }

        private static void UpdateSecrets(PlatformDataModel __instance)
        {
            __instance.m_mustacheModeAvailableModel.Value = APClient.receivedItems.Contains(Data.itemIds["mustache"]);
            __instance.m_futureModeAvailableModel.Value = APClient.receivedItems.Contains(Data.itemIds["future"]);
            __instance.m_daisyModeAvailableModel.Value = APClient.receivedItems.Contains(Data.itemIds["daisies"]);
            __instance.m_pinataModeAvailableModel.Value = APClient.receivedItems.Contains(Data.itemIds["pinata"]);
            __instance.m_sukhbirModeAvailableModel.Value = APClient.receivedItems.Contains(Data.itemIds["sukhbir"]);
            __instance.m_trickedOutModeAvailableModel.Value = APClient.receivedItems.Contains(Data.itemIds["trickedout"]);
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetMustacheModeAvailable))]
        public static class MustacheAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["mustache"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetFutureModeAvailable))]
        public static class FutureAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["future"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetTrickedOutModeAvailable))]
        public static class TrickedOutAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["trickedout"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetDaisesModeAvailable))]
        public static class DaisiesAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["daisies"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetPinataModeAvailable))]
        public static class PinataAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["pinata"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetSukhbirModeAvailable))]
        public static class SukhbirAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["sukhbir"]);
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetDanceModeAvailable))]
        public static class DanceAvailablePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = APClient.receivedItems.Contains(Data.itemIds["dance"]);
                return false;
            }
        }
    }
}
