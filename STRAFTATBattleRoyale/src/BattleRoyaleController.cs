using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace STRAFTATBattleRoyale
{
    public class BattleRoyaleController : MonoBehaviour
    {
        public static BattleRoyaleController? Instance { get; private set; }

        private SafeZone _zone = null!;
        private readonly HashSet<PlayerHealth> _alivePlayers = new();

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void OnRoundStart(IEnumerable<PlayerHealth> players)
        {
            if (_zone != null) Destroy(_zone.gameObject);

            _alivePlayers.Clear();
            foreach (var ph in players)
            {
                if (!ph.isKilled) _alivePlayers.Add(ph);
            }

            Plugin.Log.LogInfo($"[BattleRoyale] Ronda iniciada con {_alivePlayers.Count} jugadores.");

            var zoneGo = new GameObject("BattleRoyale_SafeZone");
            zoneGo.transform.position = FindMapCenter();
            _zone = zoneGo.AddComponent<SafeZone>();
            _zone.StartBattleRoyale();

            StartCoroutine(DamageLoop());
        }

        public void OnPlayerDied(PlayerHealth ph)
        {
            if (!_alivePlayers.Remove(ph)) return;

            Plugin.Log.LogInfo($"[BattleRoyale] '{ph.gameObject.name}' eliminado. Quedan {_alivePlayers.Count} jugadores.");
            CheckWinCondition();
        }

        private IEnumerator DamageLoop()
        {
            while (_alivePlayers.Count > 1)
            {
                yield return new WaitForSeconds(1f);
                if (_zone == null) yield break;

                var snapshot = new List<PlayerHealth>(_alivePlayers);
                foreach (var ph in snapshot)
                {
                    if (ph == null || ph.isKilled) continue;
                    if (!_zone.IsOutside(ph.transform.position)) continue;
                    ph.RemoveHealth(Plugin.ZoneDamagePerSecond.Value);
                }
            }
        }

        private void CheckWinCondition()
        {
            if (_alivePlayers.Count == 1)
            {
                foreach (var winner in _alivePlayers)
                    Plugin.Log.LogInfo($"[BattleRoyale] ¡GANADOR: {winner.gameObject.name}!");
            }
            else if (_alivePlayers.Count == 0)
            {
                Plugin.Log.LogInfo("[BattleRoyale] ¡Todos eliminados! Empate.");
            }
        }

        private Vector3 FindMapCenter()
        {
            if (_alivePlayers.Count == 0) return Vector3.zero;
            var sum = Vector3.zero;
            foreach (var ph in _alivePlayers) sum += ph.transform.position;
            var center = sum / _alivePlayers.Count;
            center.y = 0f;
            return center;
        }
    }
}
