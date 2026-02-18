using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    public float waitTime = 3.0f;
    public Color themeColor = new Color(0f, 1f, 1f); // Cyan

    private bool _hasWon = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_hasWon && other.CompareTag("Player"))
        {
            _hasWon = true;
            CreateVictoryUI();
            StartCelebration();
        }
    }

    private void CreateVictoryUI()
    {
        GameObject canvasObj = new GameObject("VictoryCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("VictoryText");
        textObj.transform.SetParent(panelObj.transform);
        Text winText = textObj.AddComponent<Text>();
        winText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        winText.text = "MVP COMPLETE! ✨";
        winText.fontSize = 80;
        winText.fontStyle = FontStyle.Bold;
        winText.color = themeColor;
        winText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = textRect.anchorMax = textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(1000, 300);

        textObj.AddComponent<Shadow>().effectDistance = new Vector2(4, -4);
    }

    private void StartCelebration()
    {
        RecordManager.Instance.ForceResetToPresent();
        Time.timeScale = 0.3f;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player && player.TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
        Invoke(nameof(LoadNextLevel), waitTime * Time.timeScale);
    }

    private void LoadNextLevel()
    {
        Time.timeScale = 1.0f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(next < SceneManager.sceneCountInBuildSettings ? next : 0);
    }
}