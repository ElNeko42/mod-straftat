using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace STRAFTATBattleRoyale
{
    public static class CountdownHUD
    {
        static GameObject _canvasGo;
        static TextMeshProUGUI _text;

        public static void Show(int seconds)
        {
            EnsureCreated();
            _canvasGo.SetActive(true);

            if (seconds > 0)
            {
                _text.text     = seconds.ToString();
                _text.color    = Color.Lerp(Color.yellow, Color.red, 1f - seconds / 10f);
                _text.fontSize = Mathf.Lerp(60f, 120f, 1f - seconds / 10f);
            }
            else
            {
                _text.text     = "¡ZONA ACTIVA!";
                _text.color    = Color.red;
                _text.fontSize = 80f;
            }
        }

        public static void Hide()
        {
            if (_canvasGo) _canvasGo.SetActive(false);
        }

        static void EnsureCreated()
        {
            if (_canvasGo) return;

            _canvasGo = new GameObject("BR_CountdownHUD");
            Object.DontDestroyOnLoad(_canvasGo);

            var canvas        = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            _canvasGo.AddComponent<CanvasScaler>();
            _canvasGo.AddComponent<GraphicRaycaster>();

            var textGo = new GameObject("CountdownText");
            textGo.transform.SetParent(_canvasGo.transform, false);

            _text           = textGo.AddComponent<TextMeshProUGUI>();
            _text.alignment = TextAlignmentOptions.Center;
            _text.fontSize  = 80f;
            _text.color     = Color.yellow;
            _text.fontStyle = FontStyles.Bold;

            var rect        = textGo.GetComponent<RectTransform>();
            rect.anchorMin  = new Vector2(0.5f, 0.6f);
            rect.anchorMax  = new Vector2(0.5f, 0.6f);
            rect.pivot      = new Vector2(0.5f, 0.5f);
            rect.sizeDelta  = new Vector2(400f, 150f);
            rect.anchoredPosition = Vector2.zero;
        }
    }
}
