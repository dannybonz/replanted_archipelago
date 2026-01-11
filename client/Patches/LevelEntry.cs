using HarmonyLib;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
using Il2CppTekly.DataModels.Models;
using System;
using System.Linq;

namespace ReplantedArchipelago.Patches
{
    public class LevelEntry
    {
        [HarmonyPatch(typeof(LevelDataModel), nameof(LevelDataModel.UpdateModelData))]
        public static class RefreshEntriesPatch
        {
            private static void Prefix(LevelDataModel __instance)
            {
                Main.Log("Modifying level entries...");
                if (APClient.currentlyConnected)
                {
                    ReorderLevels(__instance.m_miniGamesModel, "minigames", 20);
                    ReorderLevels(__instance.m_survivalModel, "survival", 10);
                    ReorderLevels(__instance.m_puzzlesModel, "puzzle", 18);
                }
                Main.Log("Modified level entries.");
            }
        }

        public static void ReorderLevels(LevelEntriesModel levelEntriesModel, string orderKey, int levelCount)
        {

            Il2CppSystem.Collections.Generic.List<ModelReference> levelEntries = levelEntriesModel.m_entriesModel.m_models;

            if (!Data.orderedLevelEntries.ContainsKey(orderKey))
            {
                LevelEntryData[] orderedLevelEntries = new LevelEntryData[levelCount];
                for (int i = 0; i < levelEntries.Count; i++)
                {
                    LevelEntryModel currentModel = levelEntries[i].Model.Cast<LevelEntryModel>();
                    if (Data.GameModeLevelIDs.ContainsKey(currentModel.m_entryData.GameMode))
                    {
                        int levelId = Data.GameModeLevelIDs[currentModel.m_entryData.GameMode];
                        int levelPosition = Array.FindIndex(Data.levelOrders[orderKey], order => order == levelId);
                        orderedLevelEntries[levelPosition] = currentModel.m_entryData;
                    }
                }
                Data.orderedLevelEntries[orderKey] = orderedLevelEntries;
            }

            for (int i = levelEntries.Count - 1; i >= 0; i--)
            {
                if (i < Data.orderedLevelEntries[orderKey].Count())
                {
                    LevelEntryModel currentModel = levelEntries[i].Model.Cast<LevelEntryModel>();
                    currentModel.m_entryData = Data.orderedLevelEntries[orderKey][i];
                }
                else
                {
                    LevelEntryModel currentModel = levelEntries[i].Model.Cast<LevelEntryModel>();
                    levelEntries.RemoveAt(i); //Remove the level entry - used for levels not present in the AP such as Endless modes
                }
            }

            levelEntriesModel.RefreshEntries();
        }
    }
}
