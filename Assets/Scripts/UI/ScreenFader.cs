using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Flyfe.UI
{
    /// <summary>
    /// A professional-grade Screen Fader.
    /// Uses Render Layers and Overlay mode to ensure it stays on top of gameplay.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class ScreenFader : MonoBehaviour
    {
        public static ScreenFader Instance { get; private set; }

        [Header("UI Components")]
        [SerializeField] private Image fadeImage;
        
        [Header("Professional Settings")]
        [SerializeField] private RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
        [SerializeField] private string sortingLayerName = "UI"; 
        [SerializeField] private int sortingOrder = 50; 
        [SerializeField] private float defaultFadeDuration = 1.0f;

        private Canvas _canvas;
        private Coroutine _fadeCoroutine;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            _canvas = GetComponent<Canvas>();
            ConfigureCanvas();
            SetupFadeImage();
        }

        private void ConfigureCanvas()
        {
            _canvas.renderMode = renderMode;
            _canvas.overrideSorting = true;
            _canvas.sortingLayerName = sortingLayerName;
            _canvas.sortingOrder = sortingOrder;
            
            if (TryGetComponent<GraphicRaycaster>(out var raycaster))
            {
                Destroy(raycaster);
            }
        }

        private void SetupFadeImage()
        {
            if (fadeImage == null) return;

            RectTransform rect = fadeImage.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;
            rect.localScale = Vector3.one;

            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = false; 
            
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }

        public void FadeIn(float duration = -1) => StartFade(0, duration);
        public void FadeOut(float duration = -1) => StartFade(1, duration);

        public IEnumerator FadeInCoroutine(float duration = -1) => FadeRoutine(0, duration);
        public IEnumerator FadeOutCoroutine(float duration = -1) => FadeRoutine(1, duration);

        private void StartFade(float targetAlpha, float duration)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, duration));
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration)
        {
            if (fadeImage == null) yield break;
            
            float startAlpha = fadeImage.color.a;
            float elapsed = 0;
            float d = duration < 0 ? defaultFadeDuration : duration;

            while (elapsed < d)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / d);
                fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, a);
                yield return null;
            }

            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
        }
    }
}
