using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] private float waitTime = 3.0f;
    [SerializeField] private Color themeColor = new Color(0f, 1f, 1f); // Cyan
    [SerializeField] private GameObject victoryUiPrefab;

    private bool _hasWon = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_hasWon && other.CompareTag(Tags.Player))
        {
            _hasWon = true;
            if (victoryUiPrefab != null)
            {
                Instantiate(victoryUiPrefab);
            }
            StartCelebration();
        }
    }

    private void StartCelebration()
    {
        if (RecordingService.Instance != null)
        {
            RecordingService.Instance.ForceResetToPresent();
        }
        
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