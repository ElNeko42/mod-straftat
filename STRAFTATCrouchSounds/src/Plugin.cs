using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ComputerysModdingUtilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: StraftatMod(isVanillaCompatible: false)]

namespace STRAFTATNuevasMecanicas
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; } = null!;
        public static ConfigEntry<string> MarkKey { get; private set; } = null!;

        static readonly HashSet<string> _menuScenes = new() { "MainMenu", "Menu", "main_menu", "Loading" };

        private void Awake()
        {
            Log = Logger;
            MarkKey = Config.Bind("Z-Mark Teleport", "MarkKey", "Z",
                "Key to place / use the teleport mark. Examples: Z, X, C, G, H, V, B, N, F1-F12, Mouse0, Mouse1, Mouse2");
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            gameObject.AddComponent<SettingsUI>();
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo("================================================");
            Log.LogInfo($"  {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}");
            Log.LogInfo("================================================");
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_menuScenes.Contains(scene.name)) return;
            foreach (var fpc in FindObjectsOfType<FirstPersonController>())
                AttachCrouchTracker(fpc);
        }

        public static void AttachCrouchTracker(FirstPersonController fpc)
        {
            if (fpc.GetComponent<CrouchSoundTracker>() == null)
                fpc.gameObject.AddComponent<CrouchSoundTracker>();
            if (fpc.GetComponent<ZMarkTracker>() == null)
                fpc.gameObject.AddComponent<ZMarkTracker>();
        }
    }

    [HarmonyPatch(typeof(FirstPersonController), "Awake")]
    static class Patch_FPCAwake
    {
        [HarmonyPostfix]
        static void Postfix(FirstPersonController __instance) => Plugin.AttachCrouchTracker(__instance);
    }
}
