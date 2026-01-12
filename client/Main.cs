using Il2CppReloaded.Services;
using Il2CppReloaded.TreeStateActivities;
using MelonLoader;
using ReplantedArchipelago.Patches;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(ReplantedArchipelago.Main), "Replanted Archipelago", "1.2.1", "dannybonz")]
[assembly: MelonGame("PopCap Games", "PvZ Replanted")]

namespace ReplantedArchipelago
{
    public class Main : MelonMod
    {
        public static HarmonyLib.Harmony harmony;

        //Init variables for login
        public static string defaultHost;
        public static string defaultSlot;
        public static string defaultPassword;

        public static GameplayActivity cachedGameplayActivity;
        public static Queue<Data.QueuedIngameMessage> QueuedIngameMessages = new Queue<Data.QueuedIngameMessage>();
        public static string currentMessage;
        public static string currentScene;

        private void OnSceneUnloaded(Scene scene)
        {
            currentScene = null;
            cachedGameplayActivity = null;
            Log($"Scene Unload: {scene.name}");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Log($"Scene Load: {scene.name}");
            currentScene = scene.name;
        }

        public override void OnInitializeMelon()
        {
            Log("Initialising...");

            harmony = new HarmonyLib.Harmony("com.dannybonz.pvzrandomiser");

            SceneManager.sceneUnloaded += new Action<Scene>(OnSceneUnloaded);
            SceneManager.sceneLoaded += new Action<Scene, LoadSceneMode>(OnSceneLoaded);

            //Patch DataService getters
            var storeEntryProperty = typeof(DataService).GetProperty("StoreEntryData", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo storeGetter = storeEntryProperty.GetGetMethod();
            harmony.Patch(storeGetter, postfix: new HarmonyLib.HarmonyMethod(typeof(Store).GetMethod(nameof(Store.PatchStoreEntryData))));

            APSettings.Init();
            defaultHost = APSettings.Host.Value;
            defaultSlot = APSettings.SlotName.Value;
            defaultPassword = APSettings.Password.Value;

            Application.runInBackground = true;
        }

        public static void Log(string message)
        {
            MelonLogger.Msg(message);
        }
    }
}