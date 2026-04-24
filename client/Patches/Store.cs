using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.Services;
using Il2CppTekly.DataModels.Models;
using Il2CppTMPro;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static ReplantedArchipelago.Data;

namespace ReplantedArchipelago.Patches
{
    public class Store
    {
        public static int pageUpdateQueued = 2;

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
                ScoutedItemInfo scoutedLocation = APClient.scoutedLocations[5000 + itemIndex];
                ItemFlags itemClass = APClient.GetPrimaryItemClassification(scoutedLocation.Flags);
                customStoreEntries[itemIndex] = new CustomStoreEntry { Name = scoutedLocation.ItemName, Class = itemClass, Cost = BaseCosts[itemClass] + (shopPrice * 10) };
                itemIndex++;
            }
        }

        public static void PatchStoreEntryData(ref object __result) //Patches the shop items
        {
            Il2CppSystem.Collections.Generic.List<StoreEntryData> newEntries = new Il2CppSystem.Collections.Generic.List<StoreEntryData>();

            for (int i = 0; i < customStoreEntries.Count; i++)
            {
                int pageNum = i / 8;
                int positionOnPage = i % 8;

                if (pageNum < APClient.shopPagesVisible)
                {
                    StoreEntryData customEntry = new StoreEntryData();
                    CustomStoreEntry customEntryData = customStoreEntries[i];

                    Il2CppStructArray<int> costArray = new Il2CppStructArray<int>(1);
                    costArray[0] = customEntryData.Cost;

                    customEntry.m_coinCost = costArray;
                    customEntry.m_entryIndex = positionOnPage;
                    customEntry.m_entryDescriptionIndex = i;
                    customEntry.m_order = positionOnPage;
                    customEntry.m_entryName = customEntryData.Name;
                    customEntry.m_pageIndex = pageNum;

                    newEntries.Add(customEntry);
                }
            }

            if (APClient.receivedItems.Contains(6) && (APClient.receivedItems.Contains(2) || APClient.receivedItems.Contains(28))) //Add Zen Garden items
            {
                int pageNum = APClient.shopPagesVisible;
                int positionOnPage = 0;
                foreach (CustomStoreEntry zenGardenEntry in zenGardenStoreEntries)
                {
                    StoreEntryData customEntry = new StoreEntryData();
                    Il2CppStructArray<int> costArray = new Il2CppStructArray<int>(1);
                    costArray[0] = zenGardenEntry.Cost;

                    customEntry.m_coinCost = costArray;
                    customEntry.m_entryIndex = positionOnPage;
                    customEntry.m_entryDescriptionIndex = 5000 + positionOnPage;
                    customEntry.m_order = positionOnPage;
                    customEntry.m_entryName = zenGardenEntry.Name;
                    customEntry.m_pageIndex = pageNum;

                    newEntries.Add(customEntry);

                    positionOnPage++;
                }
            }

            Il2CppReferenceArray<StoreEntryData> array = new Il2CppReferenceArray<StoreEntryData>(newEntries.Count);
            for (int i = 0; i < newEntries.Count; i++)
                array[i] = newEntries[i];

            __result = array;
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

                int visiblePages = APClient.shopPagesVisible;
                if (APClient.receivedItems.Contains(6)) //Add an extra page for Zen Garden items
                {
                    visiblePages++;
                }
                if (APClient.receivedItems.Contains(2) || APClient.receivedItems.Contains(28))
                {
                    for (int i = 0; i < visiblePages; i++) //Loop through desired number of visible pages
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

                    if (shopLocations.Count() > APClient.shopPrices.Count())
                    {
                        Array.Resize(ref shopLocations, APClient.shopPrices.Count());
                    }
                    APClient.apSession.Hints.CreateHints(locationIds: shopLocations);
                }

                pageUpdateQueued = 2; //2 tick delay before updating item images
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
                if (!(APClient.receivedItems.Contains(2) || APClient.receivedItems.Contains(28)))
                {
                    __result = true;
                }
                else if (Data.zenGardenItemStrings.ContainsKey(__instance.m_entryData.DescriptionIndex))
                {
                    if (__instance.m_entryData.DescriptionIndex == 5000)
                    {
                        __result = (__instance.m_userService.ActiveUserProfile.mPurchases[(int)StoreItem.Fertilizer] > 1115);
                    }
                    else if (__instance.m_entryData.DescriptionIndex == 5001)
                    {
                        __result = (__instance.m_userService.ActiveUserProfile.mPurchases[(int)StoreItem.BugSpray] > 1115);
                    }
                    else if (__instance.m_entryData.DescriptionIndex == 5002)
                    {
                        __result = (__instance.m_userService.ActiveUserProfile.mPurchases[(int)StoreItem.TreeFood] > 1119);
                    }
                }
                else
                {
                    long locationId = 5000 + __instance.m_entryData.DescriptionIndex;
                    __result = APClient.apSession.Locations.AllLocationsChecked.Contains(locationId); //If the location has been checked, mark the item as Sold Out
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.PurchaseSelectedItem))]
        public class PurchasePatch
        {
            private static bool Prefix(StoreModel __instance)
            {
                if (zenGardenItemStrings.ContainsKey(__instance.m_purchaseData.m_entryDescriptionIndex))
                {
                    if (__instance.m_purchaseData.m_entryDescriptionIndex == 5000)
                    {
                        Profile.AddConsumablePurchase(__instance.m_userService, (int)StoreItem.Fertilizer, 5);
                    }
                    else if (__instance.m_purchaseData.m_entryDescriptionIndex == 5001)
                    {
                        Profile.AddConsumablePurchase(__instance.m_userService, (int)StoreItem.BugSpray, 5);
                    }
                    else if (__instance.m_purchaseData.m_entryDescriptionIndex == 5002)
                    {
                        Profile.AddConsumablePurchase(__instance.m_userService, (int)StoreItem.TreeFood, 1);
                    }
                }
                APClient.SendLocation(5000 + __instance.m_purchaseData.m_entryDescriptionIndex, false); //Send out location when purchasing a shop item
                return true;
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.SetBubbleText))]
        public class SetBubbleTextPatch
        {
            private static bool Prefix(StoreModel __instance, int theCrazyDaveMessage, int theTime, bool theClickToContinue)
            {
                if (theCrazyDaveMessage != __instance.m_currentMessage && Data.zenGardenItemStrings.ContainsKey(theCrazyDaveMessage)) //Zen Garden shop items
                {
                    string textFromGame = Common.TodStringTranslate($"${Data.zenGardenItemStrings[theCrazyDaveMessage]}");

                    StringValueModel daveSaying = __instance.m_daveSaying;
                    daveSaying.Value = textFromGame.Substring(textFromGame.LastIndexOf('}') + 1);

                    __instance.m_currentMessage = theCrazyDaveMessage;
                    __instance.m_isDaveTalking.Value = true;
                    __instance.mBubbleCountDown = theTime;
                    __instance.mBubbleClickToContinue = theClickToContinue;
                }
                else if (theCrazyDaveMessage > 1000) //One of Dave's random filler dialogs - ignore it
                {
                    return true;
                }

                if (theCrazyDaveMessage != __instance.m_currentMessage) //If changing message index
                {
                    ScoutedItemInfo scoutedLocation = APClient.scoutedLocations[5000 + theCrazyDaveMessage];
                    ItemFlags itemClass = APClient.GetPrimaryItemClassification(scoutedLocation.Flags);

                    string[] flavourTexts;
                    if (!DaveFlavourTexts.TryGetValue(itemClass, out flavourTexts))
                    {
                        flavourTexts = DaveFlavourTexts[ItemFlags.None];
                    }
                    if (DaveCrossoverTexts.ContainsKey(scoutedLocation.ItemGame))
                    {
                        flavourTexts = DaveCrossoverTexts[scoutedLocation.ItemGame].Concat(flavourTexts).ToArray();
                    }
                    string flavourText = flavourTexts[random.Next(flavourTexts.Length)];

                    string playerNameText = "";
                    if (scoutedLocation.Player.Slot != APClient.apSession.Players.ActivePlayer.Slot)
                    {
                        playerNameText = $"<color=#EE00EE>{scoutedLocation.Player.Name}</color>'s ";
                    }

                    StringValueModel daveSaying = __instance.m_daveSaying;
                    daveSaying.Value = $"{playerNameText}<color={itemColors[itemClass]}>{scoutedLocation.ItemDisplayName}</color><br><br>{flavourText}";

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

        public static void UpdateStoreRowImages(Transform rowTransform, int itemOffset)
        {
            for (int i = rowTransform.childCount - 1; i >= 0; i--)
            {
                Sprite chosenSprite = null;
                string textColor = null;
                int yOffset = -110;
                float scaler = 1;

                int itemNumber = itemOffset + i;
                if (itemOffset >= APClient.shopPagesVisible * 8) //Zen Garden page
                {
                    if (i == 0)
                    {
                        chosenSprite = Graphics.GetGraphic("SPR_Store_Item_Fertilizer");
                    }
                    else if (i == 1)
                    {
                        chosenSprite = Graphics.GetGraphic("SPR_Store_Item_BugSpray");
                    }
                    else if (i == 2)
                    {
                        chosenSprite = Graphics.GetGraphic("SPR_Store_Item_Treefood");
                    }
                }
                else if (itemNumber < APClient.shopPrices.Count)
                {
                    ScoutedItemInfo scoutedLocation = APClient.scoutedLocations[5000 + itemNumber];
                    textColor = itemColors[APClient.GetPrimaryItemClassification(scoutedLocation.Flags)];
                    if (scoutedLocation.Player.Slot == APClient.apSession.Players.ActivePlayer.Slot)
                    {
                        if (Graphics.itemIdStoreImage.ContainsKey(scoutedLocation.ItemId))
                        {
                            chosenSprite = Graphics.GetGraphic(Graphics.itemIdStoreImage[scoutedLocation.ItemId]);
                        }
                        else if (Graphics.itemIdSpriteName.ContainsKey(scoutedLocation.ItemId))
                        {
                            chosenSprite = Graphics.GetGraphic(Graphics.itemIdSpriteName[scoutedLocation.ItemId]);
                        }
                        else if ((scoutedLocation.ItemId >= 200 && scoutedLocation.ItemId < 500) || (scoutedLocation.ItemId >= 20 && scoutedLocation.ItemId < 26))
                        {
                            chosenSprite = Graphics.GetGraphic("Key");
                        }
                        else
                        {
                            chosenSprite = Graphics.GetGraphic("SPR_Present");
                        }
                        if (scoutedLocation.ItemId == 60)
                        {
                            scaler = 0.75f;
                            yOffset = -77;
                        }
                        else if (scoutedLocation.ItemId == 61)
                        {
                            scaler = 0.8f;
                            yOffset = -83;
                        }
                    }
                    else
                    {
                        chosenSprite = Graphics.GetGraphic("ArchipelagoFlower");
                    }
                }

                if (chosenSprite != null)
                {
                    Transform child = rowTransform.GetChild(i);
                    Transform itemBackground = child.Find("Holder/ItemBackground");
                    itemBackground.localPosition = new Vector3(0, yOffset, 0);
                    itemBackground.localScale = new Vector3(scaler, scaler, 1);
                    Image imageComponent = itemBackground.GetComponentInChildren<Image>(true);

                    imageComponent.sprite = chosenSprite;
                    imageComponent.enabled = true;

                    Transform itemBackgroundDisabled = child.Find("Holder/ItemBackground_disabled");
                    itemBackgroundDisabled.localPosition = new Vector3(0, yOffset, 0);
                    itemBackgroundDisabled.localScale = new Vector3(scaler, scaler, 1);
                    Image imageComponentDisabled = itemBackgroundDisabled.GetComponentInChildren<Image>(true);
                    imageComponentDisabled.sprite = chosenSprite;
                    imageComponentDisabled.enabled = true;
                }

                if (textColor != null)
                {
                    Transform child = rowTransform.GetChild(i);
                    TextMeshProUGUI textObject = child.Find("Holder/NotComingSoonContainer/ItemText").GetComponent<TextMeshProUGUI>();
                    textObject.text = $"<color={textColor}>{textObject.text}</color>";
                }
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.OnTick))]
        public static class StoreTickPatch
        {
            private static void Postfix(StoreModel __instance)
            {
                if (__instance.m_pageSelectDelay == 0)
                {
                    if (pageUpdateQueued == 1)
                    {
                        GameObject trunkOpen = GameObject.Find("P_Store/Canvas/Layout/Center/Car/TrunkOpen"); //When viewing store direct from main menu
                        if (trunkOpen != null && trunkOpen.activeSelf)
                        {
                            GameObject storePage = GameObject.Find("P_Store/Canvas/Layout/Center/Car/StorePages/Page01"); //When viewing store direct from main menu
                            if (storePage != null)
                            {
                                Transform topRow = storePage.transform.Find("ItemListTop");
                                UpdateStoreRowImages(topRow, __instance.m_selectedPage * 8);

                                Transform bottomRow = storePage.transform.Find("ItemListBottom_HeyUpdateTheLayerOnTop");
                                UpdateStoreRowImages(bottomRow, __instance.m_selectedPage * 8 + 4);

                                pageUpdateQueued = 0;
                                Main.Log("Store: Updated page.");
                            }
                        }
                    }
                    else
                    {
                        pageUpdateQueued -= 1;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StoreModel), nameof(StoreModel.ChangePage))]
        public static class StoreChangePagePatch
        {
            private static void Postfix(StoreModel __instance)
            {
                pageUpdateQueued = 1;
            }
        }

        [HarmonyPatch(typeof(ZenGarden), nameof(ZenGarden.OpenedStore))]
        public static class ZenGardenToStorePatch
        {
            private static void Postfix(ZenGarden __instance)
            {
                pageUpdateQueued = 1;
            }
        }
    }
}