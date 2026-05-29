using UnityEngine;
using UnityEngine.SceneManagement;

namespace STRAFTATNuevasMecanicas
{
    public class SettingsUI : MonoBehaviour
    {
        bool _showModal;
        bool _rebinding;

        GUIStyle  _openBtn;
        GUIStyle  _modalBg;
        GUIStyle  _titleLbl;
        GUIStyle  _closeBtn;
        GUIStyle  _sectionLbl;
        GUIStyle  _rowLbl;
        GUIStyle  _keyBox;
        GUIStyle  _actionBtn;
        GUIStyle  _rebindBox;
        Texture2D _overlayTex;

        void Update()
        {
            // Cierra el modal si el juego se reanuda
            if (_showModal && !IsMenuScene() && Cursor.lockState == CursorLockMode.Locked)
            {
                _showModal = _rebinding = false;
            }
        }

        void OnGUI()
        {
            InitStyles();

            bool show = IsMenuScene() || Cursor.lockState != CursorLockMode.Locked;
            if (!show) return;

            // Botón de apertura (esquina inferior izquierda)
            if (!_showModal)
            {
                if (GUI.Button(new Rect(10f, Screen.height - 44f, 170f, 34f),
                               "Wild Mechanics  >", _openBtn))
                    _showModal = true;
            }
            else
            {
                DrawModal();
            }
        }

        void DrawModal()
        {
            const float MW = 420f, MH = 240f;
            float mx = (Screen.width  - MW) * 0.5f;
            float my = (Screen.height - MH) * 0.5f;

            // Overlay oscuro
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _overlayTex);

            // Fondo del modal
            GUI.Box(new Rect(mx, my, MW, MH), "", _modalBg);

            // Título
            GUI.Label(new Rect(mx + 16, my + 12, MW - 60, 28), "Wild Mechanics", _titleLbl);

            // Botón cierre (oculto al reasignar para evitar que un clic cierre Y asigne)
            if (!_rebinding)
            {
                if (GUI.Button(new Rect(mx + MW - 40, my + 10, 30, 28), "x", _closeBtn))
                {
                    _showModal = false;
                    return;
                }
            }

            // Línea divisora
            GUI.Box(new Rect(mx + 12, my + 48, MW - 24, 2), "", _modalBg);

            // ── Sección: Recall Mark ──────────────────────────────────────
            GUI.Label(new Rect(mx + 16, my + 58, MW - 30, 22), "Recall Mark", _sectionLbl);

            GUI.Label(new Rect(mx + 28, my + 92, 90f, 26f), "Mark key:", _rowLbl);

            if (_rebinding)
            {
                GUI.Box(new Rect(mx + 122, my + 90, 270f, 28f),
                        "Press key or mouse button...  (ESC = cancel)", _rebindBox);

                var e = Event.current;
                if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
                {
                    if (e.keyCode != KeyCode.Escape)
                        Plugin.MarkKey.Value = e.keyCode.ToString();
                    _rebinding = false;
                    e.Use();
                }
                else if (e.type == EventType.MouseDown)
                {
                    Plugin.MarkKey.Value = $"Mouse{e.button}";
                    _rebinding = false;
                    e.Use();
                }
            }
            else
            {
                GUI.Box(new Rect(mx + 122, my + 90, 90f, 28f),
                        KeyDisplayName(Plugin.MarkKey.Value), _keyBox);

                if (GUI.Button(new Rect(mx + 220, my + 90, 80f, 28f), "Change", _actionBtn))
                    _rebinding = true;
            }

            // Espacio para mecánicas futuras
            GUI.Label(new Rect(mx + 16, my + 158, MW - 30, 40),
                      "More settings will appear here as new mechanics are added.",
                      _rowLbl);
        }

        // ── helpers ───────────────────────────────────────────────────────

        public static string KeyDisplayName(string val)
        {
            return val.Trim().ToUpper() switch
            {
                "MOUSE0" => "Left Click",
                "MOUSE1" => "Right Click",
                "MOUSE2" => "Middle Click",
                "MOUSE3" => "Mouse 4",
                "MOUSE4" => "Mouse 5",
                var v   => v
            };
        }

        static bool IsMenuScene()
        {
            var n = SceneManager.GetActiveScene().name;
            return n == "MainMenu" || n == "Menu" || n == "main_menu";
        }

        void InitStyles()
        {
            if (_openBtn != null) return;

            _overlayTex = MakeTex(new Color(0f, 0f, 0f, 0.72f));

            _openBtn = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _modalBg = new GUIStyle(GUI.skin.box);
            _modalBg.normal.background = MakeTex(new Color(0.12f, 0.12f, 0.18f, 0.97f));

            _titleLbl = new GUIStyle(GUI.skin.label) { fontSize = 19, fontStyle = FontStyle.Bold };
            _titleLbl.normal.textColor = Color.white;

            _closeBtn = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            _sectionLbl = new GUIStyle(GUI.skin.label) { fontSize = 14, fontStyle = FontStyle.Bold };
            _sectionLbl.normal.textColor = new Color(0.4f, 0.85f, 1f);

            _rowLbl = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _rowLbl.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            _keyBox = new GUIStyle(GUI.skin.box) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            _keyBox.normal.textColor = Color.yellow;

            _actionBtn = new GUIStyle(GUI.skin.button) { fontSize = 13 };

            _rebindBox = new GUIStyle(GUI.skin.box) { fontSize = 12 };
            _rebindBox.normal.textColor = Color.yellow;
        }

        static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
    }
}
