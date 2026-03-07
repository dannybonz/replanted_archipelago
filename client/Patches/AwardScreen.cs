using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.TreeStateActivities;
using Il2CppSource.Utils;
using Il2CppTekly.DataModels.Models;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReplantedArchipelago.Patches
{
    internal class AwardScreen
    {
        public static void EditAwardScreen(GameplayActivity gameplayActivity) //Used to adjust the UI elements
        {
            GameObject awardScreen = GameObject.Find("GlobalPanels(Clone)/P_AwardScreen");
            if (awardScreen == null)
            {
                return;
            }

            int clearLocationId = Data.AllLevelLocations[Data.GetLevelIdFromGameplayActivity(gameplayActivity)].ClearLocation;
            ScoutedItemInfo itemInfo = null;
            if (APClient.scoutedLocations != null && APClient.scoutedLocations.ContainsKey(clearLocationId))
            {
                itemInfo = APClient.scoutedLocations[clearLocationId];
            }

            //Remove Main Menu button
            GameObject.Find("GlobalPanels(Clone)/P_AwardScreen/Canvas/Layout/Center/Panel/MainMenu").SetActive(false);

            //Change text on continue button
            GameObject.Find("GlobalPanels(Clone)/P_AwardScreen/Canvas/Layout/Center/Panel/VisiblityBinderContainer/ViewPlantsButton/ButtonText").GetComponent<TMP_Text>().text = "Continue";
            Button continueButton = GameObject.Find("GlobalPanels(Clone)/P_AwardScreen/Canvas/Layout/Center/Panel/VisiblityBinderContainer/ViewPlantsButton").GetComponent<Button>();
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener((System.Action)ContinueButtonClicked);

            if (itemInfo != null)
            {
                //Set header
                string headerText = "You found an item!";
                if (itemInfo.Player.Name == APClient.slot)
                {
                    headerText = "You got an item!";
                    if (itemInfo.ItemId >= 100 && itemInfo.ItemId < 200)
                    {
                        headerText = "You got a new plant!";
                    }
                }

                awardScreen.transform.Find("Canvas/Layout/Center/Panel/Header/HeaderBackground/HeaderText").GetComponentInChildren<TMP_Text>(true).text = headerText;

                //Set subheader
                awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetHeaderText").GetComponentInChildren<TMP_Text>(true).text = itemInfo.ItemDisplayName;

                //Set description
                bool isForMe = (itemInfo.Player.Slot == APClient.apSession.Players.ActivePlayer.Slot);
                if (isForMe && itemInfo.ItemId >= 100 && itemInfo.ItemId < 200)
                {
                    PlantDefinition plantDefinition = gameplayActivity.m_dataService.GetPlantDefinition(Data.seedTypes[itemInfo.ItemId - 100]);
                    string descriptionText = Common.TodStringTranslate($"${plantDefinition.m_plantToolTip}");
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = descriptionText;
                }
                else if (isForMe && Data.itemIdDefaultTooltips.ContainsKey(itemInfo.ItemId))
                {
                    string descriptionText = Common.TodStringTranslate($"${Data.itemIdDefaultTooltips[itemInfo.ItemId]}");
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = descriptionText;
                }
                else if (isForMe && itemInfo.ItemId >= 200 && itemInfo.ItemId < 500)
                {
                    int levelId = (int)itemInfo.ItemId - 200;
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetHeaderText").GetComponentInChildren<TMP_Text>(true).text = "Level Unlock";
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = $"{Data.AllLevelLocations[levelId].Name}";
                }
                else if (isForMe && Data.itemIdCustomDescriptions.ContainsKey(itemInfo.ItemId))
                {
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = Data.itemIdCustomDescriptions[itemInfo.ItemId];
                }
                else
                {
                    ItemFlags itemClassification = APClient.GetPrimaryItemClassification(itemInfo.Flags);
                    string itemText = "A filler item";
                    if (itemClassification == ItemFlags.Advancement)
                    {
                        itemText = "A progression item";
                    }
                    else if (itemClassification == ItemFlags.NeverExclude)
                    {
                        itemText = "A useful item";
                    }
                    else if (itemClassification == ItemFlags.Trap)
                    {
                        itemText = "A trap";
                    }

                    string playerText = "";
                    if (!isForMe)
                    {
                        playerText = $"for {itemInfo.Player.Name}";
                    }

                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = $"{itemText} {playerText}";
                }

                //Set image
                Transform awardItem = awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/SeedPacket");
                if (itemInfo.Player.Name == APClient.slot)
                {
                    if (Graphics.itemIdSpriteName.ContainsKey(itemInfo.ItemId))
                    {
                        if (itemInfo.ItemId >= 60 && itemInfo.ItemId <= 61)
                        {
                            awardItem.localScale = new Vector3(1.25f, 1.25f, 1);
                        }
                        else if (itemInfo.ItemId < 100)
                        {
                            awardItem.localScale = new Vector3(1.8f, 1.8f, 1);
                        }
                        awardItem.GetComponentInChildren<Image>(true).sprite = Graphics.GetGraphic(Graphics.itemIdSpriteName[itemInfo.ItemId]);
                    }
                    else if (itemInfo.ItemId >= 200 && itemInfo.ItemId < 500)
                    {
                        awardItem.GetComponentInChildren<Image>(true).sprite = Graphics.GetGraphic("Key");
                        awardItem.localScale = new Vector3(1.8f, 1.8f, 1);
                    }
                    else
                    {
                        awardItem.GetComponentInChildren<Image>(true).sprite = Graphics.GetGraphic("SPR_PresentOpen");
                    }
                }
                else
                {
                    awardItem.GetComponentInChildren<Image>(true).sprite = Graphics.GetGraphic("Archipelago");
                }
                awardItem.gameObject.GetComponentInChildren<Image>(true).enabled = true;
            }
        }

        public static void ContinueButtonClicked()
        {
            string transitionName = Data.GetTransitionNameFromLevelId(Data.GetLevelIdFromGameplayActivity(Main.cachedGameplayActivity));
            Main.Log($"Transition Requested: {transitionName}");
            StateTransitionUtils.Transition(transitionName);
        }

        [HarmonyPatch(typeof(AwardScreenDataModel), nameof(AwardScreenDataModel.UpdateEntries))]
        public class AwardScreenDataPatch
        {
            private static void Postfix(AwardScreenDataModel __instance)
            {
                ObjectModel entriesModel = __instance.m_entriesModel;
                Il2CppSystem.Collections.Generic.List<ModelReference> models = entriesModel.m_models;
                foreach (ModelReference model in models)
                {
                    AwardScreenEntryModel entry = model.Model.Cast<AwardScreenEntryModel>();
                    entry.m_isAlmanacModel.Value = false;
                    entry.m_showAwardNameModel.Value = true;
                    entry.m_isNoteModel.Value = false;
                }
            }
        }

        [HarmonyPatch(typeof(AwardScreenDataModel), nameof(AwardScreenDataModel.GetTransitionName))] //Decides where the game will go after closing the level's award screen
        public class AwardScreenTransitionPatch
        {
            private static bool Prefix(AwardScreenDataModel __instance, ref string __result)
            {
                if (__result == null)
                {
                    __result = Data.GetTransitionNameFromLevelId(Data.GetLevelIdFromGameplayActivity(__instance.m_gameplayActivity));
                }
                return false;
            }
        }
    }
}
