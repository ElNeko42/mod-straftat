using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace STRAFTATNuevasMecanicas
{
    [HarmonyPatch(typeof(PlayerPickup), "SetObjectInHandServer")]
    static class Patch_ExplosivePickup
    {
        const int   Odds         = 13;
        const float CancelGrace  = 0.15f; // ventana para ignorar la doble llamada de FishNet al coger

        static bool _methodsLogged;

        // Cancela cualquier mecha activa â€” se dispara al coger arma nueva (o soltar si el juego llama este mÃ©todo)
        [HarmonyPrefix]
        static void Prefix(PlayerPickup __instance)
        {
            // Una vez: vuelca los mÃ©todos de PlayerPickup para encontrar el de soltar
            if (!_methodsLogged)
            {
                _methodsLogged = true;
                foreach (var m in typeof(PlayerPickup).GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    Plugin.Log.LogInfo($"[PlayerPickup.Method] {m.Name}");
                }
            }

            var ph = __instance.GetComponent<PlayerHealth>();
            if (ph == null) return;
            var fuse = ph.GetComponent<WeaponFuse>();
            if (fuse != null) fuse.RequestCancel();
        }

        [HarmonyPostfix]
        static void Postfix(PlayerPickup __instance)
        {
            if (Random.Range(0, Odds) != 0) return;

            var ph = __instance.GetComponent<PlayerHealth>();
            if (ph == null || ph.isKilled) return;
            if (ph.GetComponent<WeaponFuse>() != null) return; // ya hay mecha activa

            // Buscamos FPC en el mismo GO, padres e hijos
            var fpc = __instance.GetComponent<FirstPersonController>()
                   ?? __instance.GetComponentInParent<FirstPersonController>()
                   ?? __instance.GetComponentInChildren<FirstPersonController>();

            ph.gameObject.AddComponent<WeaponFuse>().Arm(ph, fpc, CancelGrace);
            Plugin.Log.LogInfo($"[ExplosivePickup] Mecha encendida");
        }
    }

    public class WeaponFuse : MonoBehaviour
    {
        const float ExplosionRadius = 5f;
        const float Duration        = 0.5f;

        PlayerHealth          _ph;
        FirstPersonController _fpc;
        Renderer[]            _renderers;
        Color[]               _origColors;
        float                 _elapsed;
        float                 _nextTick;
        float                 _cancelGrace;
        bool                  _cancelRequested;

        public void Arm(PlayerHealth ph, FirstPersonController fpc, float cancelGrace)
        {
            _ph          = ph;
            _fpc         = fpc;
            _cancelGrace = cancelGrace;
        }

        // Llamado desde el Prefix: cancela si la mecha ya pasÃ³ la ventana de gracia
        public void RequestCancel() => _cancelRequested = true;

        void Start()
        {
            var source = _fpc != null
                ? (Component)_fpc
                : (Component)_ph;

            _renderers  = source.GetComponentsInChildren<Renderer>(false);
            _origColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mat = _renderers[i]?.material;
                if (mat == null) continue;
                _origColors[i] = mat.HasProperty("_BaseColor")
                    ? mat.GetColor("_BaseColor") : mat.color;
            }
            Plugin.Log.LogInfo($"[WeaponFuse] Armada â€” {_renderers.Length} renderers");
        }

        void Update()
        {
            if (_ph == null || _ph.isKilled) { Cancel(); return; }

            // Ignoramos cancelaciones durante la ventana de gracia (doble llamada FishNet)
            bool gracePassed = _elapsed > _cancelGrace;
            if (gracePassed && _cancelRequested)
            {
                Plugin.Log.LogInfo("[WeaponFuse] Cancelada â€” arma soltada");
                Cancel();
                return;
            }

            _elapsed += Time.deltaTime;

            if (_elapsed >= _nextTick)
            {
                _nextTick = _elapsed + 0.2f;
                PlayTick();
            }

            float t = Mathf.Clamp01(_elapsed / Duration);
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mat = _renderers[i]?.material;
                if (mat == null) continue;
                var c = Color.Lerp(_origColors[i], Color.red, t);
                mat.color = c;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            }

            if (_elapsed < Duration) return;

            // Tiempo agotado â€” BOOM
            Plugin.Log.LogInfo("[WeaponFuse] BOOM");
            RestoreColors();
            Destroy(this);
            Explode();
        }

        void Cancel()
        {
            RestoreColors();
            Destroy(this);
        }

        void PlayTick()
        {
            if (_fpc?.audio == null) return;
            var clips = _fpc.tauntClip;
            if (clips == null || clips.Length == 0) return;
            AudioClip best = clips[0];
            for (int i = 1; i < Mathf.Min(clips.Length, 9); i++)
                if (clips[i] != null && clips[i].length < best.length) best = clips[i];
            _fpc.audio.PlayOneShot(best, 1f);
        }

        void Explode()
        {
            if (_ph == null || _ph.isKilled) return;

            Vector3 pos = _ph.transform.position;
            var damaged = new HashSet<PlayerHealth>();
            foreach (var col in Physics.OverlapSphere(pos, ExplosionRadius))
            {
                var victim = col.GetComponentInParent<PlayerHealth>();
                if (victim == null || victim.isKilled || !damaged.Add(victim)) continue;
                float dist = Vector3.Distance(pos, victim.transform.position);
                float dmg  = Mathf.Lerp(100f, 10f, dist / ExplosionRadius);
                victim.RemoveHealth(dmg);
                Plugin.Log.LogInfo($"[ExplosivePickup] {victim.name} â€” {dmg:F1} daÃ±o");
            }

            if (!_ph.isKilled)
                _ph.Explode(true, true, "", Vector3.up * 5f, 15f, pos);
        }

        void RestoreColors()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mat = _renderers[i]?.material;
                if (mat == null) continue;
                mat.color = _origColors[i];
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", _origColors[i]);
            }
        }
    }
}
