using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using Il2CppReloaded.DataModels;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppTekly.DataModels.Models;
using static ReplantedArchipelago.Data;

namespace ReplantedArchipelago.Patches
{
    public class Store
    {
        public class CustomStoreEntry
        {
            public string Name { get; set; }
            public ItemFlags Class { get; set; }
            public int Cost { get; set; }
        }

        public static System.Collections.Generic.Dictionary<int, CustomStoreEntry> customStoreEntries;

        public static void UpdateCustomEntries()
        {
            customStoreEntries = new System.Collections.Generic.Dictionary<int, CustomStoreEntry> { }; //Reset store entries

            int itemIndex = 0;
            foreach (int shopPrice in APClient.shopPrices)
            {
                Archipelago.MultiClient.Net.Models.ScoutedItemInfo scoutedLocation = APClient.scoutedLocations[5000 + itemIndex];
                ItemFlags itemClass = scoutedLocation.Flags;
                customStoreEntries[itemIndex] = new CustomStoreEntry { Name = scoutedLocation.ItemName, Class = itemClass, Cost = BaseCosts[itemClass] + (shopPrice * 10) };
                itemIndex++;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetPurchases), new[] { typeof(int) })]
        public static class GetPurchasesIndexPatch
        {
            private static bool Prefix(UserService __instance, int index, ref int __result)
            {
                if (index == 21 ||
                    (index == 22 && APClient.receivedItems.Contains(Data.itemIds["Pool Cleaners"])) ||
                    (index == 23 && APClient.receivedItems.Contains(Data.itemIds["Roof Cleaners"])) ||
                    (index == 29 && APClient.receivedItems.Contains(Data.itemIds["Wall-nut First Aid"])) ||
                    (index == 8 && APClient.receivedItems.Contains(Data.itemIds["Imitater"]))
                    )
                {
                    __result = 1;
                }
                else
                {
                    __result = 0;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(UserService), nameof(UserService.GetPurchases), new[] { typeof(StoreItem) })]
        public static class GetPurchasesItemPatch
        {
            private static bool Prefix(UserService __instance, StoreItem item, ref int __result)
            {
                if (item == StoreItem.PacketUpgrade ||
                    (item == StoreItem.PoolCleaner && APClient.receivedItems.Contains(Data.itemIds["Pool Cleaners"])) ||
                    (item == StoreItem.RoofCleaner && APClient.receivedItems.Contains(Data.itemIds["Roof Cleaners"])) ||
                    (item == StoreItem.Firstaid && APClient.receivedItems.Contains(Data.itemIds["Wall-nut First Aid"])) ||
                    (item == StoreItem.PlantImitater && APClient.receivedItems.Contains(Data.itemIds["Imitater"]))
                    )
                {
                    __result = 1;
                }
                else
                {
                    __result = 0;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.UpdatePages))]
        public static class StorePagesPatch
        {
            private static void Postfix(StoreModel __instance)
            {
                __instance.m_pagesModel.Clear(); //Delete any pages already made by the game

                IDataService dataService = __instance.m_dataService;
                ObjectModel pagesModel = __instance.m_pagesModel;
                IUserService userService = __instance.m_userService;

                for (int i = 0; i < APClient.shopPagesVisible; i++) //Loop through desired number of visible pages
                {
                    StorePageModel model = new StorePageModel(dataService, userService, __instance, i);
                    pagesModel.Add($"{i}", model);
                }

                //Create hints for accessible items
                long[] shopLocations = new long[APClient.shopPagesVisible * 8];
                for (long i = 0; i < APClient.shopPagesVisible * 8; i++)
                {
                    shopLocations[i] = 5000 + i;
                }
                APClient.apSession.Hints.CreateHints(locationIds: shopLocations);
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.IsPageShown))]
        public static class PageShownPatch
        {
            private static bool Prefix(StoreModel __instance, ref bool __result, StorePage thePage)
            {
                __result = (int)thePage <= APClient.shopPagesVisible; //Decides whether a page should be marked as visible or not by the game
                return false;
            }
        }

        [HarmonyPatch(typeof(StoreEntryModel), nameof(StoreEntryModel.IsItemSoldOut))]
        public class SoldOutPatch
        {
            private static bool Prefix(StoreEntryModel __instance, ref bool __result)
            {
                long locationId = 5000 + __instance.m_entryData.DescriptionIndex;
                __result = APClient.apSession.Locations.AllLocationsChecked.Contains(locationId); //If the location has been checked, mark the item as Sold Out
                return false;
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.PurchaseSelectedItem))]
        public class PurchasePatch
        {
            private static bool Prefix(StoreModel __instance)
            {
                APClient.SendLocation(5000 + __instance.m_purchaseData.m_entryDescriptionIndex); //Send out location when purchasing a shop item
                return true;
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.SetBubbleText))]
        public class SetBubbleTextPatch
        {
            private static bool Prefix(StoreModel __instance, int theCrazyDaveMessage, int theTime, bool theClickToContinue)
            {
                if (theCrazyDaveMessage > 1000) //One of Dave's random filler dialogs - ignore it
                {
                    return true;
                }

                if (theCrazyDaveMessage != __instance.m_currentMessage) //If changing message index
                {
                    Archipelago.MultiClient.Net.Models.ScoutedItemInfo scoutedLocation = APClient.scoutedLocations[5000 + theCrazyDaveMessage];
                    ItemFlags itemClass = scoutedLocation.Flags;

                    string[] flavourTexts;
                    if (!DaveFlavourTexts.TryGetValue(scoutedLocation.Flags, out flavourTexts))
                    {
                        flavourTexts = DaveFlavourTexts[ItemFlags.None];
                    }
                    System.Random rnd = new System.Random();
                    string flavourText = flavourTexts[rnd.Next(flavourTexts.Length)];

                    string classificationName = "Other";
                    if (itemClass == ItemFlags.Advancement)
                    {
                        classificationName = "Progression";
                    }
                    else if (itemClass == ItemFlags.NeverExclude)
                    {
                        classificationName = "Useful";
                    }
                    else if (itemClass == ItemFlags.Trap)
                    {
                        classificationName = "Trap";
                    }
                    else if (itemClass == ItemFlags.None)
                    {
                        classificationName = "Filler";
                    }

                    StringValueModel daveSaying = __instance.m_daveSaying;
                    daveSaying.Value = $"<b>{scoutedLocation.ItemDisplayName}</b> for <b>{scoutedLocation.Player.Name}</b> <i>({classificationName})</i><br><br>{flavourText}";

                    __instance.m_currentMessage = theCrazyDaveMessage;
                    __instance.m_isDaveTalking.Value = true;
                    __instance.mBubbleCountDown = theTime;
                    __instance.mBubbleClickToContinue = theClickToContinue;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(CutScene), nameof(CutScene.CanGetPacketUpgrade))]
        public class CanGetPacketUpgradePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(CutScene), nameof(CutScene.CanGetSecondPacketUpgrade))]
        public class CanGetSecondPacketUpgradePatch
        {
            private static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
    }
}