using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultFadeDuration = 1.0f;

    private Coroutine _fadeCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (fadeImage != null)
        {
            // Ensure the image covers the screen and starts transparent
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = false; // By default, don't block clicks
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
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
        
        // Only block clicks if we are fully faded out (black screen)
        fadeImage.raycastTarget = targetAlpha >= 0.95f;
    }
}
