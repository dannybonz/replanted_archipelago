using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppReloaded.Data;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using static ReplantedArchipelago.Patches.Store;

namespace ReplantedArchipelago.Patches
{
    public class DataPatch
    {
        public static Il2CppReferenceArray<LevelEntryData> CopyLevelEntries(object oldEntriesObj, int numberOfEntries, JObject unlockRequirements, int startIndex = 0) //Used to modify level entry data - currently only used when re-ordering but maybe useful for randomising aspects of levels in the future
        {
            Il2CppSystem.Collections.Generic.IReadOnlyList<LevelEntryData> oldEntries = (Il2CppSystem.Collections.Generic.IReadOnlyList<LevelEntryData>)oldEntriesObj;
            var newEntries = new List<LevelEntryData>(); //The list is Read Only, so we can't edit it direct - instead we make a new list and then just replicate the old one
            var bonusEntries = new List<LevelEntryData>();
            var defaultUnlocks = unlockRequirements.Properties().Where(property => (int)property.Value == 0).Select(property => property.Name).ToList(); //Any entries that don't have unlock requirements will be at the start of the list

            for (int i = startIndex; i < startIndex + numberOfEntries; i++)
            {
                LevelEntryData oldEntry = oldEntries[i];
                LevelEntryData customEntry = new LevelEntryData();

                //Copy all data from old entry
                customEntry.m_backgroundPrefab = oldEntry.m_backgroundPrefab;
                customEntry.m_entryThumbnail = oldEntry.m_entryThumbnail;
                customEntry.m_entryType = oldEntry.m_entryType;
                customEntry.m_gameArea = oldEntry.m_gameArea;
                customEntry.m_gameMode = oldEntry.m_gameMode;
                customEntry.m_gameplayService = oldEntry.m_gameplayService;
                customEntry.m_initialSun = oldEntry.m_initialSun;
                customEntry.m_isLimboContent = oldEntry.m_isLimboContent;
                customEntry.m_isSpecial = oldEntry.m_isSpecial;
                customEntry.m_levelName = oldEntry.m_levelName;
                customEntry.m_levelNumber = oldEntry.m_levelNumber;
                customEntry.m_unlockThumbnail = oldEntry.m_unlockThumbnail;
                customEntry.m_zombieTypes = oldEntry.m_zombieTypes;
                customEntry.m_zombieWaves = oldEntry.m_zombieWaves;
                customEntry.m_preloadLabels = oldEntry.m_preloadLabels;
                customEntry.m_reloadedGameMode = oldEntry.m_reloadedGameMode;
                customEntry.m_order = oldEntry.m_order;

                if (Data.GameModeLevelIDs.ContainsKey(oldEntry.m_gameMode)) //Don't include unused levels (e.g. Ice Level)
                {
                    if (unlockRequirements.ContainsKey(Data.GameModeLevelIDs[oldEntry.m_gameMode].ToString())) //Level has an unlock requirement
                    {
                        int unlockRequirement = (int)unlockRequirements[Data.GameModeLevelIDs[oldEntry.m_gameMode].ToString()];

                        if (unlockRequirement == 0)
                        {
                            customEntry.m_subIndex = defaultUnlocks.IndexOf(Data.GameModeLevelIDs[oldEntry.m_gameMode].ToString());
                        }
                        else
                        {
                            customEntry.m_subIndex = unlockRequirement + 3;
                        }
                    }
                    else
                    {
                        customEntry.m_subIndex = 1000 + Data.GameModeLevelIDs[oldEntry.m_gameMode]; //Fallback if no unlock required (used for Bonus Levels)
                    }
                }

                newEntries.Add(customEntry); //Add modified entry to new list of entries
            }
            newEntries.Sort((a, b) => a.m_subIndex.CompareTo(b.m_subIndex)); //Sort by ordering

            var array = new Il2CppReferenceArray<LevelEntryData>(newEntries.Count);
            for (int i = 0; i < newEntries.Count; i++)
                array[i] = newEntries[i];

            return array;
        }

        public static void MiniGamesDataPostfix(ref object __result) //Patches the minigame order
        {
            Main.Log("DataPatch: Minigames A");

            __result = CopyLevelEntries(__result, 32, APClient.minigameUnlocks); //This includes Bonus Levels as well!

            Main.Log("DataPatch: Minigames B");

        }

        public static void SurvivalDataPostfix(ref object __result) //Removes the "Endless" survival levels - this is supposed to re-order the levels as well but for some reason it doesn't (see workaround in Menu.cs)
        {
            Main.Log("DataPatch: Survival A");

            __result = CopyLevelEntries(__result, 10, APClient.survivalUnlocks);

            Main.Log("DataPatch: Survival B");

        }

        public static void PuzzleDataPostfix(ref object __result) //Patches the puzzle order
        {
            Main.Log("DataPatch: Puzzle A");

            var vasebreakerArray = CopyLevelEntries(__result, 9, APClient.vasebreakerUnlocks).ToArray();
            var izombieUnlocks = CopyLevelEntries(__result, 9, APClient.izombieUnlocks, startIndex: 10).ToArray();
            var combinedUnlocks = vasebreakerArray.Concat(izombieUnlocks).ToArray(); //Puzzle consists of both

            __result = new Il2CppReferenceArray<LevelEntryData>(combinedUnlocks);

            Main.Log("DataPatch: Puzzle B");

        }

        public static void StoreEntryDataPostfix(ref object __result) //Patches the shop items
        {
            Main.Log("DataPatch: Store A");
            Il2CppSystem.Collections.Generic.List<StoreEntryData> newEntries = new Il2CppSystem.Collections.Generic.List<StoreEntryData>();

            for (int i = 0; i < customStoreEntries.Count; i++)
            {
                var customEntry = new StoreEntryData();
                CustomStoreEntry customEntryData = customStoreEntries[i];

                var costArray = new Il2CppStructArray<int>(1);
                costArray[0] = customEntryData.Cost;

                int pageNum = i / 8;
                int positionOnPage = i % 8;

                customEntry.m_coinCost = costArray;
                customEntry.m_entryIndex = positionOnPage;
                customEntry.m_entryDescriptionIndex = i;
                customEntry.m_order = positionOnPage;
                customEntry.m_entryName = customEntryData.Name;
                customEntry.m_pageIndex = pageNum;

                newEntries.Add(customEntry);
            }

            var array = new Il2CppReferenceArray<StoreEntryData>(newEntries.Count);
            for (int i = 0; i < newEntries.Count; i++)
                array[i] = newEntries[i];

            __result = array;

            Main.Log("DataPatch: Store B");
        }
    }
}
