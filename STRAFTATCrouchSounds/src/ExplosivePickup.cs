using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace STRAFTATCrouchSounds
{
    [HarmonyPatch(typeof(PlayerPickup), "SetObjectInHandServer")]
    static class Patch_ExplosivePickup
    {
        const float ExplosionRadius = 5f;

        [HarmonyPostfix]
        static void Postfix(PlayerPickup __instance)
        {
            if (Random.Range(0, 10) != 0) return; // 1 de cada 10

            var ph = __instance.GetComponent<PlayerHealth>();
            if (ph == null || ph.isKilled) return;

            Vector3 pos = __instance.transform.position;
            Plugin.Log.LogInfo($"[ExplosivePickup] BOOM en {pos}");

            // Daño de área — HashSet para no golpear al mismo jugador dos veces
            var damaged = new HashSet<PlayerHealth>();
            foreach (var col in Physics.OverlapSphere(pos, ExplosionRadius))
            {
                var victim = col.GetComponentInParent<PlayerHealth>();
                if (victim == null || victim.isKilled || !damaged.Add(victim)) continue;

                float dist = Vector3.Distance(pos, victim.transform.position);
                float dmg  = Mathf.Lerp(100f, 10f, dist / ExplosionRadius);
                victim.RemoveHealth(dmg);
                Plugin.Log.LogInfo($"[ExplosivePickup] {victim.name} — {dmg:F1} daño (dist {dist:F1}m)");
            }

            // Garantiza la muerte del que cogió el arma aunque el OverlapSphere no lo pillara
            if (!ph.isKilled)
                ph.Explode(true, true, "", Vector3.up * 5f, 15f, pos);
        }
    }
}
