using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace STRAFTATBattleRoyale
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        public static ManualLogSource Log { get; private set; } = null!;

        public static ConfigEntry<float> ZoneInitialRadius   { get; private set; } = null!;
        public static ConfigEntry<float> ZoneFinalRadius     { get; private set; } = null!;
        public static ConfigEntry<float> ZoneShrinkDuration  { get; private set; } = null!;
        public static ConfigEntry<float> ZoneDamagePerSecond { get; private set; } = null!;
        public static ConfigEntry<int>   ZoneShrinkPhases    { get; private set; } = null!;
        public static ConfigEntry<float> ZonePhaseWaitTime   { get; private set; } = null!;

        private Harmony _harmony = null!;

        private void Awake()
        {
            Instance = this;
            Log      = Logger;

            BindConfig();

            _harmony = new Harmony(PluginInfo.GUID);
            _harmony.PatchAll();

            gameObject.AddComponent<SoloModeKeyListener>();

            Log.LogInfo($"{PluginInfo.NAME} v{PluginInfo.VERSION} cargado correctamente.");
            if (SoloMode.Enabled.Value)
                Log.LogInfo("[SoloMode] Modo solo ACTIVADO — pulsa F8 en el lobby.");
        }

        private void BindConfig()
        {
            ZoneInitialRadius   = Config.Bind("Zona",   "RadioInicial",     120f, "Radio inicial de la zona (metros).");
            ZoneFinalRadius     = Config.Bind("Zona",   "RadioFinal",         5f, "Radio minimo de la zona.");
            ZoneShrinkDuration  = Config.Bind("Zona",   "DuracionReduccion", 60f, "Segundos por fase de cierre.");
            ZoneShrinkPhases    = Config.Bind("Zona",   "NumFases",            4, "Numero de fases.");
            ZonePhaseWaitTime   = Config.Bind("Zona",   "EsperaPorFase",     30f, "Espera antes de cerrar cada fase.");
            ZoneDamagePerSecond = Config.Bind("Dano",   "DanoPorSegundo",    10f, "Dano por segundo fuera de zona.");

            SoloMode.Enabled    = Config.Bind("ModoSolo", "Activado", true, "Permite iniciar partida con 1 jugador.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
