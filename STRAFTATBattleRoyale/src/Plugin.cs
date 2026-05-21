using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ComputerysModdingUtilities;
using HeathenEngineering.SteamworksIntegration.API;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: StraftatMod(isVanillaCompatible: false)]

namespace STRAFTATBattleRoyale
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; } = null!;

        public static ConfigEntry<float> ZoneInitialRadius   { get; private set; } = null!;
        public static ConfigEntry<float> ZoneFinalRadius     { get; private set; } = null!;
        public static ConfigEntry<float> ZoneShrinkRate      { get; private set; } = null!;
        public static ConfigEntry<float> ZoneDamagePerSecond { get; private set; } = null!;
        public static ConfigEntry<int>   SecondsUntilZone    { get; private set; } = null!;
        public static ConfigEntry<float> ZoneAlpha           { get; private set; } = null!;
        public static ConfigEntry<string> ZoneColor          { get; private set; } = null!;

        private void Awake()
        {
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            Log = Logger;

            BindConfig();

            PauseManager.OnBeforeSpawn += BattleRoyaleZone.Reset;

            SceneManager.sceneLoaded += OnSceneLoaded;

            Log.LogInfo("================================================");
            Log.LogInfo($"  {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}");
            Log.LogInfo($"  Hora : {System.DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            Log.LogInfo("================================================");

            StartCoroutine(LogSteamName());
        }

        private static System.Collections.IEnumerator LogSteamName()
        {
            float w = 0f;
            while (w < 10f)
            {
                try
                {
                    string? name = Friends.Client.PersonaName;
                    if (!string.IsNullOrEmpty(name)) { Log.LogInfo($"  Jugador : {name}"); yield break; }
                }
                catch { }
                yield return new WaitForSeconds(0.5f);
                w += 0.5f;
            }
        }

        private static readonly HashSet<string> _menuScenes = new() { "MainMenu", "Menu", "main_menu", "Loading" };
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_menuScenes.Contains(scene.name)) return;
            Log.LogInfo($"[Sesión] Modo: '{scene.name}' — {System.DateTime.Now:HH:mm:ss}");
        }

        private void BindConfig()
        {
            ZoneColor          = Config.Bind("Zona", "Color",          "110, 53, 45", "R, G, B");
            ZoneInitialRadius  = Config.Bind("Zona", "RadioInicial",   37.5f,  "Radio inicial (unidades del juego).");
            ZoneFinalRadius    = Config.Bind("Zona", "RadioFinal",     10f,    "Radio minimo.");
            ZoneShrinkRate     = Config.Bind("Zona", "VelocidadCierre",1f,    "Unidades por segundo que se cierra.");
            SecondsUntilZone   = Config.Bind("Zona", "SegundosInicio", 45,    "Segundos hasta que aparece la zona.");
            ZoneDamagePerSecond= Config.Bind("Zona", "DanoPorSegundo", 10f,   "Dano por segundo fuera de la zona.");
            ZoneAlpha          = Config.Bind("Zona", "Transparencia",  0.5f,  "0.0 = invisible, 1.0 = solido.");
        }
    }
}
