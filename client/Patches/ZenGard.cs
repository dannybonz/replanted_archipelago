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

        [HarmonyPatch(typeof(TreeOfWisdomController), nameof(TreeOfWisdomController.HideSpeechBubble))] //Used when Tree of Wisdom stops talking
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

        public static void AddRandomZenGardenPlant(ZenGarden zenGarden)
        {
            if (!ZenGarden.IsZenGardenFull(true, zenGarden.mApp))
            {
                PottedPlant thePottedPlant = new PottedPlant();
                SeedType[] pottedPlantTypes = { SeedType.Peashooter, SeedType.Sunflower, SeedType.Cherrybomb, SeedType.Wallnut, SeedType.Potatomine, SeedType.Snowpea, SeedType.Chomper, SeedType.Repeater, SeedType.Puffshroom, SeedType.Sunshroom, SeedType.Fumeshroom, SeedType.Gravebuster, SeedType.Hypnoshroom, SeedType.Scaredyshroom, SeedType.Iceshroom, SeedType.Doomshroom, SeedType.Lilypad, SeedType.Squash, SeedType.Threepeater, SeedType.Tanglekelp, SeedType.Jalapeno, SeedType.Spikeweed, SeedType.Torchwood, SeedType.Tallnut, SeedType.Seashroom, SeedType.Plantern, SeedType.Cactus, SeedType.Blover, SeedType.Splitpea, SeedType.Starfruit, SeedType.Pumpkinshell, SeedType.Magnetshroom, SeedType.Cabbagepult, SeedType.Kernelpult, SeedType.InstantCoffee, SeedType.Garlic, SeedType.Umbrella, SeedType.Marigold, SeedType.Melonpult };
                thePottedPlant.InitializePottedPlant(pottedPlantTypes[Data.random.Next(pottedPlantTypes.Length)]);
                zenGarden.AddPottedPlant(thePottedPlant);
            }
        }
    }
}
