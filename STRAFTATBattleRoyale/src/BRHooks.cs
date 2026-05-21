using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace STRAFTATBattleRoyale
{
    [HarmonyPatch(typeof(PauseManager), nameof(PauseManager.InvokeRoundStarted))]
    public static class Patch_RoundStart
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (BattleRoyaleController.Instance == null)
            {
                var go = new GameObject("BattleRoyaleController");
                go.AddComponent<BattleRoyaleController>();
            }

            var players = new List<PlayerHealth>(Object.FindObjectsOfType<PlayerHealth>());
            BattleRoyaleController.Instance?.OnRoundStart(players);
        }
    }

    [HarmonyPatch(typeof(PlayerHealth), nameof(PlayerHealth.ChangeKilledState))]
    public static class Patch_PlayerDeath
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerHealth __instance, bool tempBool)
        {
            if (tempBool)
                BattleRoyaleController.Instance?.OnPlayerDied(__instance);
        }
    }
}
