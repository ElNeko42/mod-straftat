using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

namespace STRAFTATNuevasMecanicas
{
    public class ZMarkTracker : MonoBehaviour
    {
        const float CooldownDuration = 5f;

        FirstPersonController _fpc;
        NetworkObject         _nob;
        PlayerHealth          _ph;
        bool                  _markActive;
        Vector3               _markPosition;
        GameObject            _markVisual;
        float                 _cooldown;

        GUIStyle _hudBox;

        void Start()
        {
            _fpc = GetComponent<FirstPersonController>();
            _nob = GetComponent<NetworkObject>();
            _ph  = GetComponent<PlayerHealth>();
        }

        void Update()
        {
            if (_ph != null && _ph.isKilled && _markActive) ClearMark();

            if (_cooldown > 0f) _cooldown -= Time.deltaTime;

            if (_nob != null && !_nob.IsOwner) return;

            if (IsMarkKeyPressed())
            {
                if (_markActive)          Teleport();
                else if (_cooldown <= 0f) PlaceMark();
            }
        }

        bool IsMarkKeyPressed()
        {
            var val = Plugin.MarkKey.Value.Trim();

            // Botones del ratón: Mouse0-4
            if (val.StartsWith("Mouse", System.StringComparison.OrdinalIgnoreCase)
                && int.TryParse(val.Substring(5), out int btn))
            {
                var m = Mouse.current;
                if (m == null) return false;
                return btn switch
                {
                    0 => m.leftButton.wasPressedThisFrame,
                    1 => m.rightButton.wasPressedThisFrame,
                    2 => m.middleButton.wasPressedThisFrame,
                    3 => m.forwardButton.wasPressedThisFrame,
                    4 => m.backButton.wasPressedThisFrame,
                    _ => false
                };
            }

            // Teclado
            if (System.Enum.TryParse<Key>(val, true, out var key))
                return Keyboard.current[key].wasPressedThisFrame;

            return false;
        }

        void PlaceMark()
        {
            _markPosition = _fpc.transform.position;
            _markActive   = true;
            if (_markVisual != null) Destroy(_markVisual);
            _markVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _markVisual.transform.position   = _markPosition + Vector3.up * 0.1f;
            _markVisual.transform.localScale  = Vector3.one * 0.4f;
            Destroy(_markVisual.GetComponent<Collider>());
            var mat = _markVisual.GetComponent<Renderer>().material;
            mat.color = Color.cyan;
            if (mat.HasProperty("_BaseColor"))    mat.SetColor("_BaseColor", Color.cyan);
            if (mat.HasProperty("_EmissionColor")) { mat.SetColor("_EmissionColor", Color.cyan); mat.EnableKeyword("_EMISSION"); }
        }

        void Teleport()
        {
            Vector3 dest = _markPosition;
            ClearMark();
            _cooldown = CooldownDuration;
            var cc = _fpc.GetComponent<CharacterController>();
            if (cc != null && cc.enabled) { cc.enabled = false; _fpc.transform.position = dest; cc.enabled = true; }
            else _fpc.transform.position = dest;
        }

        void ClearMark()
        {
            _markActive = false;
            if (_markVisual != null) { Destroy(_markVisual); _markVisual = null; }
        }

        void OnDisable()  => ClearMark();
        void OnDestroy()  => ClearMark();

        void OnGUI()
        {
            if (_nob != null && !_nob.IsOwner) return;
            if (_ph  != null && _ph.isKilled)  return;
            if (Cursor.lockState != CursorLockMode.Locked) return; // solo en juego

            if (_hudBox == null)
            {
                _hudBox = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 15, fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 6, 6)
                };
                _hudBox.normal.textColor = Color.white;
            }

            var key = SettingsUI.KeyDisplayName(Plugin.MarkKey.Value);
            string msg = _cooldown > 0f  ? $"Mark ready in {Mathf.Ceil(_cooldown):0}s"
                       : _markActive      ? $"[{key}] Teleport to mark"
                                          : $"[{key}] Place mark";

            GUI.Box(new Rect(10f, Screen.height - 44f, 210f, 34f), msg, _hudBox);
        }
    }
}
