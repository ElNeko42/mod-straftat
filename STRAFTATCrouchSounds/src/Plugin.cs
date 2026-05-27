using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using ComputerysModdingUtilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: StraftatMod(isVanillaCompatible: true)]

namespace STRAFTATCrouchSounds
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; } = null!;

        static readonly HashSet<string> _menuScenes = new() { "MainMenu", "Menu", "main_menu", "Loading" };

        private void Awake()
        {
            Log = Logger;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
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
                AttachTracker(fpc);
        }

        public static void AttachTracker(FirstPersonController fpc)
        {
            if (fpc.GetComponent<CrouchSoundTracker>() == null)
                fpc.gameObject.AddComponent<CrouchSoundTracker>();
        }
    }

    // También engancha cuando el FPC se crea después de la escena
    [HarmonyPatch(typeof(FirstPersonController), "Awake")]
    static class Patch_FPCAwake
    {
        [HarmonyPostfix]
        static void Postfix(FirstPersonController __instance) => Plugin.AttachTracker(__instance);
    }
}
