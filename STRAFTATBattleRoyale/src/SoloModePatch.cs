using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace STRAFTATBattleRoyale
{
    public static class SoloMode
    {
        public static ConfigEntry<bool> Enabled { get; internal set; } = null!;
    }

    public class SoloModeKeyListener : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (!kb.f8Key.wasPressedThisFrame) return;

            var lobby = FindObjectOfType<SteamLobby>();
            if (lobby == null)
            {
                Plugin.Log.LogWarning("[SoloMode] F8: no estás en un lobby.");
                return;
            }

            string mapName = GetSelectedMapName(lobby);
            if (string.IsNullOrEmpty(mapName))
            {
                Plugin.Log.LogWarning("[SoloMode] F8: selecciona un mapa primero.");
                return;
            }

            Plugin.Log.LogInfo($"[SoloMode] F8 — forzando inicio en: {mapName}");
            typeof(SteamLobby)
                .GetMethod("EnterMap", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(lobby, new object[] { mapName });
        }

        private static string GetSelectedMapName(SteamLobby lobby)
        {
            var mm = typeof(SteamLobby)
                .GetField("mapsManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(lobby) as MapsManager
                ?? Object.FindObjectOfType<MapsManager>();

            if (mm == null) return string.Empty;

            var selected = typeof(MapsManager)
                .GetField("selectedMaps", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(mm);

            if (selected != null)
            {
                int count = (int)(selected.GetType().GetProperty("Count")?.GetValue(selected) ?? 0);
                if (count > 0)
                {
                    var item = selected.GetType().GetProperty("Item")?.GetValue(selected, new object[] { 0 });
                    var name = typeof(Map).GetField("mapName", BindingFlags.Public | BindingFlags.Instance)?.GetValue(item) as string;
                    if (!string.IsNullOrEmpty(name)) return name;
                }
            }

            var all = typeof(MapsManager)
                .GetField("allMaps", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(mm) as object[];
            if (all != null && all.Length > 0)
                return typeof(Map).GetField("mapName", BindingFlags.Public | BindingFlags.Instance)?.GetValue(all[0]) as string ?? string.Empty;

            return string.Empty;
        }
    }

    [HarmonyPatch(typeof(SteamLobby), nameof(SteamLobby.AutomaticStart))]
    public static class Patch_AutomaticStart
    {
        [HarmonyPrefix]
        public static bool Prefix(SteamLobby __instance)
        {
            if (!SoloMode.Enabled.Value) return true;

            var mm = typeof(SteamLobby)
                .GetField("mapsManager", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(__instance) as MapsManager
                ?? Object.FindObjectOfType<MapsManager>();

            if (mm == null) return false;

            var selected = typeof(MapsManager)
                .GetField("selectedMaps", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(mm);

            string mapName = string.Empty;
            if (selected != null)
            {
                int count = (int)(selected.GetType().GetProperty("Count")?.GetValue(selected) ?? 0);
                if (count > 0)
                {
                    var item = selected.GetType().GetProperty("Item")?.GetValue(selected, new object[] { 0 });
                    mapName = typeof(Map).GetField("mapName", BindingFlags.Public | BindingFlags.Instance)?.GetValue(item) as string ?? string.Empty;
                }
            }

            if (string.IsNullOrEmpty(mapName))
            {
                var all = typeof(MapsManager).GetField("allMaps", BindingFlags.Public | BindingFlags.Instance)?.GetValue(mm) as object[];
                if (all != null && all.Length > 0)
                    mapName = typeof(Map).GetField("mapName", BindingFlags.Public | BindingFlags.Instance)?.GetValue(all[0]) as string ?? string.Empty;
            }

            if (string.IsNullOrEmpty(mapName)) return false;

            Plugin.Log.LogInfo($"[SoloMode] AutomaticStart → EnterMap({mapName})");
            typeof(SteamLobby)
                .GetMethod("EnterMap", BindingFlags.Public | BindingFlags.Instance)
                ?.Invoke(__instance, new object[] { mapName });

            return false;
        }
    }
}
