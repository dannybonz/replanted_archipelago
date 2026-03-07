using HarmonyLib;
using Il2CppReloaded.Gameplay;
using Il2CppSource.Controllers;

namespace ReplantedArchipelago.Patches
{
    public class ZenGard
    {
        public static string queuedHint;
        public static bool hintDisplayed = false;

        [HarmonyPatch(typeof(ZenGarden), nameof(ZenGarden.CanDropPottedPlantLoot))] //Determines whether Zen Garden plants can be dropped by Zombies or not
        public static class CanDropPottedPlantLootPatch
        {
            private static bool Prefix(ZenGarden __instance, ref bool __result)
            {
                __result = (APClient.receivedItems.Contains(6) && !ZenGarden.IsZenGardenFull(true, __instance.mApp));
                return false;
            }
        }

        [HarmonyPatch(typeof(Challenge), nameof(Challenge.TreeOfWisdomGrow))] //Used when Tree of Wisdom grows
        public static class TreeOfWisdomGrowPatch
        {
            private static void Postfix(Challenge __instance)
            {
                if (__instance.mApp.BackgroundController.m_treeOfWisdomController.m_size > 5)
                {
                    if (Data.random.NextDouble() <= 0.1)
                    {
                        Main.Log("Tree of Wisdom: Success!");
                        queuedHint = APClient.GetTreeHint();
                    }
                    else
                    {
                        Main.Log("Tree of Wisdom: Bad luck.");
                        queuedHint = null;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TreeOfWisdomController), nameof(TreeOfWisdomController.ShowSpeechBubble))] //Used to display Tree of Wisdom text
        public static class TreeOfWisdomTalkPatch
        {
            private static void Postfix(TreeOfWisdomController __instance)
            {
                if (queuedHint != null)
                {
                    __instance.m_speechBubbleText.text = queuedHint;
                    hintDisplayed = true;
                }
            }
        }

        [HarmonyPatch(typeof(TreeOfWisdomController), nameof(TreeOfWisdomController.HideSpeechBubble))] //Used to when Tree of Wisdom stops talking
        public static class TreeOfWisdomStopTalkPatch
        {
            private static void Postfix(TreeOfWisdomController __instance)
            {
                if (hintDisplayed)
                {
                    queuedHint = null; //Reset queued hint
                    hintDisplayed = false;
                }
            }
        }

        [HarmonyPatch(typeof(Challenge), nameof(Challenge.TreeOfWisdomOpenStore))] //Used when Tree of Wisdom stops talking
        public static class TreeOfWisdomOpenStorePatch
        {
            private static void Postfix(TreeOfWisdomController __instance)
            {
                Main.Log("function called");
            }
        }

        [HarmonyPatch(typeof(Dialog), nameof(Dialog.Show))]
        public static class DialogShowPatch
        {
            private static bool Prefix(Dialog __instance)
            {
                if (__instance.mDialogType == Dialogs.Store)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
