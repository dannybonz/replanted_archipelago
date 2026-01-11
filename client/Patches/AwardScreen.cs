using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Il2CppReloaded.Data;
using Il2CppReloaded.DataModels;
using Il2CppReloaded.Gameplay;
using Il2CppReloaded.TreeStateActivities;
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

            if (itemInfo != null)
            {
                //Set header
                string headerText = "You sent an item!";
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
                awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetHeaderText").GetComponentInChildren<TMP_Text>(true).text = itemInfo.ItemName;

                //Set description
                if (itemInfo.Player.Name != APClient.slot)
                {
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = $"Sent {itemInfo.ItemName} to {itemInfo.Player.Name}.";
                }
                else if (itemInfo.ItemId >= 100 && itemInfo.ItemId < 200)
                {
                    PlantDefinition plantDefinition = gameplayActivity.m_dataService.GetPlantDefinition(Data.seedTypes[itemInfo.ItemId - 100]);
                    string descriptionText = Common.TodStringTranslate($"${plantDefinition.m_plantToolTip}");
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = descriptionText;
                }
                else if (Data.itemIdDefaultTooltips.ContainsKey(itemInfo.ItemId))
                {
                    string descriptionText = Common.TodStringTranslate($"${Data.itemIdDefaultTooltips[itemInfo.ItemId]}");
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = descriptionText;
                }
                else
                {
                    string descriptionText = $"You found your {itemInfo.ItemName}!";
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/InsetBodyText").GetComponentInChildren<TMP_Text>(true).text = descriptionText;
                }

                //Set image
                if (itemInfo.Player.Name == APClient.slot)
                {
                    if (Data.itemIdSpriteName.ContainsKey(itemInfo.ItemId))
                    {
                        awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/SeedPacket").GetComponentInChildren<Image>(true).sprite = Data.FindSpriteByName(Data.itemIdSpriteName[itemInfo.ItemId]);
                    }
                    else
                    {
                        awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/SeedPacket").GetComponentInChildren<Image>(true).sprite = Data.FindSpriteByName("SPR_PresentOpen");
                    }
                }
                else
                {
                    awardScreen.transform.Find("Canvas/Layout/Center/Panel/Inset/SeedPacket").GetComponentInChildren<Image>(true).sprite = Data.FindSpriteByName("SPR_Present");
                }
            }
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
                    var entry = model.Model.Cast<AwardScreenEntryModel>();
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
