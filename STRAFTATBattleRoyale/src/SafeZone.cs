using System.Collections;
using UnityEngine;

namespace STRAFTATBattleRoyale
{
    public class SafeZone : MonoBehaviour
    {
        public float CurrentRadius { get; private set; }
        public bool  IsShrinking   { get; private set; }
        public int   CurrentPhase  { get; private set; }

        private GameObject _outerRing = null!;
        private GameObject _innerRing = null!;
        private float _targetRadius;
        private float _phaseRadiusStep;

        private void Awake()
        {
            CurrentRadius    = Plugin.ZoneInitialRadius.Value;
            _phaseRadiusStep = (Plugin.ZoneInitialRadius.Value - Plugin.ZoneFinalRadius.Value)
                               / Plugin.ZoneShrinkPhases.Value;
            CreateVisuals();
            UpdateVisuals();
        }

        public void StartBattleRoyale()
        {
            StartCoroutine(ZoneCycleCoroutine());
        }

        private IEnumerator ZoneCycleCoroutine()
        {
            int totalPhases = Plugin.ZoneShrinkPhases.Value;
            for (int phase = 1; phase <= totalPhases; phase++)
            {
                CurrentPhase  = phase;
                _targetRadius = Mathf.Max(
                    Plugin.ZoneFinalRadius.Value,
                    Plugin.ZoneInitialRadius.Value - _phaseRadiusStep * phase
                );

                Plugin.Log.LogInfo($"[BattleRoyale] Fase {phase}/{totalPhases} — esperando {Plugin.ZonePhaseWaitTime.Value}s.");
                ShowTargetRing(_targetRadius);
                yield return new WaitForSeconds(Plugin.ZonePhaseWaitTime.Value);
                yield return StartCoroutine(ShrinkCoroutine(CurrentRadius, _targetRadius, Plugin.ZoneShrinkDuration.Value));
            }

            Plugin.Log.LogInfo("[BattleRoyale] Zona completamente cerrada.");
        }

        private IEnumerator ShrinkCoroutine(float from, float to, float duration)
        {
            IsShrinking = true;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                CurrentRadius = Mathf.Lerp(from, to, elapsed / duration);
                UpdateVisuals();
                yield return null;
            }
            CurrentRadius = to;
            UpdateVisuals();
            IsShrinking = false;
        }

        private void CreateVisuals()
        {
            _innerRing = CreateRingMesh("SafeZone_Inner", new Color(0f, 0.5f, 1f, 0.25f));
            _outerRing = CreateRingMesh("SafeZone_Outer", new Color(1f, 0.1f, 0.1f, 0.15f));
            _outerRing.SetActive(false);
        }

        private GameObject CreateRingMesh(string goName, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = goName;
            go.transform.SetParent(transform, false);
            Destroy(go.GetComponent<Collider>());
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = color;
            go.GetComponent<Renderer>().material = mat;
            return go;
        }

        private void UpdateVisuals()
        {
            float d = CurrentRadius * 2f;
            _innerRing.transform.localScale = new Vector3(d, 100f, d);
        }

        private void ShowTargetRing(float targetRadius)
        {
            float d = targetRadius * 2f;
            _outerRing.transform.localScale = new Vector3(d, 100f, d);
            _outerRing.SetActive(true);
        }

        public bool IsOutside(Vector3 worldPos)
        {
            var flat = new Vector2(worldPos.x - transform.position.x, worldPos.z - transform.position.z);
            return flat.magnitude > CurrentRadius;
        }
    }
}
