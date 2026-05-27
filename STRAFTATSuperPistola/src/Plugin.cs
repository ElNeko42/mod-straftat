using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using ComputerysModdingUtilities;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: StraftatMod(isVanillaCompatible: false)]

namespace STRAFTATSuperPistola
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; } = null!;
        public static Plugin Instance   { get; private set; } = null!;

        static readonly HashSet<string> _menuScenes = new() { "MainMenu", "Menu", "main_menu", "Loading" };

        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            Log = Logger;
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();
            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo("================================================");
            Log.LogInfo($"  {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}");
            Log.LogInfo("================================================");
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_menuScenes.Contains(scene.name)) return;
            Log.LogInfo($"[SuperPistola] Escena cargada: {scene.name} — buscando jugador...");
            StartCoroutine(SpawnWeaponWhenReady());
        }

        IEnumerator SpawnWeaponWhenReady()
        {
            FirstPersonController fpc = null;
            float waited = 0f;
            while (fpc == null && waited < 15f)
            {
                yield return new WaitForSeconds(0.5f);
                waited += 0.5f;
                fpc = FindObjectOfType<FirstPersonController>();
                Log.LogInfo($"[SuperPistola] Buscando jugador... {waited}s  fpc:{fpc != null}");
            }

            if (fpc == null) { Log.LogWarning("[SuperPistola] FirstPersonController no encontrado tras 15s."); yield break; }

            WeaponFactory.SpawnNearPlayer(fpc);
        }
    }

    // Cachea el prefab al cargar armas
    [HarmonyPatch(typeof(SpawnerManager), "PopulateAllWeapons")]
    static class Patch_PopulateAllWeapons
    {
        [HarmonyPostfix]
        static void Postfix() => WeaponFactory.Initialize();
    }

    // Añade la Super Pistola al pool de armas random
    [HarmonyPatch(typeof(Resources), "LoadAll", new[] { typeof(string), typeof(System.Type) })]
    static class Patch_RandomWeapons
    {
        [HarmonyPostfix]
        static void Postfix(string path, ref Object[] __result)
        {
            if (path != "RandomWeapons") return;
            if (WeaponFactory.ClonePrefab == null) return;

            var list = new List<Object>(__result) { WeaponFactory.ClonePrefab };
            __result = list.ToArray();
            Plugin.Log.LogInfo("[SuperPistola] Añadida al pool RandomWeapons.");
        }
    }

    // Re-registra en FishNet cada vez que el NetworkManager arranca (igual que KokiWeapons)
    [HarmonyPatch(typeof(FishNet.Managing.NetworkManager), "Start")]
    static class Patch_NetworkManagerStart
    {
        [HarmonyPostfix]
        static void Postfix() => WeaponFactory.RegisterWithFishNet();
    }

    // Intercepta el disparo
    [HarmonyPatch(typeof(WeaponHandSpawner), nameof(WeaponHandSpawner.Fire))]
    static class Patch_Fire
    {
        [HarmonyPrefix]
        static bool Prefix(WeaponHandSpawner __instance)
        {
            if (__instance.GetComponentInParent<SuperPistolaMarker>() == null) return true;
            CubeShooter.Shoot(__instance);
            return false;
        }
    }
}
